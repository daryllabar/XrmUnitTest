using System;
using System.Collections.Generic;
using System.Text;

namespace DLaB.Xrm.LocalCrm.OptionSets
{
    internal partial class OptionSet
    {
        public enum ActivityParty_ParticipationTypeMask
        {
            BCCRecipient = 4,
            CCRecipient = 3,
            Customer = 11,
            Optionalattendee = 6,
            Organizer = 7,
            Owner = 9,
            Regarding = 8,
            Requiredattendee = 5,
            Resource = 10,
            Sender = 1,
            ToRecipient = 2,
        }
    }
}
