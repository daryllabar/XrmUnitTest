using System;
using System.IO;

#if !NET
using DLaB.Xrm.Test;
#endif

namespace DataverseUnitTest
{
    /// <summary>
    /// Finds a relative path using an environment folder as the root path.
    /// </summary>
    public class PathFinderEnvironmentFolder: IPathFinder
    {
        private readonly Environment.SpecialFolder _folder;
        private readonly string _relativePath;

        /// <summary>
        /// Defaults to using the Environment.SpecialFolder.ApplicationData Folder.
        /// </summary>
        /// <param name="relativePath">The path.</param>
        public PathFinderEnvironmentFolder(string relativePath): this(Environment.SpecialFolder.ApplicationData, relativePath)
        {
        }

        /// <summary>
        /// Uses the given special Folder and relative path.
        /// </summary>
        /// <param name="folder">The Special Folder.</param>
        /// <param name="relativePath">The path.</param>
        public PathFinderEnvironmentFolder(Environment.SpecialFolder folder, string relativePath)
        {
            _folder = folder;
            _relativePath = relativePath;
        }

        /// <summary>
        /// Returns the path.
        /// </summary>
        public string GetPath()
        {
            return string.IsNullOrWhiteSpace(_relativePath)
                ? Environment.GetFolderPath(_folder)
                : Path.Combine(Environment.GetFolderPath(_folder), _relativePath);
        }
    }
}
