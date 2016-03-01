﻿// ----------------------------------------------------------------------------------
// <copyright file="ExecutionContext.cs" company="NMemory Team">
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
// ----------------------------------------------------------------------------------

namespace NMemory.Execution
{
    using System.Collections.Generic;
    using NMemory.Exceptions;
    using NMemory.Modularity;
    using NMemory.Transactions;

    internal class ExecutionContext : IExecutionContext
    {
        private Transaction transaction;
        private IDictionary<string, object> parameters;
        private IDatabase database;
        private OperationType operationType;

        public ExecutionContext(
            IDatabase database, 
            Transaction transaction,
            OperationType operationType)
        {
            this.database = database;
            this.transaction = transaction;
            this.operationType = operationType;
            this.parameters = new Dictionary<string, object>();
        }

        public ExecutionContext(
            IDatabase database, 
            Transaction transaction,
            OperationType operationType,
            IDictionary<string, object> parameters)
        {
            this.database = database;
            this.transaction = transaction;
            this.operationType = operationType;
            this.parameters = parameters;

            // Ensure initialization
            if (this.parameters == null)
            {
                this.parameters = new Dictionary<string, object>();
            }
        }

        public IDatabase Database
        {
            get { return this.database; }
        }

        public Transaction Transaction
        {
            get { return this.transaction; }
        }

        public OperationType OperationType
        {
            get { return this.operationType; }
        }

        public T GetParameter<T>(string name)
        {
            object result = default(T);

            if (!this.parameters.TryGetValue(name, out result))
            {
                throw new ParameterException();
            }

            return (T)result;
        }
    }
}
