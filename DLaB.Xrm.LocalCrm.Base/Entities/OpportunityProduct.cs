using System.Diagnostics.CodeAnalysis;

namespace DLaB.Xrm.LocalCrm.Entities
{
    [SuppressMessage("ReSharper", "InconsistentNaming")]
    internal class OpportunityProduct
    {
        public struct Fields
        {
            public const string BaseAmount = "baseamount";
            public const string BaseAmount_Base = "baseamount_base";
            public const string CreatedBy = "createdby";
            public const string CreatedOn = "createdon";
            public const string CreatedOnBehalfBy = "createdonbehalfby";
            public const string Description = "description";
            public const string dfnd_IncentivePoints = "dfnd_incentivepoints";
            public const string EntityImage = "entityimage";
            public const string EntityImage_Timestamp = "entityimage_timestamp";
            public const string EntityImage_URL = "entityimage_url";
            public const string EntityImageId = "entityimageid";
            public const string ExchangeRate = "exchangerate";
            public const string ExtendedAmount = "extendedamount";
            public const string ExtendedAmount_Base = "extendedamount_base";
            public const string ImportSequenceNumber = "importsequencenumber";
            public const string IsPriceOverridden = "ispriceoverridden";
            public const string IsProductOverridden = "isproductoverridden";
            public const string LineItemNumber = "lineitemnumber";
            public const string ManualDiscountAmount = "manualdiscountamount";
            public const string ManualDiscountAmount_Base = "manualdiscountamount_base";
            public const string ModifiedBy = "modifiedby";
            public const string ModifiedOn = "modifiedon";
            public const string ModifiedOnBehalfBy = "modifiedonbehalfby";
            public const string OpportunityId = "opportunityid";
            public const string OpportunityProductId = "opportunityproductid";
            public const string Id = "opportunityproductid";
            public const string OpportunityStateCode = "opportunitystatecode";
            public const string OverriddenCreatedOn = "overriddencreatedon";
            public const string OwnerId = "ownerid";
            public const string OwningBusinessUnit = "owningbusinessunit";
            public const string OwningUser = "owninguser";
            public const string ParentBundleId = "parentbundleid";
            public const string PricePerUnit = "priceperunit";
            public const string PricePerUnit_Base = "priceperunit_base";
            public const string PricingErrorCode = "pricingerrorcode";
            public const string ProductAssociationId = "productassociationid";
            public const string ProductDescription = "productdescription";
            public const string ProductId = "productid";
            public const string ProductTypeCode = "producttypecode";
            public const string PropertyConfigurationStatus = "propertyconfigurationstatus";
            public const string Quantity = "quantity";
            public const string SequenceNumber = "sequencenumber";
            public const string Tax = "tax";
            public const string Tax_Base = "tax_base";
            public const string TimeZoneRuleVersionNumber = "timezoneruleversionnumber";
            public const string TransactionCurrencyId = "transactioncurrencyid";
            public const string UoMId = "uomid";
            public const string UTCConversionTimeZoneCode = "utcconversiontimezonecode";
            public const string VersionNumber = "versionnumber";
            public const string VolumeDiscountAmount = "volumediscountamount";
            public const string VolumeDiscountAmount_Base = "volumediscountamount_base";
            public const string lk_opportunityproduct_createdonbehalfby = "lk_opportunityproduct_createdonbehalfby";
            public const string lk_opportunityproduct_modifiedonbehalfby = "lk_opportunityproduct_modifiedonbehalfby";
            public const string lk_opportunityproductbase_createdby = "lk_opportunityproductbase_createdby";
            public const string lk_opportunityproductbase_modifiedby = "lk_opportunityproductbase_modifiedby";
            public const string opportunity_products = "opportunity_products";
            public const string Referencingopportunityproduct_parent_opportunityproduct = "opportunityproduct_parent_opportunityproduct";
            public const string product_opportunities = "product_opportunities";
            public const string productAssociation_opportunity_product = "productAssociation_opportunity_product";
            public const string transactioncurrency_opportunityproduct = "transactioncurrency_opportunityproduct";
            public const string unit_of_measurement_opportunity_products = "unit_of_measurement_opportunity_products";
        }

        public const string EntityLogicalName = "opportunityproduct";
    }
}
