// ----------------------------------------------------------------------------------
// <copyright file="EntityPropertyCloner.cs" company="NMemory Team">
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
#pragma warning disable 8604
#pragma warning disable 8618
using System.Collections.Generic;
using Microsoft.Xrm.Sdk;

namespace NMemory.Tables
{
    using System;
    using System.Linq.Expressions;
    using System.Reflection;

    internal class EntityPropertyCloner<TEntity>
    {
        public static EntityPropertyCloner<TEntity> Instance = new EntityPropertyCloner<TEntity>();

        private readonly Action<TEntity, TEntity> entityPropertyCloner;

        public EntityPropertyCloner()
        {

            if (typeof (Entity).IsAssignableFrom(typeof (TEntity)))
            {
                this.entityPropertyCloner = CrmEntityCloner;
            }
            else
            {
                ParameterExpression source = Expression.Parameter(typeof(TEntity));
                ParameterExpression destination = Expression.Parameter(typeof(TEntity));

                PropertyInfo[] properties = typeof (TEntity).GetProperties();
                Expression[] setters = new Expression[properties.Length];

                for (int i = 0; i < properties.Length; i++)
                {
                    PropertyInfo property = properties[i];

                    // destination.Property = source.Property;
                    setters[i] = Expression.Call(destination, property.GetSetMethod(), Expression.Call(source, property.GetGetMethod()));
                }

                // DDL - Added if Statement because Any() method was creating an error since no actual properties are being set
                if (setters.Length > 0)
                {
                    BlockExpression body = Expression.Block(setters);

                    this.entityPropertyCloner = Expression.Lambda<Action<TEntity, TEntity>>(body, source, destination).Compile();
                }
            }
        }

        private void CrmEntityCloner(TEntity s, TEntity d)
        {
            var source = (s as Entity);
            var destination = (d as Entity);
            if (source == null)
            {
                throw new Exception("Couldn't cast value to Entity");
            }
            if (destination == null)
            {
                throw new Exception("Couldn't cast value to Entity");
            }
            destination.Attributes.Clear();
            destination.Attributes.AddRange(source.Attributes);
            if (destination.Id != source.Id)
            {
                // Issues using Linq To Create.  May need to figure out how to make it not readonly.
                destination.Id = source.Id;
            }
            if (destination.EntityState != source.EntityState)
            {
                // Issues using Linq To Create.  May need to figure out how to make it not readonly.
                destination.EntityState = source.EntityState;
            }
            destination.ExtensionData = source.ExtensionData;
            destination.FormattedValues.Clear();
            destination.FormattedValues.AddRange(source.FormattedValues);
            destination.RelatedEntities.Clear();
            destination.RelatedEntities.AddRange(source.RelatedEntities);
        }

        public void Clone(TEntity source, TEntity destination)
        {
            this.entityPropertyCloner.Invoke(source, destination);
        }
        
    }
}
