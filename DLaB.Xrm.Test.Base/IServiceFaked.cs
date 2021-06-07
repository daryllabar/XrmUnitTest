using System;
using System.Linq;

namespace DLaB.Xrm.Test
{
    /// <summary>
    /// Used to determine the interface that the given type is stored in the service provider as
    /// </summary>
    /// <typeparam name="TService"></typeparam>
    // ReSharper disable once UnusedTypeParameter
    public interface IServiceFaked<TService> { }
    /// <summary>
    /// Service used to constrain the 
    /// </summary>
    public interface IFakeService { }
}
