﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
#if NET
using DataverseUnitTest;
#else
using DLaB.Xrm.Test;
#endif

namespace XrmUnitTest.Test
{
    public class DebugLogger: ITestLogger
    {
        public void WriteLine(string message)
        {
            Debug.WriteLine(message);
        }

        public void WriteLine(string format, params object[] args)
        {
            Debug.WriteLine(format, args);
        }
    }
}
