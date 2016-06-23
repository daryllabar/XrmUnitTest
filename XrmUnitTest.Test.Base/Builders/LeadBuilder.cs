using DLaB.Xrm.Entities;
using DLaB.Xrm.Test;
using Microsoft.Xrm.Sdk;

namespace XrmUnitTest.Test.Builders
{
    public class LeadBuilder : EntityBuilder<Lead>
    {
        public Lead Lead { get; set; }

        public LeadBuilder()
        {
            Lead = new Lead();
            WithDoNotContact(false);
        }

        public LeadBuilder(Id id)
            : this() { Id = id; }

        #region Fluent Methods

        /// <summary>
        /// Sets the Address1 of the lead.
        /// </summary>
        /// <returns></returns>
        public LeadBuilder WithAddress1()
        {
            Lead.Address1_Line1 = "1600 Pennsylvania Ave NW";
            Lead.Address1_City = "Washington";
            Lead.Address1_Country = "United States";
            Lead.Address1_County = "DISTRICT OF COLUMBIA";
            Lead.Address1_StateOrProvince = "DC";
            Lead.Address1_PostalCode = "20500-0003";
            Lead.Address1_Name = "White House";
            Lead.Address1_Fax = "202-456-7890";
            Lead.Address1_Latitude = 38.8977;
            Lead.Address1_Longitude = -77.0365;
            return this;
        }

        /// <summary>
        /// Sets the Address2 of the lead.
        /// </summary>
        /// <returns></returns>
        public LeadBuilder WithAddress2()
        {
            Lead.Address2_Line1 = "101 Independence Ave";
            Lead.Address2_City = "Washington";
            Lead.Address2_Country = "United States";
            Lead.Address2_County = "DISTRICT OF COLUMBIA";
            Lead.Address2_StateOrProvince = "DC";
            Lead.Address2_PostalCode = "20540-0002";
            Lead.Address2_Name = "Libary of Congress";
            Lead.Address2_Fax = "202-707-2371";
            Lead.Address2_Latitude = 38.8887;
            Lead.Address2_Longitude = -77.0047;
            return this;
        }

        /// <summary>
        /// Sets the Campaign
        /// </summary>
        /// <param name="campaign">The campaign.</param>
        /// <returns></returns>
        public LeadBuilder WithCampaign(EntityReference campaign)
        {
            Lead.CampaignId = campaign;
            return this;
        }

        public LeadBuilder WithDoNotContact(bool doNot)
        {
            Lead.DoNotBulkEMail = doNot;
            Lead.DoNotEMail = doNot;
            Lead.DoNotFax = doNot;
            Lead.DoNotPhone = doNot;
            Lead.DoNotPostalMail= doNot;
            Lead.DoNotSendMM = doNot;

            return this;
        }

        public LeadBuilder WithEmail(string email1 = "test1@test.com", string email2 = "test2@test.com", string email3 = "test3@test.com")
        {
            Lead.EMailAddress1 = email1;
            Lead.EMailAddress2 = email2;
            Lead.EMailAddress3 = email3;

            return this;
        }

        public LeadBuilder WithPhone(string mobilePhone = "999-999-999",
                                     string address1Telephone1 = "111-111-1111",
                                     string address1Telephone2 = "111-111-1112",
                                     string address1Telephone3 = "111-111-1113",
                                     string address2Telephone1 = "211-111-1111",
                                     string address2Telephone2 = "211-111-1112",
                                     string address2Telephone3 = "211-111-1113",
                                     string telephone1 = "555-555-5551",
                                     string telephone2 = "555-555-5552",
                                     string telephone3 = "555-555-5553",
                                     string pager = "777-777-7777") 
        {
            Lead.Address1_Telephone1 = address1Telephone1;
            Lead.Address1_Telephone2 = address1Telephone2;
            Lead.Address1_Telephone3 = address1Telephone3;
            Lead.Address2_Telephone1 = address2Telephone1;
            Lead.Address2_Telephone2 = address2Telephone2;
            Lead.Address2_Telephone3 = address2Telephone3;
            Lead.MobilePhone = mobilePhone;
            Lead.Pager = pager;
            Lead.Telephone1 = telephone1;
            Lead.Telephone2 = telephone2;
            Lead.Telephone3 = telephone3;

            return this;
        }

        public LeadBuilder WithName(string firstName = "Bob", 
                                    string middleName = "WeHadABaby", 
                                    string lastName = "ItsABoy", 
                                    string salutation = "Sir")
        {
            Lead.FirstName = firstName;
            Lead.MiddleName = middleName;
            Lead.LastName = lastName;
            Lead.YomiFirstName = firstName;
            Lead.YomiMiddleName = middleName;
            Lead.YomiLastName = lastName;
            Lead.Salutation = salutation;
            return this;
        }

        #endregion // Fluent Methods

        protected override Lead BuildInternal()
        {
            return Lead;
        }
    }
}
