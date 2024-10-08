﻿// -----------------------------------------------------------------------------------
// <copyright file="TransactionContext.cs" company="NMemory Team">
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
#pragma warning disable 1063
namespace NMemory.Transactions
{
    using System;

    public class TransactionContext : IDisposable
    {
        private readonly System.Transactions.CommittableTransaction transaction;
        private bool completed;

        public TransactionContext()
        {
            this.transaction = null;
            this.completed = false;
        }

        public TransactionContext(System.Transactions.CommittableTransaction transaction)
        {
            this.transaction = transaction;
            this.completed = false;
        }

        public void Complete()
        {
            this.completed = true;
        }

        public void Dispose()
        {
            if (this.transaction != null)
            {
                if (this.completed)
                {
                    this.transaction.Commit();
                }
                else
                {
                    this.transaction.Rollback();
                }
            }
        }
    }
}
