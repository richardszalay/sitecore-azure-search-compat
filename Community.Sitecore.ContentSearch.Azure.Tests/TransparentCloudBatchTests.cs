using Community.Sitecore.ContentSearch.Azure;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using Sitecore.ContentSearch.Azure;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Community.Sitecore.ContentSearch.Azure.Tests
{
    [TestClass]
    public class TransparentCloudBatchTests
    {
        private TransparentCloudBatchBuilder sut = new TransparentCloudBatchBuilder();

        [TestMethod]
        public void IsFull_returns_true_if_MaxDocuments_met()
        {
            sut.MaxDocuments = 1;

            sut.AddDocument(new CloudSearchDocument(new Dictionary<string, object>
            {
                ["field1"] = "First value",
                ["field2"] = "Second value"
            }, SearchAction.Upload));

            Assert.IsTrue(sut.IsFull);
        }

        [TestMethod]
        public void Release_returns_TransparentCloudBatch()
        {
            sut.MaxDocuments = 1;

            sut.AddDocument(new CloudSearchDocument(new Dictionary<string, object>
            {
                ["field1"] = "First value",
                ["field2"] = "Second value"
            }, SearchAction.Upload));

            var result = sut.Release();

            Assert.IsInstanceOfType(result, typeof(TransparentCloudBatch));
            Assert.AreEqual(1, ((TransparentCloudBatch)result).Documents.Count);
        }

        [TestMethod]
        public void Release_strips_null_field_values()
        {
            sut.AddDocument(new CloudSearchDocument(new Dictionary<string, object>
            {
                ["field1"] = null,
                ["field2"] = "Second value"
            }, SearchAction.MergeOrUpload));

            var result = (sut.Release() as TransparentCloudBatch).Documents.ToList();

            CollectionAssert.DoesNotContain(result[0].Keys, "field1");
        }

        [TestMethod]
        public void Release_strips_null_array_elements()
        {
            sut.AddDocument(new CloudSearchDocument(new Dictionary<string, object>
            {
                ["field1"] = new List<string> { null, "second" },
                ["field2"] = "Second value"
            }, SearchAction.MergeOrUpload));

            var result = (sut.Release() as TransparentCloudBatch).Documents.ToList();

            CollectionAssert.AreEqual(new[] { "second" }, result[0]["field1"] as ICollection);
        }

        [TestMethod]
        public void Release_strips_null_dictionary_elements()
        {
            sut.AddDocument(new CloudSearchDocument(new Dictionary<string, object>
            {
                ["field1"] = new Dictionary<string, string>
                {
                    ["child1"] = null,
                    ["child2"] = "child value"
                },
                ["field2"] = "Second value"
            }, SearchAction.MergeOrUpload));

            var result = (sut.Release() as TransparentCloudBatch).Documents.ToList();

            CollectionAssert.DoesNotContain(((Dictionary<string, string>)result[0]["field1"]).Keys, "child1");
        }

        [TestMethod]
        public void Release_appends_search_action_field()
        {
            sut.AddDocument(new CloudSearchDocument(new Dictionary<string, object>
            {
                ["field1"] = "First value",
                ["field2"] = "Second value"
            }, SearchAction.MergeOrUpload));

            var result = sut.Release() as TransparentCloudBatch;

            Assert.AreEqual("mergeOrUpload", result.Documents.First()["@search.action"]);
        }

        [TestMethod]
        public void Clear_resets_accumulators()
        {
            sut.MaxDocuments = 1;

            sut.AddDocument(new CloudSearchDocument(new Dictionary<string, object>
            {
                ["field1"] = "First value",
                ["field2"] = "Second value"
            }, SearchAction.Upload));

            sut.Clear();

            Assert.IsFalse(sut.IsFull);
            Assert.AreEqual(0, sut.Count());
            Assert.AreEqual(0, ((TransparentCloudBatch)sut.Release()).Documents.Count);
        }


        private int GetSerializedSize(IEnumerable<Dictionary<string, object>> documents) => new CloudBatch(documents).GetJson().Length;
    }
}
