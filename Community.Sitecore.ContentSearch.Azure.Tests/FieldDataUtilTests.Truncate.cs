using System.Collections;
using System.Collections.Generic;
using Community.Sitecore.ContentSearch.Azure;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Community.Sitecore.ContentSearch.Azure.Tests
{
    public partial class FieldDataUtilTests
    {
        [TestClass]
        public class Truncate
        {
            [TestMethod]
            public void Strings_remove_specified_bytes_as_characters()
            {
                Assert.AreEqual("abcd", FieldDataUtil.Truncate("abcd def", 3));
            }

            [TestMethod]
            public void Strings_truncate_to_word_barrier()
            {
                Assert.AreEqual("abcd", FieldDataUtil.Truncate("abcd def", 2));
            }

            [TestMethod]
            public void Strings_return_blank_when_truncating_more_than_string_size()
            {
                Assert.AreEqual("", FieldDataUtil.Truncate("abcd def", 20));
            }

            [TestMethod]
            public void Strings_return_blank_when_word_barrier_is_first()
            {
                Assert.AreEqual("", FieldDataUtil.Truncate("abcd def", 5));
            }

            [TestMethod]
            public void Arrays_trim_values_from_end()
            {
                var input = new []
                {
                    "ffff ffff",
                    "abcd def"
                };

                CollectionAssert.AreEqual(
                    new[] { "ffff ffff", "abcd" },
                    FieldDataUtil.Truncate(input, 3) as ICollection
                );
            }

            [TestMethod]
            public void Arrays_remove_entire_values()
            {
                var input = new[]
                {
                    "ffff ffff",
                    "abcd def"
                };

                CollectionAssert.AreEqual(
                    new[] { "ffff" },
                    FieldDataUtil.Truncate(input, 12) as ICollection
                );
            }

            [TestMethod]
            public void Dictionaries_trim_values()
            {
                var input = new Dictionary<string, object>
                {
                    ["key1"] = "ffff ffff",
                    ["key2"] = "abcd def"
                };

                var result = FieldDataUtil.Truncate(input, 3) as Dictionary<string, object>;

                Assert.AreEqual("ffff", result["key1"]);
                Assert.AreEqual("abcd def", result["key2"]);
            }

            [TestMethod]
            public void Dictionaries_does_not_remove_empty_values()
            {
                var input = new Dictionary<string, object>
                {
                    ["key1"] = "ffff ffff",
                    ["key2"] = "abcd def"
                };

                var result = FieldDataUtil.Truncate(input, 6) as Dictionary<string, object>;

                Assert.AreEqual("", result["key1"]);
                Assert.AreEqual("abcd def", result["key2"]);
            }
        }
    }
}
