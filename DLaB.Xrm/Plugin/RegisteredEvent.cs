using System;
using DLaB.Common;

namespace DLaB.Xrm.Plugin
{
    public class RegisteredEvent
    {
        public PipelineStage Stage { get; set; }
        public MessageType Message { get; set; }
        public String MessageName { get { return Message.ToString(); } }
        public String EntityLogicalName { get; set; }
        public Action<LocalPluginContext> Execute { get; set; }


        /// <summary>
        /// Defaults the execute method to be InternalExecute and run against all entities.
        /// </summary>
        /// <param name="stage"></param>
        /// <param name="message"></param>
        public RegisteredEvent(PipelineStage stage, MessageType message) : this(stage, message, null, null) { }

        /// <summary>
        /// Defaults the execute method to be InternalExecute and runs against the specified entity.
        /// </summary>
        /// <param name="stage"></param>
        /// <param name="message"></param>
        /// <param name="entityLogicalName"></param>
        public RegisteredEvent(PipelineStage stage, MessageType message, string entityLogicalName) : this(stage, message, null, entityLogicalName) { }

        /// <summary>
        /// Runs against all entities.
        /// </summary>
        /// <param name="stage"></param>
        /// <param name="message"></param>
        /// <param name="execute"></param>
        public RegisteredEvent(PipelineStage stage, MessageType message, Action<LocalPluginContext> execute) : this(stage, message, execute, null) { }


        /// <summary>
        /// Runs against the specified entity
        /// </summary>
        /// <param name="stage"></param>
        /// <param name="message"></param>
        /// <param name="execute"></param>
        /// <param name="entityLogicalName"></param>
        public RegisteredEvent(PipelineStage stage, MessageType message, Action<LocalPluginContext> execute, string entityLogicalName)
        {
            Stage = stage;
            EntityLogicalName = entityLogicalName;
            Execute = execute;
            Message = message;
        }
    }

    public enum PipelineStage
    {
        PreValidation = 10,
        PreOperation = 20,
        PostOperation = 40
    }

    public class MessageType : TypeSafeEnumBase<String>
    {
        public MessageType(string name, string value) : base(name, value){ }
        public MessageType(string nameValue) : base(nameValue, nameValue) { }


        // Default OOB CRM Messages
        public static MessageType AddItem = new MessageType("AddItem");
        public static MessageType AddListMembers = new MessageType("AddListMembers");
        public static MessageType AddMember = new MessageType("AddMember");
        public static MessageType AddMembers = new MessageType("AddMembers");
        public static MessageType AddPrincipalToQueue = new MessageType("AddPrincipalToQueue");
        public static MessageType AddPrivileges = new MessageType("AddPrivileges");
        public static MessageType AddProductToKit = new MessageType("AddProductToKit");
        public static MessageType AddRecurrence = new MessageType("AddRecurrence");
        public static MessageType AddToQueue = new MessageType("AddToQueue");
        public static MessageType AddUserToRecordTeam = new MessageType("AddUserToRecordTeam");
        public static MessageType Assign = new MessageType("Assign");
        public static MessageType AssignUserRoles = new MessageType("AssignUserRoles");
        public static MessageType Associate = new MessageType("Associate");
        public static MessageType BackgroundSend = new MessageType("BackgroundSend");
        public static MessageType Book = new MessageType("Book");
        public static MessageType Cancel = new MessageType("Cancel");
        public static MessageType CheckIncoming = new MessageType("CheckIncoming");
        public static MessageType CheckPromote = new MessageType("CheckPromote");
        public static MessageType Clone = new MessageType("Clone");
        public static MessageType Close = new MessageType("Close");
        public static MessageType CopyDynamicListToStatic = new MessageType("CopyDynamicListToStatic");
        public static MessageType CopySystemForm = new MessageType("CopySystemForm");
        public static MessageType Create = new MessageType("Create");
        public static MessageType CreateException = new MessageType("CreateException");
        public static MessageType CreateInstance = new MessageType("CreateInstance");
        public static MessageType Delete = new MessageType("Delete");
        public static MessageType DeleteOpenInstances = new MessageType("DeleteOpenInstances");
        public static MessageType DeliverIncoming = new MessageType("DeliverIncoming");
        public static MessageType DeliverPromote = new MessageType("DeliverPromote");
        public static MessageType DetachFromQueue = new MessageType("DetachFromQueue");
        public static MessageType Disassociate = new MessageType("Disassociate");
        public static MessageType Execute = new MessageType("Execute");
        public static MessageType ExecuteById = new MessageType("ExecuteById");
        public static MessageType Export = new MessageType("Export");
        public static MessageType ExportAll = new MessageType("ExportAll");
        public static MessageType ExportCompressed = new MessageType("ExportCompressed");
        public static MessageType ExportCompressedAll = new MessageType("ExportCompressedAll");
        public static MessageType GenerateSocialProfile = new MessageType("GenerateSocialProfile");
        public static MessageType GrantAccess = new MessageType("GrantAccess");
        public static MessageType Handle = new MessageType("Handle");
        public static MessageType Import = new MessageType("Import");
        public static MessageType ImportAll = new MessageType("ImportAll");
        public static MessageType ImportCompressedAll = new MessageType("ImportCompressedAll");
        public static MessageType ImportCompressedWithProgress = new MessageType("ImportCompressedWithProgress");
        public static MessageType ImportWithProgress = new MessageType("ImportWithProgress");
        public static MessageType LockInvoicePricing = new MessageType("LockInvoicePricing");
        public static MessageType LockSalesOrderPricing = new MessageType("LockSalesOrderPricing");
        public static MessageType Lose = new MessageType("Lose");
        public static MessageType Merge = new MessageType("Merge");
        public static MessageType ModifyAccess = new MessageType("ModifyAccess");
        public static MessageType PickFromQueue = new MessageType("PickFromQueue");
        public static MessageType Publish = new MessageType("Publish");
        public static MessageType PublishAll = new MessageType("PublishAll");
        public static MessageType QualifyLead = new MessageType("QualifyLead");
        public static MessageType Recalculate = new MessageType("Recalculate");
        public static MessageType ReleaseToQueue = new MessageType("ReleaseToQueue");
        public static MessageType RemoveFromQueue = new MessageType("RemoveFromQueue");
        public static MessageType RemoveItem = new MessageType("RemoveItem");
        public static MessageType RemoveMember = new MessageType("RemoveMember");
        public static MessageType RemoveMembers = new MessageType("RemoveMembers");
        public static MessageType RemovePrivilege = new MessageType("RemovePrivilege");
        public static MessageType RemoveProductFromKit = new MessageType("RemoveProductFromKit");
        public static MessageType RemoveRelated = new MessageType("RemoveRelated");
        public static MessageType RemoveUserFromRecordTeam = new MessageType("RemoveUserFromRecordTeam");
        public static MessageType RemoveUserRoles = new MessageType("RemoveUserRoles");
        public static MessageType ReplacePrivileges = new MessageType("ReplacePrivileges");
        public static MessageType Reschedule = new MessageType("Reschedule");
        public static MessageType Retrieve = new MessageType("Retrieve");
        public static MessageType RetrieveExchangeRate = new MessageType("RetrieveExchangeRate");
        public static MessageType RetrieveFilteredForms = new MessageType("RetrieveFilteredForms");
        public static MessageType RetrieveMultiple = new MessageType("RetrieveMultiple");
        public static MessageType RetrievePersonalWall = new MessageType("RetrievePersonalWall");
        public static MessageType RetrievePrincipalAccess = new MessageType("RetrievePrincipalAccess");
        public static MessageType RetrieveRecordWall = new MessageType("RetrieveRecordWall");
        public static MessageType RetrieveSharedPrincipalsAndAccess = new MessageType("RetrieveSharedPrincipalsAndAccess");
        public static MessageType RetrieveUnpublished = new MessageType("RetrieveUnpublished");
        public static MessageType RetrieveUnpublishedMultiple = new MessageType("RetrieveUnpublishedMultiple");
        public static MessageType RetrieveUserQueues = new MessageType("RetrieveUserQueues");
        public static MessageType RevokeAccess = new MessageType("RevokeAccess");
        public static MessageType Route = new MessageType("Route");
        public static MessageType RouteTo = new MessageType("RouteTo");
        public static MessageType Send = new MessageType("Send");
        public static MessageType SendFromTemplate = new MessageType("SendFromTemplate");
        public static MessageType SetRelated = new MessageType("SetRelated");
        public static MessageType SetState = new MessageType("SetState");
        public static MessageType SetStateDynamicEntity = new MessageType("SetStateDynamicEntity");
        public static MessageType TriggerServiceEndpointCheck = new MessageType("TriggerServiceEndpointCheck");
        public static MessageType UnlockInvoicePricing = new MessageType("UnlockInvoicePricing");
        public static MessageType UnlockSalesOrderPricing = new MessageType("UnlockSalesOrderPricing");
        public static MessageType Update = new MessageType("Update");
        public static MessageType ValidateRecurrenceRule = new MessageType("ValidateRecurrenceRule");
        public static MessageType Win = new MessageType("Win");

        // Since this is open (the constructors are public, not private) to allow for Custom Actions to be added, overriding the ToString to allow for value equality
        public override bool Equals(Object obj)
        {
            // If parameter is null return false.
            return obj != null && Equals(obj as MessageType);

        }

        public bool Equals(MessageType message)
        {
            // If parameter is null return false:
            if (message == null)
            {
                return false;
            }

            // Return true if the fields match:
            return Value == message.Value;
        }

        public override int GetHashCode()
        {
            return (Value ?? String.Empty).GetHashCode();
        }

        public static bool operator ==(MessageType a, MessageType b)
        {
            // If both are null, or both are same instance, return true.
            if (ReferenceEquals(a, b))
            {
                return true;
            }

            return (object)a != null && a.Equals(b);
        }

        public static bool operator !=(MessageType a, MessageType b)
        {
            return !(a == b);
        }
    }
}
