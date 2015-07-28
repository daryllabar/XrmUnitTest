using System;
using System.Collections.Generic;
using DLaB.Common;

namespace DLaB.Xrm.Comparers
{
    /// <summary>
    /// Taken from http://stackoverflow.com/questions/716552/can-you-create-a-simple-equalitycomparert-using-a-lamba-expression,
    /// which Jon Skeet copied from his MiscUtil.
    /// </summary>
    /// 
    public static class ProjectionEqualityComparer
    {
        public static ProjectionEqualityComparer<TSource, TKey> Create<TSource, TKey>(Func<TSource, TKey> projection)
        {
            return new ProjectionEqualityComparer<TSource, TKey>(projection);
        }

        public static ProjectionEqualityComparer<TSource, TKey> Create<TSource, TKey>(TSource ignored, Func<TSource, TKey> projection)
        {
            return new ProjectionEqualityComparer<TSource, TKey>(projection);
        }
    }

    public static class ProjectionEqualityComparer<TSource>
    {
        public static ProjectionEqualityComparer<TSource, TKey>
            Create<TKey>(Func<TSource, TKey> projection)
        {
            return new ProjectionEqualityComparer<TSource, TKey>(projection);
        }
    }

    public class ProjectionEqualityComparer<TSource, TKey>
        : IEqualityComparer<TSource>
    {
        readonly Func<TSource, TKey> _projection;
        readonly IEqualityComparer<TKey> _comparer;

        public ProjectionEqualityComparer(Func<TSource, TKey> projection)
            : this(projection, null)
        {
        }

        public ProjectionEqualityComparer(
            Func<TSource, TKey> projection,
            IEqualityComparer<TKey> comparer)
        {
            projection.ThrowIfNull("projection");
            _comparer = comparer ?? EqualityComparer<TKey>.Default;
            _projection = projection;
        }

        public bool Equals(TSource x, TSource y)
        {
            // ReSharper disable CompareNonConstrainedGenericWithNull
            if (x == null && y == null)
            {
                return true;
            }
            if (x == null || y == null)
            {
                return false;
            }
            // ReSharper restore CompareNonConstrainedGenericWithNull

            return _comparer.Equals(_projection(x), _projection(y));
        }

        public int GetHashCode(TSource obj)
        {
            // ReSharper disable once CompareNonConstrainedGenericWithNull
            if (obj == null)
            {
                throw new ArgumentNullException("obj");
            }
            return _comparer.GetHashCode(_projection(obj));
        }
    }
}
