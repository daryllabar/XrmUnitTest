using System.Linq;
using DLaB.Xrm.Plugin;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DLaB.Xrm.Tests.Plugin
{
    [TestClass]
    public class RegisteredEventBuilderTests
    {
        [TestMethod]
        public void RegisteredEventBuilder_NoMessage_Should_UseAnyMessage()
        {
            TestStage(PipelineStage.PreValidation);
            TestStage(PipelineStage.PreOperation);
            TestStage(PipelineStage.PostOperation);

            void TestStage(PipelineStage stage)
            {
                var sut = new RegisteredEventBuilder(stage).Build().Single();
                Assert.AreEqual(RegisteredEvent.Any, sut.Message);
                Assert.AreEqual(stage, sut.Stage);
            }
        }

        [TestMethod]
        public void RegisteredEventBuilder_WithAnd_Should_CreateMultipleRegisteredEvents()
        {
            var values = new RegisteredEventBuilder(PipelineStage.PreOperation, MessageType.Create)
                .And(PipelineStage.PostOperation, MessageType.Update)
                .Build();

            Assert.AreEqual(2, values.Count, "Two events should have been created, a single Pre Operation Registered Event and a Single Post Operation Event.");
            var pre = values.Single(v => v.Stage == PipelineStage.PreOperation);
            var post = values.Single(v => v.Stage == PipelineStage.PostOperation);
            Assert.AreEqual(MessageType.Create, pre.Message);
            Assert.AreEqual(MessageType.Update, post.Message);
        }
    }
}
