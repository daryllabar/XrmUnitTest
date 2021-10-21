using System;
using System.IO;
using System.Linq;
using System.Text;

#if NET
namespace DataverseUnitTest
#else
namespace DLaB.Xrm.Test
#endif
{
    /// <summary>
    /// Defines the path of a project
    /// </summary>
    public class PatherFinderProjectOfType : IPathFinder
    {
        private string ProjectPath { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="PatherFinderProjectOfType"/> class.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <param name="projectRelativePath">The project relative path.</param>
        public PatherFinderProjectOfType(Type type, string projectRelativePath = null)
        {
            var projectPath = FindProjectOfType(type);
            if (projectRelativePath != null)
            {
                projectPath = Path.Combine(projectPath, projectRelativePath);
            }
            ProjectPath = projectPath;
        }

        private static string FindProjectOfType(Type type)
        {
            var sb = new StringBuilder();
            var projectName  = type.AssemblyQualifiedName?.Split(',')[1].Trim();
            sb.AppendLine($"Looking for project folder for ${projectName}");

#if NET
            // NET doesn't support CodeBase
            var solutionFolder = GetProjectParentDirectory(projectName, type.Assembly.Location, sb);
#else
            // XUnit moves the location of the assembly to a temp location, use CodeBase instead
            var solutionFolder = GetProjectParentDirectory(projectName, type.Assembly.Location, sb)
                ?? GetProjectParentDirectory(projectName, type.Assembly.CodeBase.Substring(8), sb);
#endif

            if (string.IsNullOrWhiteSpace(solutionFolder))
            {
                throw new Exception($"Unable to find Project Path for {type.FullName}.  Assembly Located at {type.Assembly.Location}{Environment.NewLine}{sb}");
            }

            sb.AppendLine("Project Name" + projectName);
            sb.AppendLine("SolutionFolder " + solutionFolder);
            var projectPath = Path.Combine(solutionFolder, projectName);

            sb.AppendLine("Project Folder " + projectPath);
            if (!Directory.Exists(projectPath))
            {
                throw new Exception($"Unable to find Project Path for {type.FullName} at {projectPath}. Log {sb}");
            }

            return projectPath;
        }

        private static string GetProjectParentDirectory(string projectName, string dllFilePath, StringBuilder sb)
        {
            var dll = new FileInfo(dllFilePath);
            string solutionFolder = null;

            if (dll.Directory?.Parent?.Parent?.Parent == null) // ...\Solution
            {
                sb.AppendLine("Checking for VSOnline");
                sb.AppendLine(dll.DirectoryName);
                if (dll.DirectoryName == @"C:\a\bin")
                {
                    return GetSolutionFolderForVSOnline(sb, solutionFolder);
                }
            }

            var folders = dllFilePath.ToLower().Split(Path.DirectorySeparatorChar);

            if (folders.Contains(".vs") && folders.Contains("lut"))
            {
                solutionFolder = GetSolutionFolderForLiveUnitTest(sb, dll, folders);
            }
            else
            {
                solutionFolder = GetProjectParentDirectory(dll);
            }
            return solutionFolder;
        }

        private static string GetProjectParentDirectory(FileInfo dll)
        {
            // Check for XUnit Temp Directory
            var directory = dll.Directory;
            while (directory != null)
            {
                if (directory.GetFiles("*.csproj").Length > 0)
                {
                    return directory.Parent?.FullName;
                }
                if (directory.GetFiles("*.sln").Length > 0)
                {
                    return directory.FullName;
                }
                directory = directory.Parent;
            }

            return null;
        }

        private static string GetSolutionFolderForLiveUnitTest(StringBuilder sb, FileInfo dll, string[] folders)
        {
            string solutionFolder = null;
            sb.AppendLine("Checking for Live Unit Tests");
            sb.AppendLine(dll.DirectoryName);
            var vsIndex = Array.IndexOf(folders, ".vs");
            var lutIndex = Array.IndexOf(folders, "lut");
            if (vsIndex < lutIndex)
            {
                var values = folders.ToList();
                values.RemoveRange(vsIndex, folders.Length - vsIndex);
                solutionFolder = string.Join(Path.DirectorySeparatorChar + "", values);
            }

            return solutionFolder;
        }

        private static string GetSolutionFolderForVSOnline(StringBuilder sb, string solutionFolder)
        {
            // Build is on VSOnline.  Redirect to other c:\a\src\Branch Name
            var s = new System.Diagnostics.StackTrace(true);
            sb.AppendLine(s.ToString());
            for (var i = 0; i < s.FrameCount; i++)
            {
                var fileName = s.GetFrame(i).GetFileName();
                sb.AppendLine(fileName ?? String.Empty);
                if (!string.IsNullOrEmpty(fileName))
                {
                    // File name will be in the form of c:\a\src\Branch Name\project\filename.  Get everything up to and including the Branch Name
                    var parts = fileName.Split(Path.DirectorySeparatorChar);
                    solutionFolder = Path.Combine(parts[0] + Path.DirectorySeparatorChar + parts[1], parts[2], parts[3]);
                    sb.AppendLine(solutionFolder);
                    break;
                }
            }

            return solutionFolder;
        }


        /// <summary>
        /// Gets the path.
        /// </summary>
        /// <returns></returns>
        public string GetPath()
        {
            return ProjectPath;
        }
    }
}
