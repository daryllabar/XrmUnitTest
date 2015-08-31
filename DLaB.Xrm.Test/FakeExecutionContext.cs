using System;
using System.Diagnostics;
using Microsoft.Xrm.Sdk;

namespace DLaB.Xrm.Test
{
    [DebuggerDisplay("{DebugInfo}")]
    public class FakeExecutionContext : IExecutionContext, ICloneable
    {
        public int Mode { get; set; }
        public int IsolationMode { get; set; }
        public int Depth { get; set; }
        public string MessageName { get; set; }
        public string PrimaryEntityName { get; set; }
        public Guid? RequestId { get; set; }
        public string SecondaryEntityName { get; set; }
        public ParameterCollection InputParameters { get; set; }
        public ParameterCollection OutputParameters { get; set; }
        public ParameterCollection SharedVariables { get; set; }
        public Guid UserId { get; set; }
        public Guid InitiatingUserId { get; set; }
        public Guid BusinessUnitId { get; set; }
        public Guid OrganizationId { get; set; }
        public string OrganizationName { get; set; }
        public Guid PrimaryEntityId { get; set; }
        public EntityImageCollection PreEntityImages { get; set; }
        public EntityImageCollection PostEntityImages { get; set; }
        public EntityReference OwningExtension { get; set; }
        public Guid CorrelationId { get; set; }
        public bool IsExecutingOffline { get; set; }
        public bool IsOfflinePlayback { get; set; }
        public bool IsInTransaction { get; set; }
        public Guid OperationId { get; set; }
        public DateTime OperationCreatedOn { get; set; }

        private string DebugInfo
        {
            get { return String.Format("Message: {0}, Entity: {1}, Depth: {2}", MessageName, PrimaryEntityName, Depth ); }
        }

        public FakeExecutionContext()
        {
            InputParameters = new ParameterCollection();
            OutputParameters = new ParameterCollection();
            SharedVariables = new ParameterCollection();
            PreEntityImages = new EntityImageCollection();
            PostEntityImages = new EntityImageCollection();
            UserId = new Guid("eb96c0b5-93cc-4a82-bf9d-f8f5880f4772");
        }

        #region Clone

        public FakeExecutionContext Clone()
        {
            var clone = (FakeExecutionContext) MemberwiseClone();
            CloneReferenceValues(clone);
            return clone;
        }

        protected void CloneReferenceValues(FakeExecutionContext clone)
        {
            clone.InputParameters = new ParameterCollection();
            clone.InputParameters.AddRange(InputParameters);
            clone.OutputParameters = new ParameterCollection();
            clone.OutputParameters.AddRange(OutputParameters);
            clone.SharedVariables = new ParameterCollection();
            clone.SharedVariables.AddRange(SharedVariables);

            clone.PreEntityImages = new EntityImageCollection();
            clone.PreEntityImages.AddRange(PreEntityImages);
            clone.PostEntityImages = new EntityImageCollection();
            clone.PostEntityImages.AddRange(PostEntityImages);
        }

        object ICloneable.Clone()
        {
            return Clone();
        }

        #endregion Clone
    }
}
