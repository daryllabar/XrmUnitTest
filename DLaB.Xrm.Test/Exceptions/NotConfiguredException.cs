using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace DLaB.Xrm.Test.Exceptions
{
    public class NotConfiguredException : Exception
    {
        public NotConfiguredException(string message) : base(message)
        {
        }

        public NotConfiguredException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}
