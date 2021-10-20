#if NET
namespace DataverseUnitTest
#else
namespace DLaB.Xrm.Test
#endif
{
    /// <summary>
    /// Defines an interface for getting a path
    /// </summary>
    public interface IPathFinder
    {
        /// <summary>
        /// Gets the path.
        /// </summary>
        /// <returns></returns>
        string GetPath();
    }
}
