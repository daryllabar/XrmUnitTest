﻿// -----------------------------------------------------------------------------------
// <copyright file="TransactionLog.cs" company="NMemory Team">
//     Copyright (C) 2012-2014 NMemory Team
//
//     Permission is hereby granted, free of charge, to any person obtaining a copy
//     of this software and associated documentation files (the "Software"), to deal
//     in the Software without restriction, including without limitation the rights
//     to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
//     copies of the Software, and to permit persons to whom the Software is
//     furnished to do so, subject to the following conditions:
//
//     The above copyright notice and this permission notice shall be included in
//     all copies or substantial portions of the Software.
//
//     THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
//     IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
//     FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
//     AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
//     LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
//     OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
//     THE SOFTWARE.
// </copyright>
// -----------------------------------------------------------------------------------
#pragma warning disable 1591
namespace NMemory.Transactions.Logs
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using NMemory.Indexes;
    using System.Threading;
    using NMemory.Tables;

    public class TransactionLog : ITransactionLog
    {
        private List<ITransactionLogItem> logItems;

        public TransactionLog()
        {
            this.logItems = new List<ITransactionLogItem>();
        }

        public int CurrentPosition
        {
            get { return this.logItems.Count; }
        }

        public void Rollback()
        {
            this.RollbackTo(0);
        }

        public void RollbackTo(int position)
        {
            for (int i = this.logItems.Count - 1; i >= position; i--)
            {
                this.logItems[i].Undo();
                this.logItems.RemoveAt(i);
            }
        }

        public void Write(ITransactionLogItem item)
        {
            this.logItems.Add(item);
        }

        public void Release()
        {
            this.logItems.Clear();
        }
    }
}
