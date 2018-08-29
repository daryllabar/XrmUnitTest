using System;
using System.Collections.Generic;

namespace DLaB.Xrm.Test
{
    /// <summary>
    /// A Fake that implements IServiceProvider
    /// </summary>
    public class FakeServiceProvider : IServiceProvider, ICloneable
    {
        private Dictionary<Type, object> Services { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="FakeServiceProvider"/> class.
        /// </summary>
        public FakeServiceProvider()
        {
            Services = new Dictionary<Type, object>();
        }

        /// <summary>
        /// Gets the service.
        /// </summary>
        /// <param name="serviceType">Type of the service.</param>
        /// <returns></returns>
        /// <exception cref="System.Exception">No Service Found For Type:  + serviceType.FullName</exception>
        public object GetService(Type serviceType)
        {
            if (Services.TryGetValue(serviceType, out object service))
            {
                return service;
            }

            throw new Exception("No Service Found For Type: " + serviceType.FullName);
        }

        /// <summary>
        /// Adds the object as the service to be returned when GetService is called.
        /// </summary>
        /// <param name="serviceType">Type of the service.</param>
        /// <param name="service">The service.</param>
        public void AddService(Type serviceType, Object service)
        {
            Services[serviceType] = service;
        }

        /// <summary>
        /// Adds the object as the service to be returned when GetService is called.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="service">The service.</param>
        public void AddService<T>(T service)
        {
            Services[typeof(T)] = service;
        }

        #region Clone

        /// <summary>
        /// Clones this instance.
        /// </summary>
        /// <returns></returns>
        public FakeServiceProvider Clone()
        {
            var clone = (FakeServiceProvider)MemberwiseClone();
            clone.Services = new Dictionary<Type, object>();
            foreach (var value in Services)
            {
                var cloneableService = value.Value as ICloneable;
                if (cloneableService == null)
                {
                    clone.Services[value.Key] = value.Value;
                }
                else
                {
                    clone.Services[value.Key] = cloneableService.Clone();
                }
            }

            return clone;
        }

        object ICloneable.Clone()
        {
            return Clone();
        }

        #endregion Clone
    }
}
