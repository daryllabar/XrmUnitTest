#if NET
namespace DataverseUnitTest
#else
namespace DLaB.Xrm.Test
#endif
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
