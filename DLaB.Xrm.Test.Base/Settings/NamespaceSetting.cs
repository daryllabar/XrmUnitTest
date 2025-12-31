using System;
using System.Linq;
using System.Reflection;
#if NET
using DataverseUnitTest.Exceptions;

namespace DataverseUnitTest.Settings
#else
using DLaB.Xrm.Test.Exceptions;

namespace DLaB.Xrm.Test.Settings
#endif
{
    /// <summary>
    /// Defines a Path to the assembly given the namespace
    /// </summary>
    public class NamespaceSetting
    {
        private Assembly? _assembly;
        private string? _namespace;
        private string NotConfiguredMessage { get; }

        /// <summary>
        /// Gets or sets the assembly.
        /// </summary>
        /// <value>
        /// The assembly.
        /// </value>
        /// <exception cref="NotConfiguredException"></exception>
        public Assembly Assembly
        {
            get
            {
                if (_assembly == null)
                {
                    throw new NotConfiguredException(NotConfiguredMessage);
                }

                return _assembly;
            }
            protected set => _assembly = value;
        }

        /// <summary>
        /// Gets or sets the namespace.
        /// </summary>
        /// <value>
        /// The namespace.
        /// </value>
        /// <exception cref="NotConfiguredException"></exception>
        public string Namespace
        {
            get
            {
                if (_namespace == null)
                {
                    throw new NotConfiguredException(NotConfiguredMessage);
                }

                return _namespace;
            }
            protected set => _namespace = value;
        }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is configured.
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance is configured; otherwise, <c>false</c>.
        /// </value>
        public bool IsConfigured { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="NamespaceSetting"/> class.
        /// </summary>
        /// <param name="notConfiguredMessage">The not configured message.</param>
        public NamespaceSetting(string notConfiguredMessage)
        {
            NotConfiguredMessage = notConfiguredMessage;
        }

        /// <summary>
        /// Configures the assembly.
        /// </summary>
        public void ConfigureAssembly<T>()
        {
            var type = typeof(T);
            Assembly = type.Assembly;
            Namespace = type.Namespace ?? throw new NullReferenceException("No Namespace found for type " + type.FullName);
            IsConfigured = true;
        }
    }

    /// <summary>
    /// Generic Namespace Setting
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class NamespaceSetting<T> : NamespaceSetting
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="NamespaceSetting{T}"/> class.
        /// </summary>
        /// <param name="notConfiguredMessage">The not configured message.</param>
        public NamespaceSetting(string notConfiguredMessage) : base(notConfiguredMessage)
        {
        }

        /// <summary>
        /// Configures the assembly, requiring that TDerivedClass be derived from T
        /// </summary>
        /// <typeparam name="TDerivedClass">The type of the class.</typeparam>
        /// <exception cref="System.Exception">Must pass in a derived type from  + typeof(T).AssemblyQualifiedName</exception>
        public void ConfigureDerivedAssembly<TDerivedClass>() where TDerivedClass : T
        {
            if (typeof(T).IsInterface
                ? typeof(TDerivedClass) == typeof(T)
                : typeof(TDerivedClass).GetInterfaces().Contains(typeof(T)))
            {
                throw new Exception("Must pass in a derived type from " + typeof(T).AssemblyQualifiedName);
            }

            ConfigureAssembly<TDerivedClass>();
        }
    }
}
