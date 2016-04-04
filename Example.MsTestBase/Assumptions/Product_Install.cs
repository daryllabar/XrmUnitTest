using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DLaB.Xrm;
using DLaB.Xrm.Entities;
using Microsoft.Xrm.Sdk;

namespace Example.MsTestBase.Assumptions
{
    public class Product_Install : DLaB.Xrm.Test.Assumptions.EntityDataAssumptionBaseAttribute
    {
        protected override Entity RetrieveEntity(IOrganizationService service)
        {
            return service.GetFirstOrDefault<Product>(p => new
            {
                p.ProductNumber,
                p.ProductId,
                p.Description
            }, 
            Product.Fields.ProductNumber, "Install");
        }
    }
}
