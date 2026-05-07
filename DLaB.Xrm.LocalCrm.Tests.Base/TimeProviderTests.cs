using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DLaB.Xrm.LocalCrm.Tests
{
    [TestClass]
    public class TimeProviderTests
    {
        private TimeProvider _sut;

        [TestInitialize]
        public void TestInitialize()
        {
            _sut = new TimeProvider();
        }

        [TestMethod]
        public void GetUtcNow_WithNoUtcNowSet_ShouldReturnDefaultGetUtcResult()
        {
            var before = DateTime.UtcNow;
            var result = _sut.GetUtcNow();
            var after = DateTime.UtcNow;

            Assert.IsTrue(result >= before && result <= after, 
                $"Expected result between {before} and {after}, but got {result}");
        }

        [TestMethod]
        public void GetUtcNow_WithUtcNowSet_ShouldReturnSetValue()
        {
            var expectedTime = new DateTime(2024, 6, 15, 10, 30, 45, DateTimeKind.Utc);
            _sut.UtcNow = expectedTime;

            var result = _sut.GetUtcNow();

            Assert.AreEqual(expectedTime, result);
        }

        [TestMethod]
        public void GetUtcNow_WithCustomDefaultGetUtc_ShouldReturnCustomValue()
        {
            var customTime = new DateTime(2023, 12, 25, 0, 0, 0, DateTimeKind.Utc);
            _sut.DefaultGetUtc = () => customTime;

            var result = _sut.GetUtcNow();

            Assert.AreEqual(customTime, result);
        }

        [TestMethod]
        public void GetUtcNow_WithUtcNowSetAndCustomDefaultGetUtc_ShouldReturnUtcNowValue()
        {
            var customTime = new DateTime(2023, 12, 25, 0, 0, 0, DateTimeKind.Utc);
            var overrideTime = new DateTime(2024, 1, 1, 12, 0, 0, DateTimeKind.Utc);
            
            _sut.DefaultGetUtc = () => customTime;
            _sut.UtcNow = overrideTime;

            var result = _sut.GetUtcNow();

            Assert.AreEqual(overrideTime, result, "UtcNow should take precedence over DefaultGetUtc");
        }

        [TestMethod]
        public void GetUtcNow_AfterSettingUtcNowToNull_ShouldReturnDefaultGetUtcResult()
        {
            var customTime = new DateTime(2023, 12, 25, 0, 0, 0, DateTimeKind.Utc);
            
            _sut.DefaultGetUtc = () => customTime;
            _sut.UtcNow = new DateTime(2024, 1, 1);
            _sut.UtcNow = null;

            var result = _sut.GetUtcNow();

            Assert.AreEqual(customTime, result, "Should return DefaultGetUtc result when UtcNow is null");
        }

        [TestMethod]
        public void UtcNow_CanBeSetAndRetrieved()
        {
            var testTime = new DateTime(2024, 3, 15, 14, 30, 0, DateTimeKind.Utc);

            _sut.UtcNow = testTime;

            Assert.AreEqual(testTime, _sut.UtcNow);
        }

        [TestMethod]
        public void UtcNow_InitiallyNull()
        {
            Assert.IsNull(_sut.UtcNow);
        }

        [TestMethod]
        public void DefaultGetUtc_CanBeReplaced()
        {
            var customTime1 = new DateTime(2024, 1, 1);
            var customTime2 = new DateTime(2024, 6, 1);

            _sut.DefaultGetUtc = () => customTime1;
            Assert.AreEqual(customTime1, _sut.GetUtcNow());

            _sut.DefaultGetUtc = () => customTime2;
            Assert.AreEqual(customTime2, _sut.GetUtcNow());
        }

        [TestMethod]
        public void GetUtcNow_MultipleCallsWithUtcNowSet_ShouldReturnSameValue()
        {
            var fixedTime = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            _sut.UtcNow = fixedTime;

            var result1 = _sut.GetUtcNow();
            var result2 = _sut.GetUtcNow();
            var result3 = _sut.GetUtcNow();

            Assert.AreEqual(fixedTime, result1);
            Assert.AreEqual(fixedTime, result2);
            Assert.AreEqual(fixedTime, result3);
        }

        [TestMethod]
        public void TimeProvider_SupportsTestingScenarios()
        {
            var simulatedTime = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            
            _sut.UtcNow = simulatedTime;
            Assert.AreEqual(simulatedTime, _sut.GetUtcNow(), "Should support fixed time for testing");

            _sut.UtcNow = simulatedTime.AddHours(1);
            Assert.AreEqual(simulatedTime.AddHours(1), _sut.GetUtcNow(), "Should support time progression simulation");

            _sut.UtcNow = null;
            var actualTime = _sut.GetUtcNow();
            Assert.IsLessThan(1, Math.Abs((DateTime.UtcNow - actualTime).TotalSeconds), "Should return to actual time when override is removed");
        }
    }
}
