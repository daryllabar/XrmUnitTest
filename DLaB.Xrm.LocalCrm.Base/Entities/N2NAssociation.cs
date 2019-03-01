using Microsoft.Xrm.Sdk;
using System;

namespace DLaB.Xrm.LocalCrm.Entities
{
    internal static class N2NAssociations
    {
        public static class Fields
        {
            public const string One = "one";
            public const string Two = "two";
            public const string Id = "n2nassociationid";
        }

    }

    [System.Runtime.Serialization.DataContract()]
    internal class N2NAssociation<TOne, TTwo> : Entity
        where TOne : Entity
        where TTwo : Entity
    {
        // ReSharper disable once StaticMemberInGenericType
        public static string EntityLogicalName;

        public const string PrimaryIdAttribute = N2NAssociations.Fields.Id;

        /// <summary>
        /// Unique identifier of the lead for the N2NAssocation.
        /// </summary>
        [AttributeLogicalName(N2NAssociations.Fields.Id)]
        public Guid? N2NAssociationid
        {
            [System.Diagnostics.DebuggerNonUserCode()]
            get
            {
                return GetAttributeValue<Guid?>(N2NAssociations.Fields.Id);
            }
            [System.Diagnostics.DebuggerNonUserCode()]
            set
            {
                SetAttributeValue(N2NAssociations.Fields.Id, value);
                if (value.HasValue)
                {
                    base.Id = value.Value;
                }
                else
                {
                    base.Id = Guid.Empty;
                }
            }
        }

        [AttributeLogicalName("one")]
        public EntityReference One
        {
            [System.Diagnostics.DebuggerNonUserCode]
            get => GetAttributeValue<EntityReference>("one");
            [System.Diagnostics.DebuggerNonUserCode]
            set => SetAttributeValue("one", value);
        }

        [AttributeLogicalName("two")]
        public EntityReference Two
        {
            [System.Diagnostics.DebuggerNonUserCode]
            get => GetAttributeValue<EntityReference>("two");
            [System.Diagnostics.DebuggerNonUserCode]
            set => SetAttributeValue("two", value);
        }
    }
}
