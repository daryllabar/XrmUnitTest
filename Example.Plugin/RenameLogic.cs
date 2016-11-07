using System;
using System.Linq;
using DLaB.Xrm.Entities;
using Microsoft.Xrm.Sdk;

namespace Example.Plugin
{
    public class RenameLogic
    {
        /// <summary>
        /// Updates the First or Last Name of the contact to match the given casing
        /// </summary>
        /// <param name="service">The service.</param>
        /// <param name="name">The name.</param>
        public static void MakeNameMatchCase(IOrganizationService service, string name)
        {
            using (var context = new CrmContext(service))
            {
                var contacts = (from c in context.ContactSet
                                where c.FirstName == name || c.LastName == name
                                select new Contact { Id = c.Id, FirstName = c.FirstName, LastName = c.LastName }).ToList();

                foreach (var contact in contacts.Where(c => StringsAreEqualButCaseIsNot(c.FirstName, name)))
                {
                    contact.FirstName = name;
                    context.UpdateObject(contact);
                }

                foreach (var contact in contacts.Where(c => StringsAreEqualButCaseIsNot(c.LastName, name)))
                {
                    contact.LastName = name;
                    context.UpdateObject(contact);
                }

                context.SaveChanges();
            }
        }
        private static bool StringsAreEqualButCaseIsNot(string a, string b)
        {
            return a != b && string.Equals(a, b, StringComparison.OrdinalIgnoreCase);
        }
    }
}
