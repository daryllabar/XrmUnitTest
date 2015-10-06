using DLaB.Xrm.Entities;
using DLaB.Xrm.Test;

namespace Example.MsTestBase.Builders
{
    public class AccountBuilder : EntityBuilder<Account>
    {
        public Account Account { get; set; }

        public AccountBuilder()
        {
            Account = new Account
            {
                AccountNumber = "10"
            };
        }

        public AccountBuilder(Id id)
            : this()
        {
            Id = id;
        }

        #region Fluent Methods


        public AccountBuilder WithAddress1()
        {
            Account.Address1_City = "Any Town";
            Account.Address1_Line1 = "123 Any Street";
            Account.Address1_PostalCode = "12345";
            Account.Address1_StateOrProvince = "IN";
            return this;
        }

        #endregion // Fluent Methods

        protected override Account BuildInternal()
        {
            return Account;
        }
    }
}
