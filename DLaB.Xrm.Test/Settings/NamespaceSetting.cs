using System;
using System.Reflection;
using DLaB.Xrm.Test.Exceptions;

namespace DLaB.Xrm.Test.Settings
{
    public class NamespaceSetting
    {
        private string NotConfiguredMessage { get; set; }

        private Assembly _assembly;
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
            protected set { _assembly = value; }
        }

        private String _namespace;
        public String Namespace
        {
            get
            {
                if (_namespace == null)
                {
                    throw new NotConfiguredException(NotConfiguredMessage);
                }
                return _namespace;
            }
            protected set { _namespace = value; }
        }

        public bool IsConfigured { get; set; }

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
            Namespace = type.Namespace;
            IsConfigured = true;
        }
    }

    public class NamespaceSetting<T> : NamespaceSetting
    {
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
            if (typeof(TDerivedClass) == typeof(T))
            {
                throw new Exception("Must pass in a derived type from " + typeof(T).AssemblyQualifiedName);
            }

            ConfigureAssembly<TDerivedClass>();
        }
    }
}
