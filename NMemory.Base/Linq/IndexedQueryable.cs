﻿// ----------------------------------------------------------------------------------
// <copyright file="IndexedQueryable.cs" company="NMemory Team">
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
#pragma warning disable 1591
namespace NMemory.Linq.Helpers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;
    using NMemory.Indexes;

    public class IndexedQueryable<T> : IIndexedQueryable<T>
    {
        private IIndex index;
        private IQueryable<T> query;

        public IndexedQueryable(IQueryable<T> query, IIndex index)
        {
            this.index = index;
            this.query = query;
        }
        
        public IEnumerator<T> GetEnumerator()
        {
            return this.query.GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return this.query.GetEnumerator();
        }

        public Type ElementType
        {
            get { return this.query.ElementType; }
        }

        public Expression Expression
        {
            get { return this.query.Expression; }
        }

        public IQueryProvider Provider
        {
            get { return this.query.Provider; }
        }

        public IIndex Index
        {
            get { return this.index; }
        }
    }
}
