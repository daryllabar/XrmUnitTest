﻿// ----------------------------------------------------------------------------------
// <copyright file="ExpressionSearchVisitor.cs" company="NMemory Team">
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
namespace NMemory.Common.Visitors
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Linq.Expressions;

    public class ExpressionSearchVisitor : ExpressionVisitor
    {
        private Expression expression;
        private bool found;

        public ExpressionSearchVisitor(Expression expression)
        {
            this.expression = expression;
            this.found = false;
        }

        public override Expression Visit(Expression node)
        {
            if (node == this.expression)
            {
                this.found = true;
            }

            return base.Visit(node);
        }

        public static bool Search(Expression source, Expression node)
        {
            ExpressionSearchVisitor visitor = new ExpressionSearchVisitor(node);
            visitor.Visit(source);

            return visitor.found;
        }
    }
}
