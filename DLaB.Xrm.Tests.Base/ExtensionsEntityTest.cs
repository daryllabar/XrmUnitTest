using DLaB.Xrm.Entities;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Xrm.Sdk;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DLaB.Xrm.Tests
{
    [TestClass]
    public class ExtensionsEntityTest
    {
        [TestMethod]
        public void Extensions_Entity_ToStringAttributes()
        {
            var entity = new Contact
            {
                FirstName = "a",
                LastName = "b",
            };

            AssertAreEqual("{0}[firstname]: a{1}{0}[lastname]: b", entity.ToStringAttributes());
            var phoneCall = new PhoneCall
            {
                From = new List<ActivityParty>{
                    new ActivityParty { PartyId = new EntityReference(entity.LogicalName, Guid.NewGuid())
                        {
                            Name = "a"
                        }
                    },
                    new ActivityParty { PartyId = new EntityReference(entity.LogicalName, Guid.NewGuid())
                        {
                            Name = "b"
                        }
                    }
                }
            };

            AssertAreEqual("{0}[from]: {1}{0}{0}[0:partyid]: " 
                + phoneCall.From.First().PartyId.GetNameId() + "{1}{0}{0}[1:partyid]: " 
                + phoneCall.From.Skip(1).First().PartyId.GetNameId(), phoneCall.ToStringAttributes());
        }

        private static void AssertAreEqual(string expectedFormat, string actual)
        {
            var expected = string.Format(expectedFormat, GenerateNonBreakingSpace(4), Environment.NewLine);
            for (var i = 0; i < expected.Length; i++)
            {
                Assert.AreEqual(expected[i].ToString(), actual[i].ToString(), "error occured at " + i);
            }
        }

        private static string GenerateNonBreakingSpace(int spaces)
        {
            const string space = " "; // This is not a space, it is a Non-Breaking Space (alt+255).  In the log things get trimmed, and this will prevent that from happening;
            return new string(space[0], spaces);
        }
    }
}

