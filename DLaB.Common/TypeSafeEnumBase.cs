using System;

namespace DLaB.Common
{
    /// <summary>
    /// Base class to Create Enums that are typed to something besides int, and allowed to be exapnded by other code bases
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public abstract class TypeSafeEnumBase<T>
    {
        public string Name { get; private set; }
        public T Value { get; private set; }

        protected TypeSafeEnumBase(string name, T value)
        {
            name.ThrowIfNull("name");
            Name = name;
            Value = value;
        }

        public static implicit operator T(TypeSafeEnumBase<T> t)
        {
            return t.Value;
        }

        public override string ToString()
        {
            return Value == null ? String.Empty : Value.ToString();
        }
    }

    /*
     * public class CompanyEnum : TypeSafeEnumBase<Guid>
     * {
     *     // Keep this private to prevent anyone else from creating a value.  If this is public, may have to deal with overriding Equal and HashString to 
     *     // ensure that Values are compared, and not just references
     *     private CompanyEnum(string itemName, Guid itemId)
     *         : base(itemName, itemId)
     *     {
     *         //base class implemention is sufficient
     *     }
     *
     *     public static CompanyEnum AbcCorp = new CompanyEnum("Abc Corp", new Guid("A9A3FAC6-499C-401D-8E58-5BA38B73CA21"));
     *
     *
     *     public static CompanyEnum DefCorp = new CompanyEnum("DefCorp", new Guid("0D58B7F1-89D5-4EA6-A0E6-67D795AA7CE1"));
     * }

     */


}
