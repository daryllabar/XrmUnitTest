using System;
using System.Diagnostics;
using Microsoft.Xrm.Sdk;

#if NET
namespace DataverseUnitTest
#else
namespace DLaB.Xrm.Test
#endif
{
    /// <summary>
    /// Fake Execution Context that implements IExecutionContext
    /// </summary>
    [DebuggerDisplay("{DebugInfo}")]
    public class FakeExecutionContext : IExecutionContext, IServiceFaked<IExecutionContext>, IFakeService, ICloneable
    {
        /// <summary>
        /// Gets or sets the mode.
        /// </summary>
        /// <value>
        /// The mode.
        /// </value>
        public int Mode { get; set; }
        /// <summary>
        /// Gets or sets the isolation mode.
        /// </summary>
        /// <value>
        /// The isolation mode.
        /// </value>
        public int IsolationMode { get; set; }
        /// <summary>
        /// Gets or sets the depth.
        /// </summary>
        /// <value>
        /// The depth.
        /// </value>
        public int Depth { get; set; }
        /// <summary>
        /// Gets or sets the name of the message.
        /// </summary>
        /// <value>
        /// The name of the message.
        /// </value>
        public string MessageName { get; set; }
        /// <summary>
        /// Gets or sets the name of the primary entity.
        /// </summary>
        /// <value>
        /// The name of the primary entity.
        /// </value>
        public string PrimaryEntityName { get; set; }
        /// <summary>
        /// Gets or sets the request identifier.
        /// </summary>
        /// <value>
        /// The request identifier.
        /// </value>
        public Guid? RequestId { get; set; }
        /// <summary>
        /// Gets or sets the name of the secondary entity.
        /// </summary>
        /// <value>
        /// The name of the secondary entity.
        /// </value>
        public string SecondaryEntityName { get; set; }
        /// <summary>
        /// Gets or sets the input parameters.
        /// </summary>
        /// <value>
        /// The input parameters.
        /// </value>
        public ParameterCollection InputParameters { get; set; }
        /// <summary>
        /// Gets or sets the output parameters.
        /// </summary>
        /// <value>
        /// The output parameters.
        /// </value>
        public ParameterCollection OutputParameters { get; set; }
        /// <summary>
        /// Gets or sets the shared variables.
        /// </summary>
        /// <value>
        /// The shared variables.
        /// </value>
        public ParameterCollection SharedVariables { get; set; }
        /// <summary>
        /// Gets or sets the user identifier.
        /// </summary>
        /// <value>
        /// The user identifier.
        /// </value>
        public Guid UserId { get; set; }
        /// <summary>
        /// Gets or sets the initiating user identifier.
        /// </summary>
        /// <value>
        /// The initiating user identifier.
        /// </value>
        public Guid InitiatingUserId { get; set; }
        /// <summary>
        /// Gets or sets the business unit identifier.
        /// </summary>
        /// <value>
        /// The business unit identifier.
        /// </value>
        public Guid BusinessUnitId { get; set; }
        /// <summary>
        /// Gets or sets the organization identifier.
        /// </summary>
        /// <value>
        /// The organization identifier.
        /// </value>
        public Guid OrganizationId { get; set; }
        /// <summary>
        /// Gets or sets the name of the organization.
        /// </summary>
        /// <value>
        /// The name of the organization.
        /// </value>
        public string OrganizationName { get; set; }
        /// <summary>
        /// Gets or sets the primary entity identifier.
        /// </summary>
        /// <value>
        /// The primary entity identifier.
        /// </value>
        public Guid PrimaryEntityId { get; set; }
        /// <summary>
        /// Gets or sets the pre entity images.
        /// </summary>
        /// <value>
        /// The pre entity images.
        /// </value>
        public EntityImageCollection PreEntityImages { get; set; }
        /// <summary>
        /// Gets or sets the post entity images.
        /// </summary>
        /// <value>
        /// The post entity images.
        /// </value>
        public EntityImageCollection PostEntityImages { get; set; }
        /// <summary>
        /// Gets or sets the owning extension.
        /// </summary>
        /// <value>
        /// The owning extension.
        /// </value>
        public EntityReference OwningExtension { get; set; }
        /// <summary>
        /// Gets or sets the correlation identifier.
        /// </summary>
        /// <value>
        /// The correlation identifier.
        /// </value>
        public Guid CorrelationId { get; set; }
        /// <summary>
        /// Gets or sets a value indicating whether this instance is executing offline.
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance is executing offline; otherwise, <c>false</c>.
        /// </value>
        public bool IsExecutingOffline { get; set; }
        /// <summary>
        /// Gets or sets a value indicating whether this instance is offline playback.
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance is offline playback; otherwise, <c>false</c>.
        /// </value>
        public bool IsOfflinePlayback { get; set; }
        /// <summary>
        /// Gets or sets a value indicating whether this instance is in transaction.
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance is in transaction; otherwise, <c>false</c>.
        /// </value>
        public bool IsInTransaction { get; set; }
        /// <summary>
        /// Gets or sets the operation identifier.
        /// </summary>
        /// <value>
        /// The operation identifier.
        /// </value>
        public Guid OperationId { get; set; }
        /// <summary>
        /// Gets or sets the operation created on.
        /// </summary>
        /// <value>
        /// The operation created on.
        /// </value>
        public DateTime OperationCreatedOn { get; set; }

        private string DebugInfo => $"Message: {MessageName}, Entity: {PrimaryEntityName}, Depth: {Depth}";

        /// <summary>
        /// Initializes a new instance of the <see cref="FakeExecutionContext" /> class.
        /// </summary>
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

        /// <summary>
        /// Clones this instance.
        /// </summary>
        /// <returns></returns>
        public FakeExecutionContext Clone()
        {
            var clone = (FakeExecutionContext)MemberwiseClone();
            CloneReferenceValues(clone);
            return clone;
        }

        /// <summary>
        /// Clones the reference values.
        /// </summary>
        /// <param name="clone">The clone.</param>
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
