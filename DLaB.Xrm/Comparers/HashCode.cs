using System.Collections.Generic;

namespace DLaB.Xrm.Comparers
{

    /// <summary>
    /// Taken from http://stackoverflow.com/a/18613926/227436 an answer for http://stackoverflow.com/questions/263400/what-is-the-best-algorithm-for-an-overridden-system-object-gethashcode
    /// Changed methods so it was a new, followed by Hash.
    /// Added overload for Comparers
    /// </summary>
    public struct HashCode
    {
        private const int SEED_PRIME = 17;
        private const int MULTIPER_PRIME = 23;
        private readonly int _Hash;
        private readonly bool _HashIsSet;

        private HashCode(int hash)
        {
            _Hash = hash;
            _HashIsSet = true;
        }

        public static implicit operator int(HashCode hashCode)
        {
            return hashCode.GetHashCode();
        }

        public HashCode Hash<T>(T obj, IEqualityComparer<T> comparer = null)
        {
            var h = 0;
            // ReSharper disable once CompareNonConstrainedGenericWithNull
            if (obj != null) {
                h = comparer == null ? obj.GetHashCode() : comparer.GetHashCode(obj);   
            }
            unchecked { h += GetHashCode() * MULTIPER_PRIME; }
            return new HashCode(h);
        }


        public override int GetHashCode()
        {
            return _HashIsSet ? _Hash : SEED_PRIME;
        }
    }
}
