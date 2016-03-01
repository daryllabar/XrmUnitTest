﻿// -----------------------------------------------------------------------------------
// <copyright file="IndexTransactionLogItemBase`1.cs" company="NMemory Team">
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

namespace NMemory.Transactions.Logs
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using NMemory.Indexes;

    internal abstract class IndexTransactionLogItemBase<TEntity> : ITransactionLogItem
        where TEntity : class
    {
        private IIndex<TEntity> index;
        private TEntity entity;

        public IndexTransactionLogItemBase(IIndex<TEntity> index, TEntity entity)
        {
            this.index = index;
            this.entity = entity;
        }

        protected IIndex<TEntity> Index
        {
            get { return this.index; }
        }

        protected TEntity Entity
        {
            get { return this.entity; }
        }

        public abstract void Undo();
    }
}
