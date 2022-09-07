using System;
using System.IO;

#if !NET
using DLaB.Xrm.Test;
#endif

namespace DataverseUnitTest
{
    public class PathFinderEnvironmentFolder: IPathFinder
    {
        private readonly Environment.SpecialFolder _folder;
        private readonly string _relativePath;

        public PathFinderEnvironmentFolder(string relativePath): this(Environment.SpecialFolder.ApplicationData, relativePath)
        {
        }

        public PathFinderEnvironmentFolder(Environment.SpecialFolder folder, string relativePath)
        {
            _folder = folder;
            _relativePath = relativePath;
        }

        public string GetPath()
        {
            return string.IsNullOrWhiteSpace(_relativePath)
                ? Environment.GetFolderPath(_folder)
                : Path.Combine(Environment.GetFolderPath(_folder), _relativePath);
        }
    }
}
