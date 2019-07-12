using Sitecore.ContentSearch.Azure;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Community.Sitecore.ContentSearch.Azure
{
    public class TransparentCloudBatchBuilder : ICloudBatchBuilder
    {
        private List<CloudSearchDocument> documents = new List<CloudSearchDocument>();

        public int MaxDocuments { get; set; }

        public bool IsFull => documents.Count >= MaxDocuments;

        public void AddDocument(CloudSearchDocument document)
        {
            documents.Add(document);
        }

        public void Clear()
        {
            documents.Clear();
        }

        public ICloudBatch Release() => new TransparentCloudBatch(documents.Select(CreateDocumentDictionary).ToList());

        private Dictionary<string, object> CreateDocumentDictionary(CloudSearchDocument document)
        {
            var dictionary = document.Fields
                .Select(kvp => new KeyValuePair<string, object>(kvp.Key, SimplifyFieldValue(kvp.Value)))
                .Where(kvp => HasValidValue(kvp.Value))
                .ToDictionary(kvp => kvp.Key, kvp => kvp.Value);

            dictionary.Add("@search.action", FormatSearchAction(document.Action));

            return dictionary;
        }

        static bool HasValidValue(object value)
        {
            if (value is IEnumerable<object> enumerable)
                return enumerable.Any();

            return value != null;
        }

        static object SimplifyFieldValue(object value)
        {
            if (value is IEnumerable<object> enumerable)
            {
                return enumerable.Where(element => element != null).ToList();
            }

            if (value is IDictionary<string, string> dictionary)
            {
                return dictionary
                    .Where(kvp => kvp.Value != null)
                    .ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
            }

            return value;
        }

        static string FormatSearchAction(SearchAction action) =>
            char.ToLowerInvariant(action.ToString()[0]) + action.ToString().Substring(1);

        public IEnumerator<CloudSearchDocument> GetEnumerator() => documents.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => documents.GetEnumerator();
    }
}
