#if NET
namespace DataverseUnitTest
#else
namespace DLaB.Xrm.Test
#endif
{
    /// <summary>
    /// Defines the Abolute Path
    /// </summary>
    public class PathFinderAbsolute : IPathFinder
    {
        private string Path { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="PathFinderAbsolute"/> class.
        /// </summary>
        /// <param name="path">The path.</param>
        public PathFinderAbsolute(string path)
        {
            Path = path;
        }

        /// <summary>
        /// Gets the path.
        /// </summary>
        /// <returns></returns>
        public string GetPath()
        {
            return Path;
        }
    }
}
