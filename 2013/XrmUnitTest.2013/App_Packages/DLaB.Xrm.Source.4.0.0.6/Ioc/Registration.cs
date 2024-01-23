using System;

#if DLAB_UNROOT_NAMESPACE || DLAB_XRM
namespace DLaB.Xrm.Ioc
#else
namespace Source.DLaB.Xrm.Ioc
#endif
{
    /// <summary>
    /// Defines the method of type creation, as well as the lifetime of the instances created.
    /// </summary>
    public struct Registration : IEquatable<Registration>
    {
        /// <summary>
        /// Defines when a type will be reused vs recreated.
        /// </summary>
        public Lifetime Lifetime { get; set; }
        /// <summary>
        /// The type to be created.
        /// </summary>
        public Type Type { get; set; }
        /// <summary>
        /// Factory method to create the instance.
        /// </summary>
        public Func<IServiceProvider, object> Factory { get; set; }
        /// <summary>
        /// The instance to be used.
        /// </summary>
        public object Instance { get; set; }

        /// <summary>
        /// Creates a registration.
        /// </summary>
        /// <param name="type">Required unless factory or instance is provided.</param>
        /// <param name="lifetime">The lifetime to register the type.  Defaults to scoped.</param>
        /// <param name="factory">The function factory to call to create the type.  If not null, type is not required.</param>
        /// <param name="instance">If not null, the lifetime must be Singleton and the type is not required.</param>
        /// <exception cref="Exception"></exception>
        public Registration(Type type = null, Lifetime lifetime = Lifetime.Scoped, Func<IServiceProvider, object> factory = null, object instance = null)
        {
            if (instance != null && lifetime != Lifetime.Singleton)
            {
                throw new Exception("Instance value is only valid for Singleton!");
            }

            var reflectiveConstructorCallRequired = factory == null && instance == null;

            if (reflectiveConstructorCallRequired)
            {
                if (type == null)
                {
                    throw new Exception($@"Either {nameof(type)} or {nameof(factory)} or {nameof(instance)} must not be null!");
                }
                else if (type.IsInterface)
                {
                    throw new Exception($@"Implementation Type ({type.FullName}) must not be an interface!");
                }
            }

            Type = type;
            Lifetime = lifetime;
            Factory = factory;
            Instance = instance;
        }

        /// <summary>
        /// Determines whether the specified object is equal to the current object.
        /// </summary>
        /// <param name="obj">The object to compare with the current object.</param>
        /// <returns>True if the specified object is equal to the current object; otherwise, false.</returns>
        public override bool Equals(object obj)
        {
            if (obj == null || GetType() != obj.GetType())
            {
                return false;
            }

            return Equals((Registration)obj);
        }

        /// <summary>
        /// Determines whether the specified Registration is equal to the current Registration.
        /// </summary>
        /// <param name="other">The Registration to compare with the current Registration.</param>
        /// <returns>True if the specified Registration is equal to the current Registration; otherwise, false.</returns>
        public bool Equals(Registration other)
        {
            if (Type != other.Type
                || Lifetime != other.Lifetime
                || Instance != other.Instance)
            {
                return false;
            }

            return Factory?.Method == other.Factory?.Method;
        }

        /// <summary>
        /// Serves as the default hash function.
        /// </summary>
        /// <returns>A hash code for the current object.</returns>
        public override int GetHashCode()
        {
            unchecked
            {
                var hash = 17;
                hash = hash * 23 + Type.GetHashCode();
                hash = hash * 23 + Lifetime.GetHashCode();

                if (Factory != null)
                {
                    hash = hash * 23 + Factory.Method.GetHashCode();
                }
                else if (Instance != null)
                {
                    hash = hash * 23 + Instance.GetHashCode();
                }

                return hash;
            }
        }

        /// <summary>
        /// Determines whether two Registration objects are equal.
        /// </summary>
        /// <param name="left">The first Registration to compare.</param>
        /// <param name="right">The second Registration to compare.</param>
        /// <returns>True if the two Registration objects are equal; otherwise, false.</returns>
        public static bool operator ==(Registration left, Registration right)
        {
            return left.Equals(right);
        }

        /// <summary>
        /// Determines whether two Registration objects are not equal.
        /// </summary>
        /// <param name="left">The first Registration to compare.</param>
        /// <param name="right">The second Registration to compare.</param>
        /// <returns>True if the two Registration objects are not equal; otherwise, false.</returns>
        public static bool operator !=(Registration left, Registration right)
        {
            return !(left == right);
        }
    }
}
