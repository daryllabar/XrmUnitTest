using System;
using Microsoft.Xrm.Sdk;

namespace Xyz.Xrm.Entities
{
    public interface IAddressable
    {
        // ReSharper disable InconsistentNaming
        string Address1_City { get; set; }
        string Address1_Line1 { get; set; }
        string Address1_Line2 { get; set; }
        string Address1_Line3 { get; set; }
        string Address1_PostalCode { get; set; }
        string Address1_StateOrProvince { get; set; }
        // ReSharper restore InconsistentNaming
    }

    public static class AddressableExtensions
    {
        public static string GetAddress(this IAddressable callable)
        {
            return callable.Address1_Line1 + Environment.NewLine
                   + GetLine(callable.Address1_Line2) 
                   + GetLine(callable.Address1_Line3) 
                   + $@"{callable.Address1_City}, {callable.Address1_StateOrProvince} {callable.Address1_PostalCode}";
        }

        public static string GetAddressLateBound(this Entity callable)
        {
            return callable.GetAttributeValue<string>(Contact.Fields.Address1_Line1) + Environment.NewLine
                                           + GetLine(callable.GetAttributeValue<string>(Contact.Fields.Address1_Line2))
                                           + GetLine(callable.GetAttributeValue<string>(Contact.Fields.Address1_Line3))
                                           + $@"{
                                              callable.GetAttributeValue<string>(Contact.Fields.Address1_City)}, {
                                              callable.GetAttributeValue<string>(Contact.Fields.Address1_StateOrProvince)} { 
                                              callable.GetAttributeValue<string>(Contact.Fields.Address1_PostalCode)}";
        }

        private static string GetLine(string line)
        {
            return string.IsNullOrWhiteSpace(line)
                ? string.Empty
                : line + Environment.NewLine;
        }

        private static void Example()
        {
            // Now GetAddress is a function that exists on all IAddressable Entities
            var address = new Account().GetAddress();
            address = new Contact().GetAddress();
            address = new Lead().GetAddress();
        }
    }

    // List classes that implement this interface here for simplicity or in a seperate file for each class.

    partial class Account : IAddressable { }
    partial class Contact : IAddressable { }
    partial class Lead : IAddressable { }
    partial class BusinessUnit : IAddressable { }
    partial class Competitor : IAddressable { }
    partial class Site : IAddressable { }
    partial class SystemUser : IAddressable { }
}
