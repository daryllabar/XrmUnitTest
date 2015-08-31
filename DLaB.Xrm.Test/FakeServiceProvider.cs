using System;
using System.Collections.Generic;

namespace DLaB.Xrm.Test
{
    public class FakeServiceProvider : IServiceProvider, ICloneable
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

        #region Clone

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
