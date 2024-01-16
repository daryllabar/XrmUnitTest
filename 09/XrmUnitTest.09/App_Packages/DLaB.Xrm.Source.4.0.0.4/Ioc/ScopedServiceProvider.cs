using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

#if DLAB_UNROOT_NAMESPACE || DLAB_XRM
namespace DLaB.Xrm.Ioc
#else
namespace Source.DLaB.Xrm.Ioc
#endif
{

    /// <summary>
    /// Utilizes the registrations to create instances whose scope is itself.
    /// </summary>
    public class ScopedServiceProvider : IServiceProvider
    {
        private readonly Dictionary<Type, Registration> _registrations = new Dictionary<Type, Registration>();
        private readonly Dictionary<Type, object> _scopedInstances = new Dictionary<Type, object>();
        private readonly ConcurrentDictionary<Type, object> _instances;
        private readonly IServiceProvider _scopedProvider;
        private readonly Stack<Type> _currentRequestedTypes = new Stack<Type>();

        /// <summary>
        /// The number of items registered in the container.
        /// </summary>
        public int Count => _registrations.Count;

        /// <summary>
        /// The default lifetime to use when registering a concrete type upon request.  Null value means don't register any new types, just return null.
        /// </summary>
        public Lifetime? DefaultLifetime { get; set; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="scopedProvider">A service provider to attempt to get a type for, if the type is not defined in the registrations.</param>
        /// <param name="registrations">List of registrations by Type.  This is copied so future changes won't affect this provider instance.</param>
        /// <param name="instances">All Singleton Instances.</param>
        /// <exception cref="ArgumentNullException"></exception>
        public ScopedServiceProvider(IServiceProvider scopedProvider, Dictionary<Type, Registration> registrations, ConcurrentDictionary<Type, object> instances)
        {
            _scopedInstances[typeof(IServiceProvider)] = this;
            _registrations[typeof(IServiceProvider)] = new Registration(typeof(IServiceProvider), Lifetime.Scoped, s => this);
            registrations = registrations ?? throw new ArgumentNullException(nameof(registrations));
            _instances = instances ?? throw new ArgumentNullException(nameof(instances));
            foreach (var kvp in registrations)
            {
                _registrations[kvp.Key] = kvp.Value;
            }
            _scopedProvider = scopedProvider;
            if (_scopedProvider != null)
            {
                _registrations[typeof(WrappedServiceProvider)] = new Registration(typeof(WrappedServiceProvider), Lifetime.Scoped, s => new WrappedServiceProvider(_scopedProvider));
            }
        }

        /// <inheritdoc />
        public object GetService(Type serviceType)
        {
            if (serviceType == null) throw new ArgumentNullException(nameof(serviceType));

            if (_registrations.TryGetValue(serviceType, out var registration))
            {
                return GetService(serviceType, registration);
            }

            var service = _scopedProvider?.GetService(serviceType);

            if (service != null
                || serviceType.IsInterface
                || !serviceType.IsClass
                || serviceType.IsAbstract
                || serviceType.IsPrimitive
                || serviceType.IsEnum
                || serviceType.IsArray
                || serviceType == typeof(string)
                || DefaultLifetime == null)
            {
                return service;
            }

            // Type is a concrete type, register it.
            registration = new Registration(serviceType, DefaultLifetime.Value);
            _registrations[serviceType] = registration;
            return GetService(serviceType, registration);
        }

        private object GetService(Type serviceType, Registration registration)
        {
            object instance;
            switch (registration.Lifetime)
            {
                case Lifetime.Singleton:
                    instance = _instances.GetOrAdd(serviceType, (t) => CreateInstance(registration));
                    break;
                case Lifetime.Scoped:
                    if (!_scopedInstances.TryGetValue(serviceType, out var scopedInstance))
                    {
                        scopedInstance = CreateInstance(registration);
                        _scopedInstances[serviceType] = scopedInstance;
                    }

                    instance = scopedInstance;
                    break;
                case Lifetime.Transient:
                    instance = CreateInstance(registration);
                    break;
                default:
                    throw new NotImplementedException($"Lifetime {registration.Lifetime} ({(int)registration.Lifetime}) not implemented!");
            }

            if (instance == null)
            {
                throw new InvalidOperationException($"Unable to create instance of type {serviceType.FullName}.");
            }
            return instance;
        }

        private object CreateInstance(Registration registration)
        {
            if (registration.Instance != null)
            {
                return registration.Instance;
            }

            if (registration.Factory != null)
            {
                var value = registration.Factory(this);
                if (value == null)
                {
                    throw new InvalidOperationException($"Factory for type {registration.Type.FullName} returned null.");
                }

                return value;
            }

            if (_currentRequestedTypes.Contains(registration.Type))
            {
                var calls = string.Join("," + Environment.NewLine, _currentRequestedTypes.Select(t => "  - " + t.FullName));
                throw new InvalidOperationException($"Circular dependency detected for type {registration.Type.FullName}.{Environment.NewLine}{calls}");
            }
            _currentRequestedTypes.Push(registration.Type);

            if (registration.Type.IsGenericType && registration.Type.GetGenericTypeDefinition() == typeof(Lazy<>))
            {
                var lazyType = registration.Type.GetGenericArguments()[0];
                return LazyActivator.CreateLazyInstance(this, lazyType);
            }

            try
            {
                var constructor = registration.Type.GetConstructors()[0];
                var constructorParameters = constructor.GetParameters();
                if (constructorParameters.Length == 0)
                {
                    return Activator.CreateInstance(registration.Type);
                }

                var parameters = constructorParameters.Select(parameterInfo => GetService(parameterInfo.ParameterType)).ToArray();
                return constructor.Invoke(parameters);
            }
            finally
            {
                _currentRequestedTypes.Pop();
            }
        }

        /// <summary>
        /// Attempting to create a generic lazy instance is not easy.  This is the best I could come up with.
        /// </summary>
        private class LazyActivator
        {
            /// <summary>
            /// Creates a generic lazy instance.
            /// </summary>
            /// <param name="serviceProvider">The service provider.</param>
            /// <param name="type">The type of the lazy instance.</param>
            /// <returns>The created lazy instance.</returns>
            [DebuggerStepThrough]
            public static object CreateLazyInstance(IServiceProvider serviceProvider, Type type)
            {
                var method = typeof(LazyActivator).GetMethod(nameof(TypedCreateLazyInstance)) ?? throw new InvalidOperationException($"Unable to find method {nameof(TypedCreateLazyInstance)}");
                var genericMethod = method.MakeGenericMethod(type);
                return genericMethod.Invoke(null, new object[] { serviceProvider });
            }

            /// <summary>
            /// Creates a typed lazy instance.
            /// </summary>
            /// <typeparam name="T">The type of the lazy instance.</typeparam>
            /// <param name="serviceProvider">The service provider.</param>
            /// <returns>The created lazy instance.</returns>
            [DebuggerStepThrough, DebuggerStepperBoundary]
            public static Lazy<T> TypedCreateLazyInstance<T>(IServiceProvider serviceProvider)
            {
                var helper = new Helper(serviceProvider);
                return new Lazy<T>(helper.GetService<T>);
            }

            /// <summary>
            /// Required in order to not result in a debugger step into event when stepping into a Lazy&lt;T&gt;.Value line the first time.
            /// </summary>
            private class Helper
            {
                private readonly IServiceProvider _provider;

                /// <summary>
                /// Initializes a new instance of the <see cref="Helper"/> class.
                /// </summary>
                /// <param name="provider">The service provider.</param>
                public Helper(IServiceProvider provider)
                {
                    _provider = provider;
                }

                /// <summary>
                /// Gets the service of the specified type.
                /// </summary>
                /// <typeparam name="T">The type of the service.</typeparam>
                /// <returns>The service object of the specified type.</returns>
                [DebuggerStepThrough]
                public T GetService<T>() { return (T)_provider.GetService(typeof(T)); }
            }
        }
    }
}
