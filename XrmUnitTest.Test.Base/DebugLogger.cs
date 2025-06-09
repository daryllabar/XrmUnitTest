using System.Diagnostics;
#if NET
using DataverseUnitTest;
#else
using DLaB.Xrm.Test;
#endif

namespace XrmUnitTest.Test
{
    public class DebugLogger: ITestLogger
    {
        public bool Enabled { get; set; } = true;

        public void WriteLine(string message)
        {
            if (Enabled)
            {
                Debug.WriteLine(message);
                Debug.WriteLine("");
            }
        }

        public void WriteLine(string format, params object[] args)
        {
            if (Enabled)
            {
                Debug.WriteLine(format, args);
                Debug.WriteLine("");
            }
        }
    }
}
