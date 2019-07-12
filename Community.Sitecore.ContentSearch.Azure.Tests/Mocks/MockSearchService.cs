using Sitecore.ContentSearch.Azure;
using Sitecore.ContentSearch.Azure.Http;
using Sitecore.ContentSearch.Azure.Models;
using Sitecore.ContentSearch.Azure.Schema;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Community.Sitecore.ContentSearch.Azure.Tests.Mocks
{
    class MockSearchService : ISearchService
    {
        public MockSearchService(ICloudSearchIndexSchema schema)
        {
            this.Schema = schema;
        }

        public List<List<Dictionary<string, object>>> PostedBatches = new List<List<Dictionary<string, object>>>();

        public string Name => throw new NotImplementedException();

        public ICloudSearchIndexSchema Schema { get; private set; }

        public void Cleanup()
        {
            throw new NotImplementedException();
        }

        public IndexStatistics GetStatistics()
        {
            throw new NotImplementedException();
        }

        public void PostDocuments(ICloudBatch batch)
        {
            if (batch is TransparentCloudBatch transparentBatch)
                PostedBatches.Add(transparentBatch.Documents.ToList());
        }

        public string Search(string expression)
        {
            throw new NotImplementedException();
        }
    }
}
