using Community.Sitecore.ContentSearch.Azure.Tests.Mocks;
using Community.Sitecore.ContentSearch.Azure;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Sitecore.ContentSearch.Azure.Models;
using System.Collections;
using System.Collections.Generic;

namespace Community.Sitecore.ContentSearch.Azure.Tests
{
    [TestClass]
    public class TruncatingSearchServiceTests
    {
        private readonly TruncatingSearchService sut;
        private readonly MockSchema schema;
        private readonly MockSearchService mockService;

        public TruncatingSearchServiceTests()
        {
            schema = new MockSchema();
            mockService = new MockSearchService(schema);
            sut = new TruncatingSearchService(mockService);
        }

        [TestMethod]
        public void Truncates_sortable_fields()
        {
            sut.MaxFieldSize = 5;
            schema.Add(new IndexedField("big", "string", false, true, true, true, true, true));

            sut.PostDocuments(new TransparentCloudBatch(new[]
            {
                new Dictionary<string, object>
                {
                    ["uniqueid_1"] = "d1",
                    ["big"] = "abc def"
                }
            }));

            Assert.AreEqual("abc", mockService.PostedBatches[0][0]["big"]);
        }

        [TestMethod]
        public void Does_not_truncate_purely_searchable_fields()
        {
            sut.MaxFieldSize = 5;
            schema.Add(new IndexedField("big", "string", false, true, true, false, false, false));

            sut.PostDocuments(new TransparentCloudBatch(new[]
            {
                new Dictionary<string, object>
                {
                    ["uniqueid_1"] = "d1",
                    ["big"] = "abc def"
                }
            }));

            Assert.AreEqual("abc def", mockService.PostedBatches[0][0]["big"]);
        }

        [TestMethod]
        public void Truncates_complex_field_types()
        {
            sut.MaxFieldSize = 8;
            schema.Add(new IndexedField("big", "string", false, true, true, true, true, true));

            sut.PostDocuments(new TransparentCloudBatch(new[]
            {
                new Dictionary<string, object>
                {
                    ["uniqueid_1"] = "d1",
                    ["big"] = new Dictionary<string, object>
                    {
                        ["abc"] = new [] { "aa", "bb cc" }
                    }
                }
            }));

            var result = mockService.PostedBatches[0][0]["big"] as Dictionary<string, object>;

            CollectionAssert.AreEqual(new[] { "aa", "bb" }, result["abc"] as ICollection);
        }

        [TestMethod]
        public void Splits_batches_into_smaller_sizes_when_required()
        {
            sut.MaxBatchSize = 250;
            schema.Add(new IndexedField("big", "string", false, true, true, true, true, true));

            sut.PostDocuments(new TransparentCloudBatch(new[]
            {
                new Dictionary<string, object>
                {
                    ["uniqueid_1"] = "d1",
                    ["big"] = new Dictionary<string, object>
                    {
                        ["abc"] = new [] { "aa", "bb cc" }
                    }
                },
                new Dictionary<string, object>
                {
                    ["uniqueid_1"] = "d2",
                    ["big"] = new Dictionary<string, object>
                    {
                        ["abc"] = new [] { "aa", "bb cc" }
                    }
                }
            }));

            Assert.AreEqual(2, mockService.PostedBatches.Count);

            Assert.AreEqual(1, mockService.PostedBatches[0].Count);
            Assert.AreEqual("d1", mockService.PostedBatches[0][0]["uniqueid_1"]);

            Assert.AreEqual(1, mockService.PostedBatches[1].Count);
            Assert.AreEqual("d2", mockService.PostedBatches[1][0]["uniqueid_1"]);
        }

        [TestMethod]
        public void Does_not_split_batches_into_smaller_sizes_when_not_required()
        {
            sut.MaxBatchSize = 1000;
            schema.Add(new IndexedField("big", "string", false, true, true, true, true, true));

            sut.PostDocuments(new TransparentCloudBatch(new[]
            {
                new Dictionary<string, object>
                {
                    ["uniqueid_1"] = "d1",
                    ["big"] = new Dictionary<string, object>
                    {
                        ["abc"] = new [] { "aa", "bb cc" }
                    }
                },
                new Dictionary<string, object>
                {
                    ["uniqueid_1"] = "d2",
                    ["big"] = new Dictionary<string, object>
                    {
                        ["abc"] = new [] { "aa", "bb cc" }
                    }
                }
            }));

            Assert.AreEqual(1, mockService.PostedBatches.Count);
            Assert.AreEqual(2, mockService.PostedBatches[0].Count);
            Assert.AreEqual("d1", mockService.PostedBatches[0][0]["uniqueid_1"]);
            Assert.AreEqual("d2", mockService.PostedBatches[0][1]["uniqueid_1"]);
        }

        [TestMethod]
        public void GetDocumentId_returns_uniqueid_field_if_specified()
        {
            var result = TruncatingSearchService.GetDocumentId(new Dictionary<string, object>
            {
                ["uniqueid_1"] = "d1",
                ["big"] = "abc def"
            });

            Assert.AreEqual("d1", result);
        }

        [TestMethod]
        public void GetDocumentId_returns_UNKNOWN_if_uniqueid_field_not_specified()
        {
            var result = TruncatingSearchService.GetDocumentId(new Dictionary<string, object>
            {
                ["big"] = "abc def"
            });

            Assert.AreEqual("UNKNOWN", result);
        }
    }
}
