using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DLaB.Common.Tests
{
    [TestClass]
    public class SubstringByStringTests
    {
        private const string SingleTestString = "_ABC123_";
        private const string RepeatingTestString = "_123456789_123456789_123456789_123456789_123456789_";

        [TestMethod]
        public void SubstringByString_NoSubstringFound_Should_ReturnNull()
        {
            int endIndex;
            Assert.IsNull(SingleTestString.SubstringByString("a"));
            Assert.IsNull(SingleTestString.SubstringByString("a", "b"));
            Assert.IsNull(SingleTestString.SubstringByString("a", "B"));
            Assert.IsNull(SingleTestString.SubstringByString("A", "b"));
            Assert.IsNull(SingleTestString.SubstringByString("a", "B", out endIndex));
            Assert.IsNull(SingleTestString.SubstringByString("A", "b", out endIndex));
            Assert.AreEqual(-1, endIndex);
            Assert.IsNull(SingleTestString.SubstringByString(0, "b"));
            Assert.IsNull(SingleTestString.SubstringByString(0, "b", out endIndex));
            Assert.AreEqual(-1, endIndex);
        }

        [TestMethod]
        public void SubstringByString_StartOnly_Should_ReturnSubstring()
        {
            Assert.AreEqual("BC123_", SingleTestString.SubstringByString("A"));
            Assert.AreEqual("BC123_", SingleTestString.SubstringByString("a", StringComparison.OrdinalIgnoreCase));
        }

        [TestMethod]
        public void SubstringByString_StartAndEnd_Should_ReturnSubstring()
        {
            // _123456789_123456789_123456789_123456789_123456789_
            const string expected = "123456789";
            int endIndex;
            Assert.AreEqual(expected, RepeatingTestString.SubstringByString("_", "_"));
            Assert.AreEqual(expected, RepeatingTestString.SubstringByString("_", "_", out endIndex));
            Assert.AreEqual(10, endIndex);
            
            Assert.AreEqual(expected, RepeatingTestString.SubstringByString(41, "_"));
            Assert.AreEqual(expected, RepeatingTestString.SubstringByString(41, "_", out endIndex));
            Assert.AreEqual(50, endIndex);
        }


        [TestMethod]
        public void SubstringAllByString_MultipleMatches_Should_ReturnAll()
        {
            // _123456789_123456789_123456789_123456789_123456789_
            const string expected = "123456789";
            var values = RepeatingTestString.SubstringAllByString("_", "_");
            Assert.AreEqual(5, values.Count);
            Assert.AreEqual(expected, values[0]);
            Assert.AreEqual(expected, values[1]);
            Assert.AreEqual(expected, values[2]);
            Assert.AreEqual(expected, values[3]);
            Assert.AreEqual(expected, values[4]);

            values = "_1__1_".SubstringAllByString("_", "_", splitOptions: StringSplitOptions.RemoveEmptyEntries);
            Assert.AreEqual(2, values.Count);

            values = "_1__1_".SubstringAllByString("_", "_");
            Assert.AreEqual(3, values.Count);
        }

        [TestMethod]
        public void SubstringAllByString_NoMatches_Should_ReturnEmpty()
        {
            // "_123456789_123456789_123456789_123456789_123456789_"
            var values = RepeatingTestString.SubstringAllByString("_", "a");
            Assert.AreEqual(0, values.Count);
            values = RepeatingTestString.SubstringAllByString("a", "_");
            Assert.AreEqual(0, values.Count);
        }
    }
}
