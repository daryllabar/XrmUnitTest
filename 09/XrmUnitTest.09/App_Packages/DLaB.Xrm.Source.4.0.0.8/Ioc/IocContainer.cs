using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

#if DLAB_UNROOT_NAMESPACE || DLAB_XRM
namespace DLaB.Xrm.Ioc
#else
namespace Source.DLaB.Xrm.Ioc
#endif
{
    /// <summary>
    /// IOC Container that Implements IIocContainer
    /// </summary>
    public class IocContainer : IIocContainer
    {
        private readonly Dictionary<Type, Registration> _registrations = new Dictionary<Type, Registration>();
        private readonly ConcurrentDictionary<Type, object> _instances = new ConcurrentDictionary<Type, object>();

        /// <summary>
        /// Defines how duplicate type registrations should be handled.  Defaults to Override
        /// </summary> 
        public DuplicateRegistrationStrategy DuplicateRegistrationStrategy { get; set; } = DuplicateRegistrationStrategy.Override;

        private IocContainer AddRegistration(Type serviceType, Registration registration)
        {
            switch (DuplicateRegistrationStrategy)
            {
                case DuplicateRegistrationStrategy.Override:
                    _registrations[serviceType] = registration;
                    break;
                case DuplicateRegistrationStrategy.Ignore:
                    if (!_registrations.ContainsKey(serviceType))
                    {
                        _registrations[serviceType] = registration;
                    }
                    break;
                case DuplicateRegistrationStrategy.Throw:
                    if (_registrations.ContainsKey(serviceType))
                    {
                        throw new InvalidOperationException($"Duplicate registration for type {serviceType.Name}");
                    }
                    break;
                default:
                    throw new NotImplementedException($"Duplicate Registration Strategy {DuplicateRegistrationStrategy} ({(int)DuplicateRegistrationStrategy} is not implemented!)");
            }

            return this;
        }

        #region IIocContainer

        #region Scoped

        /// <summary>
        /// Adds a scoped registration for the specified service type.
        /// </summary>
        /// <param name="serviceType">The type of the service.</param>
        /// <returns>The current instance of the IIocContainer.</returns>
        public IIocContainer AddScoped(Type serviceType)
        {
            return AddRegistration(serviceType, new Registration(serviceType));
        }

        /// <summary>
        /// Adds a scoped registration for the specified service type and implementation type.
        /// </summary>
        /// <param name="serviceType">The type of the service.</param>
        /// <param name="implementationType">The type of the implementation.</param>
        /// <returns>The current instance of the IIocContainer.</returns>
        public IIocContainer AddScoped(Type serviceType, Type implementationType)
        {
            return AddRegistration(serviceType, new Registration(implementationType));
        }

        /// <summary>
        /// Adds a scoped registration for the specified service type and implementation factory.
        /// </summary>
        /// <param name="serviceType">The type of the service.</param>
        /// <param name="implementationFactory">The factory function that creates the implementation.</param>
        /// <returns>The current instance of the IIocContainer.</returns>
        public IIocContainer AddScoped(Type serviceType, Func<IServiceProvider, object> implementationFactory)
        {
            return AddRegistration(serviceType, new Registration(factory: implementationFactory));
        }

        /// <inheritdoc />
        public IIocContainer AddScoped<TService>()
        {
            return AddScoped(typeof(TService));
        }

        /// <inheritdoc />
        public IIocContainer AddScoped<TService>(Func<IServiceProvider, TService> implementationFactory)
        {
            return AddScoped(typeof(TService), (Func<IServiceProvider, object>)(Delegate)implementationFactory);
        }

        /// <inheritdoc />
        public IIocContainer AddScoped<TService, TImplementation>() where TImplementation : TService
        {
            return AddScoped(typeof(TService), typeof(TImplementation));
        }

        #endregion Scoped

        #region Singleton

        /// <summary>
        /// Adds a singleton registration for the specified service type.
        /// </summary>
        /// <param name="serviceType">The type of the service.</param>
        /// <returns>The current instance of the IIocContainer.</returns>
        public IIocContainer AddSingleton(Type serviceType)
        {
            AddRegistration(serviceType, new Registration(serviceType, Lifetime.Singleton));
            ClearSingletonInstance(serviceType);
            return this;
        }

        /// <summary>
        /// Adds a singleton registration for the specified service type and implementation type.
        /// </summary>
        /// <param name="serviceType">The type of the service.</param>
        /// <param name="implementationType">The type of the implementation.</param>
        /// <returns>The current instance of the IIocContainer.</returns>
        public IIocContainer AddSingleton(Type serviceType, Type implementationType)
        {
            AddRegistration(serviceType, new Registration(implementationType, Lifetime.Singleton));
            ClearSingletonInstance(serviceType);
            return this;
        }

        /// <summary>
        /// Adds a singleton registration for the specified service type and implementation instance.
        /// </summary>
        /// <param name="serviceType">The type of the service.</param>
        /// <param name="implementationInstance">The instance of the implementation.</param>
        /// <returns>The current instance of the IIocContainer.</returns>
        public IIocContainer AddSingleton(Type serviceType, object implementationInstance)
        {
            AddRegistration(serviceType, new Registration(lifetime: Lifetime.Singleton, instance: implementationInstance));
            ClearSingletonInstance(serviceType);
            return this;
        }

        /// <summary>
        /// Adds a singleton registration for the specified service type and implementation factory.
        /// </summary>
        /// <param name="serviceType">The type of the service.</param>
        /// <param name="implementationFactory">The factory function that creates the implementation.</param>
        /// <returns>The current instance of the IIocContainer.</returns>
        public IIocContainer AddSingleton(Type serviceType, Func<IServiceProvider, object> implementationFactory)
        {
            AddRegistration(serviceType, new Registration(lifetime: Lifetime.Singleton, factory: implementationFactory));
            ClearSingletonInstance(serviceType);
            return this;
        }

        /// <summary>
        /// Clears the singleton instance of the specified service type.
        /// </summary>
        /// <param name="serviceType">The type of the service.</param>
        private void ClearSingletonInstance(Type serviceType)
        {
            // Registration has been updated, so remove any existing instance
            _instances.TryRemove(serviceType, out _);
        }

        /// <inheritdoc />
        public IIocContainer AddSingleton<TService>()
        {
            return AddSingleton(typeof(TService));
        }

        /// <inheritdoc />
        public IIocContainer AddSingleton<TService>(Func<IServiceProvider, TService> implementationFactory)
        {
            return AddSingleton(typeof(TService), (Func<IServiceProvider, object>)(Delegate)implementationFactory);
        }

        /// <inheritdoc />
        public IIocContainer AddSingleton<TService>(TService implementationInstance)
        {
            return AddSingleton(typeof(TService), implementationInstance);
        }

        /// <inheritdoc />
        public IIocContainer AddSingleton<TService, TImplementation>() where TImplementation : TService
        {
            return AddSingleton(typeof(TService), typeof(TImplementation));
        }

        #endregion Singleton

        #region Transient

        /// <summary>
        /// Adds a transient registration for the specified service type.
        /// </summary>
        /// <param name="serviceType">The type of the service.</param>
        /// <returns>The current instance of the IIocContainer.</returns>
        public IIocContainer AddTransient(Type serviceType)
        {
            return AddRegistration(serviceType, new Registration(serviceType, Lifetime.Transient));
        }

        /// <summary>
        /// Adds a transient registration for the specified service type and implementation type.
        /// </summary>
        /// <param name="serviceType">The type of the service.</param>
        /// <param name="implementationType">The type of the implementation.</param>
        /// <returns>The current instance of the IIocContainer.</returns>
        public IIocContainer AddTransient(Type serviceType, Type implementationType)
        {
            return AddRegistration(serviceType, new Registration(implementationType, Lifetime.Transient));
        }

        /// <summary>
        /// Adds a transient registration for the specified service type and implementation factory.
        /// </summary>
        /// <param name="serviceType">The type of the service.</param>
        /// <param name="implementationFactory">The factory function that creates the implementation.</param>
        /// <returns>The current instance of the IIocContainer.</returns>
        public IIocContainer AddTransient(Type serviceType, Func<IServiceProvider, object> implementationFactory)
        {
            return AddRegistration(serviceType, new Registration(lifetime: Lifetime.Transient, factory: implementationFactory));
        }

        /// <inheritdoc />
        public IIocContainer AddTransient<TService>()
        {
            return AddTransient(typeof(TService));
        }

        /// <inheritdoc />
        public IIocContainer AddTransient<TService>(Func<IServiceProvider, TService> implementationFactory)
        {
            return AddTransient(typeof(TService), (Func<IServiceProvider, object>)(Delegate)implementationFactory);
        }

        /// <inheritdoc />
        public IIocContainer AddTransient<TService, TImplementation>() where TImplementation : TService
        {
            return AddTransient(typeof(TService), typeof(TImplementation));
        }

        #endregion Transient


        /// <inheritdoc />
        public IServiceProvider BuildServiceProvider(IServiceProvider fallbackProvider = null, Lifetime defaultLifetime = Lifetime.Scoped)
        {
            return new ScopedServiceProvider(fallbackProvider, _registrations, _instances)
            {
                DefaultLifetime = defaultLifetime
            };
        }

        /// <inheritdoc />
        public bool IsRegistered<TService>()
        {
            return _registrations.ContainsKey(typeof(TService));
        }

        /// <inheritdoc />
        public IIocContainer Remove<TService>()
        {
            _registrations.Remove(typeof(TService));
            return this;
        }

        #endregion IIocContainer
    }
}
