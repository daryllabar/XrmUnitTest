using System.Collections.Generic;
using System.Linq;

namespace DLaB.Xrm.Comparers
{

    /// <summary>
    /// Taken from Stake Overflow question: http://stackoverflow.com/questions/50098/comparing-two-collections-for-equality-irrespective-of-the-order-of-items-in-them
    /// Checks for Value Comparison of a collection, allowing for the collection to not be in order, as long as they contain the same count.
    /// <para>The ability for it to accept an IEqualityComparer for type T was added.</para>
    ///  </summary>
    /// <typeparam name="T"></typeparam>
    public class EnumerableComparer<T> : IEqualityComparer<IEnumerable<T>>
    {

        private IEqualityComparer<T> Comparer { get; set; }

        public EnumerableComparer(IEqualityComparer<T> comparer = null)
        {
            Comparer = comparer;
        }

        public bool Equals(IEnumerable<T> first, IEnumerable<T> second)
        {
            if (first == null)
                return second == null;

            if (second == null)
                return false;

            if (ReferenceEquals(first, second))
                return true;

            var firstCollection = first as ICollection<T>;
            var secondCollection = second as ICollection<T>;
            if (firstCollection != null && secondCollection != null)
            {
                if (firstCollection.Count != secondCollection.Count)
                    return false;

                if (firstCollection.Count == 0)
                    return true;
            }

            return !HaveMismatchedElement(first, second, Comparer);
        }

        private static bool HaveMismatchedElement(IEnumerable<T> first, IEnumerable<T> second, IEqualityComparer<T> comparer = null)
        {
            int firstCount;
            int secondCount;

            var firstElementCounts = GetElementCounts(first, out firstCount, comparer);
            var secondElementCounts = GetElementCounts(second, out secondCount, comparer);

            return firstCount != secondCount 
                   || 
                   firstElementCounts.Any(kvp => !secondElementCounts.TryGetValue(kvp.Key, out secondCount) 
                                                 ||
                                                 kvp.Value != secondCount);
        }

        private static Dictionary<T, int> GetElementCounts(IEnumerable<T> enumerable, out int nullCount, IEqualityComparer<T> comparer = null)
        {
            var dictionary = new Dictionary<T, int>(comparer);
            nullCount = 0;

            foreach (var element in enumerable)
            {
                // ReSharper disable once CompareNonConstrainedGenericWithNull
                if (element == null)
                {
                    nullCount++;
                }
                else
                {
                    int num;
                    dictionary.TryGetValue(element, out num);
                    num++;
                    dictionary[element] = num;
                }
            }

            return dictionary;
        }

        public int GetHashCode(IEnumerable<T> enumerable)
        {
            if (Comparer == null)
            {
                return enumerable.OrderBy(x => x).
                     Aggregate(new HashCode(), (current, val) => current.Hash(val));   
            }

            // Since we have to ensure the items are sorted in the same order, we could require a comparer as well, 
            // but this seems more correct, although it may be more costly.

            // ReSharper disable once CompareNonConstrainedGenericWithNull
            return enumerable.OrderBy(x => (x == null ? new HashCode() : Comparer.GetHashCode(x))).
                              Aggregate(new HashCode(), (current, val) => current.Hash(val, Comparer));
        }
    }
}
