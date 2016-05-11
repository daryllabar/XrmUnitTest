using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DLaB.Common.Tests
{
    [TestClass]
    public class ConfigTests
    {
        [TestMethod]
        public void RoundTripDictionary_Should_BeUnchanged()
        {
            var dict = new Dictionary<string, string>
            {
                ["null"] = null,
                ["one"] = "1",
                ["two"] = "2"
            };

            var values = Config.ToString(dict);

            var output = Config.GetDictionary<string, string>(Guid.NewGuid().ToString(), values);

            foreach (var key in dict.Keys)
            {
                Assert.AreEqual(dict[key], output[key]);
            }
        }

        [TestMethod]
        public void RoundTripDictionaryList_Should_BeUnchanged()
        {
            var dict = new Dictionary<string, List<int>>
            {
                ["null"] = null,
                ["empty"] = new List<int> (),
                ["one"] = new List<int>{ 1 },
                ["two"] = new List<int>{ 1, 2 },
                ["three"] = new List<int> { 1, 2, 3 }
            };

            var values = Config.ToString(dict);

            var output = Config.GetDictionaryList<string, int>(Guid.NewGuid().ToString(), values);

            foreach (var key in dict.Keys)
            {
                if (key == "null")
                {
                    Assert.AreEqual(0, output[key].Count);
                    continue;
                }
                Assert.AreEqual(dict[key].Count, output[key].Count);

                var i = 0;
                foreach (var value in dict[key])
                {
                    Assert.AreEqual(value, output[key][i++]);
                }
            }
        }

        [TestMethod]
        public void RoundTripDictionaryHash_Should_BeUnchanged()
        {
            var dict = new Dictionary<string, HashSet<string>>
            {
                {"0", new HashSet<string>()},
                {
                    "1", new HashSet<string>
                    {
                        "A"
                    }
                },
                {
                    "2", new HashSet<string>
                    {
                        "A",
                        "B"
                    }
                }
            };

            var values = Config.ToString(dict);

            var output = Config.GetDictionaryHash<string, string>(Guid.NewGuid().ToString(), values);
            
            Assert.AreEqual(dict.Count, output.Count);

            foreach (var kvp in dict)
            {
                Assert.AreEqual(kvp.Value.Count, output[kvp.Key].Count);
                foreach (var value in kvp.Value)
                {
                    Assert.IsTrue(output[kvp.Key].Contains(value));
                }
            }
        }

        [TestMethod]
        public void RoundTripHash_Should_BeUnchanged()
        {
            var hash = new HashSet<string>
            {
                "",
                "1",
                "2"
            };

            var values = Config.ToString(hash);

            var output = Config.GetHashSet<string>(Guid.NewGuid().ToString(), values);

            Assert.AreEqual(hash.Count, output.Count);

            hash.Remove(null);
            hash.Add("");

            foreach (var key in hash)
            {
                Assert.IsTrue(output.Contains(key));
            }
        }

        [TestMethod]
        public void RoundTripList_Should_BeUnchanged()
        {
            var list = new List<string>
            {
                "",
                "1",
                "2"
            };

            var values = Config.ToString(list);

            var output = Config.GetHashSet<string>(Guid.NewGuid().ToString(), values);

            Assert.AreEqual(list.Count, output.Count);

            list.Remove(null);
            list.Add("");

            foreach (var key in list)
            {
                Assert.IsTrue(output.Contains(key));
            }
        }
    }
}
