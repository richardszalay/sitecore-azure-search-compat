using System;
using System.Collections.Generic;
using Community.Sitecore.ContentSearch.Azure;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Community.Sitecore.ContentSearch.Azure.Tests
{
    public partial class FieldDataUtilTests
    {
        [TestClass]
        public class EstimateSize
        {
            [TestMethod]
            public void Returns_UTF8_byte_count_for_strings()
            {
                Assert.AreEqual(7, FieldDataUtil.EstimateSize("ābcdef"));
            }

            [TestMethod]
            public void Returns_total_element_size_for_lists()
            {
                Assert.AreEqual(10, FieldDataUtil.EstimateSize(new[] { "ābcdef", "ghi" }));
            }

            [TestMethod]
            public void Handles_lists_with_nested_lists()
            {
                Assert.AreEqual(11, FieldDataUtil.EstimateSize(new object[] { "a", new[] { "ābcdef", "ghi" } }));
            }

            [TestMethod]
            public void Returns_key_and_value_sizes_for_dictionaries()
            {
                Assert.AreEqual(16, FieldDataUtil.EstimateSize(new Dictionary<string, object>
                {
                    ["ābc"] = "def",
                    ["hijk"] = "lmnö"
                }));
            }

            [TestMethod]
            public void Handles_dictionaries_in_dictionaries()
            {
                Assert.AreEqual(15, FieldDataUtil.EstimateSize(new Dictionary<string, object>
                {
                    ["ābc"] = new Dictionary<string, object>
                    {
                        ["b"] = "c"
                    },
                    ["hijk"] = "lmnö"
                }));
            }

            [TestMethod]
            public void Handles_lists_in_dictionaries()
            {
                Assert.AreEqual(18, FieldDataUtil.EstimateSize(new Dictionary<string, object>
                {
                    ["ābc"] = new Dictionary<string, object>
                    {
                        ["b"] = new [] { "aa", "bb" }
                    },
                    ["hijk"] = "lmnö"
                }));
            }
        }
    }
}
