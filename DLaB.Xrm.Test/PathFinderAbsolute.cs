using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DLaB.Xrm.Test
{
    public class PathFinderAbsolute : IPathFinder
    {
        private string Path { get; set; }

        public PathFinderAbsolute(string path)
        {
            Path = path;
        }

        public string GetPath()
        {
            return Path;
        }
    }
}
