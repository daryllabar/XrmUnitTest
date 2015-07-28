using System;
using System.Collections.Generic;

namespace DLaB.Xrm.Test
{
    public class FakeServiceProvider : IServiceProvider
    {
        private Dictionary<Type, object> Services { get; set; }

        public FakeServiceProvider()
        {
            Services = new Dictionary<Type, object>();
        }

        public object GetService(Type serviceType)
        {
            object service;
            if (Services.TryGetValue(serviceType, out service))
            {
                return service;
            }
            
            throw new Exception("No Service Found For Type: " + serviceType.FullName);
        }

        public void AddService(Type serviceType, Object service)
        {
            Services[serviceType] = service;
        }

        public void AddService<T>(T service)
        {
            Services[typeof(T)] = service;
        }
    }
}
