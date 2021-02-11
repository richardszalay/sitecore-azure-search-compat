using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Sitecore.ContentSearch;
using Sitecore.ContentSearch.Azure;
using Sitecore.ContentSearch.Azure.Http;
using Sitecore.ContentSearch.Azure.Models;
using Sitecore.ContentSearch.Azure.Schema;
using Sitecore.ContentSearch.Diagnostics;

namespace Community.Sitecore.ContentSearch.Azure
{
    /// <summary>
    /// Honors the following limits of the Azure Search API:
    /// 
    /// 1. Fields marked as Filterable/Facetable/Sortable are truncated at 32766 bytes
    /// 2. Batches are split into smaller sizes if the data size is greater than 16mb
    /// </summary>
    public class TruncatingSearchService : ISearchService, IProvideAvailabilityManager, ISearchServiceConnectionInitializable, ISearchIndexInitializable, IDisposable, ISearchServiceSchemaSyncNotification
    {
        const int AzureFieldSizeLimit = 32766;
        const int AzureBatchSizeLimit = 16 * 1024 * 1024;

        private readonly ISearchService innerService;

        public int MaxFieldSize { get; set; } = AzureFieldSizeLimit;

        public int MaxBatchSize { get; set; } = AzureBatchSizeLimit;

        public TruncatingSearchService(ISearchService innerService)
        {
            this.innerService = innerService;
        }

        public void PostDocuments(ICloudBatch batch)
        {
            if (batch is TransparentCloudBatch mutableBatch)
            {
                foreach (var document in mutableBatch.Documents)
                {
                    var documentId = GetDocumentId(document);

                    var keys = document.Keys.ToList();
                    foreach (var key in keys)
                    {
                        var fieldName = key;
                        var fieldSchema = Schema.GetFieldByCloudName(fieldName);

                        if (!IsFieldSizeLimited(fieldSchema))
                            continue;

                        var estimatedSize = FieldDataUtil.EstimateSize(document[key]);

                        if (estimatedSize >= MaxFieldSize)
                        {
                            document[key] = FieldDataUtil.Truncate(document[key], estimatedSize - MaxFieldSize);

                            CrawlingLog.Log.Info($"Field '{key}' on document '{documentId}' is too large and was truncated. Disable the Filterable, Sortable, and Facetable flags on this field to prevent truncation.");
                        }
                    }
                }

                var subBatches = SplitBatch(mutableBatch).ToList();

                if (subBatches.Count > 1)
                {
                    CrawlingLog.Log.Info($"Batch was too large and was split into {subBatches.Count} smaller batches");
                }

                foreach (var subBatch in subBatches)
                {
                    innerService.PostDocuments(subBatch);
                }

                return;
            }

            // TODO: Configuration warning
            innerService.PostDocuments(batch);
        }

        private IEnumerable<ICloudBatch> SplitBatch(TransparentCloudBatch batch)
        {
            var isBatchTooLarge = Encoding.UTF8.GetByteCount(batch.GetJson()) >= MaxBatchSize;

            if (!isBatchTooLarge || batch.Documents.Count == 1)
            {
                yield return batch;
                yield break;
            }

            int subBatchSize = batch.Documents.Count() / 2;

            var first = new TransparentCloudBatch(batch.Documents.Take(subBatchSize));
            var second = new TransparentCloudBatch(batch.Documents.Skip(subBatchSize));

            foreach (var subBatch in SplitBatch(first).Concat(SplitBatch(second)))
                yield return subBatch;
        }

        public static string GetDocumentId(Dictionary<string, object> document) =>
            document.TryGetValue("uniqueid_1", out var uniqueid) ? uniqueid.ToString() : "UNKNOWN";

        private bool IsFieldSizeLimited(IndexedField field) =>
            field != null && (field.Filterable || field.Sortable || field.Facetable);

        public string Name => innerService.Name;

        public ICloudSearchIndexSchema Schema => innerService.Schema;

        public void Cleanup() => innerService.Cleanup();

        public IndexStatistics GetStatistics() => innerService.GetStatistics();

        public string Search(string expression) => innerService.Search(expression);

        public ISearchServiceAvailabilityManager AvailabilityManager =>
            (innerService as IProvideAvailabilityManager).AvailabilityManager;

        void ISearchServiceConnectionInitializable.Initialize(string indexName, string connectionString) =>
            ((ISearchServiceConnectionInitializable)innerService).Initialize(indexName, connectionString);

        void ISearchIndexInitializable.Initialize(ISearchIndex searchIndex) =>
            ((ISearchIndexInitializable)innerService).Initialize(searchIndex);

        void IDisposable.Dispose() =>
            ((IDisposable)innerService).Dispose();

        event EventHandler ISearchServiceSchemaSyncNotification.SchemaSynced
        {
            add { ((ISearchServiceSchemaSyncNotification)innerService).SchemaSynced += value; }
            remove { ((ISearchServiceSchemaSyncNotification)innerService).SchemaSynced -= value; }
        }
    }
}
