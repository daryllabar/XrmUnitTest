﻿// -----------------------------------------------------------------------------------
// <copyright file="RelationConstraint`3.cs" company="NMemory Team">
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
namespace NMemory.Tables
{
    using System;
    using System.Linq.Expressions;
    using System.Reflection;

    public class RelationConstraint<TPrimary, TForeign, TField> : IRelationContraint
    {
        public RelationConstraint(Expression<Func<TPrimary, TField>> primaryField, Expression<Func<TForeign, TField>> foreignField)
        {
            if (primaryField == null)
            {
                throw new ArgumentNullException("primaryField");
            }

            if (foreignField == null)
            {
                throw new ArgumentNullException("foreignField");
            }

            MemberExpression primaryMember = primaryField.Body as MemberExpression;
            
            if (primaryMember == null)
            {
                UnaryExpression convertExpression = primaryField.Body as UnaryExpression;

                if (convertExpression.NodeType == ExpressionType.Convert)
                {
                    primaryMember = convertExpression.Operand as MemberExpression;
                }
            }

            if (primaryMember == null)
            {
                throw new ArgumentException("", "primaryField");
            }

            this.PrimaryField = primaryMember.Member;

            if (this.PrimaryField == null)
            {
                throw new ArgumentException("", "primaryField");
            }

            MemberExpression foreignMember = foreignField.Body as MemberExpression;

            if (foreignMember == null)
            {
                UnaryExpression convertExpression = foreignField.Body as UnaryExpression;

                if (convertExpression.NodeType == ExpressionType.Convert)
                {
                    foreignMember = convertExpression.Operand as MemberExpression;
                }
            }

            if (foreignMember == null)
            {
                throw new ArgumentException("", "foreignField");
            }

            this.ForeignField = foreignMember.Member;

            if (this.ForeignField == null)
            {
                throw new ArgumentException("", "foreignField");
            }
        }

        public MemberInfo PrimaryField
        {
            get;
            private set;
        }

        public MemberInfo ForeignField
        {
            get;
            private set;
        }
    }
}
