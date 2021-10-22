using System;
using System.IO;
using System.Linq;
using System.Text;
using DLaB.Common;

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
            sb.AppendLine($"Looking for project folder for {projectName}");

#if NET
            // NET doesn't support CodeBase
            var projectParentDirectory = GetProjectParentDirectory(type.Assembly.Location, sb);
#else
            // XUnit moves the location of the assembly to a temp location, use CodeBase instead
            var projectParentDirectory = GetProjectParentDirectory(type.Assembly.Location, sb)
                ?? GetProjectParentDirectory(type.Assembly.CodeBase.Substring(8), sb);

#endif

            if (string.IsNullOrWhiteSpace(projectParentDirectory))
            {
                throw new Exception($"Unable to find Project Path for {type.FullName}.  Assembly Located at {type.Assembly.Location}{Environment.NewLine}{sb}");
            }

            sb.AppendLine("Project Name " + projectName);
            sb.AppendLine("Project Parent Folder " + projectParentDirectory);
            var projectPath = Path.Combine(projectParentDirectory, projectName);

            sb.AppendLine("Project Folder " + projectPath);
            if (!Directory.Exists(projectPath))
            {
                throw new Exception($"Unable to find Project Path for {type.FullName} at {projectPath}. Log {sb}");
            }

            return projectPath;
        }

        private static string GetProjectParentDirectory(string dllFilePath, StringBuilder sb)
        {
            var dll = new FileInfo(dllFilePath);

            if (dll.Directory?.Parent?.Parent?.Parent == null) // ...\Solution
            {
                sb.AppendLine("Checking for VSOnline");
                sb.AppendLine(dll.DirectoryName);
                if (dll.DirectoryName == @"C:\a\bin")
                {
                    return GetSolutionFolderForVSOnline(sb, dllFilePath);
                }
            }

            var folders = dllFilePath.ToLower().Split(Path.DirectorySeparatorChar);
            string solutionFolder;

            if (folders.Contains(".vs") && folders.Contains("lut"))
            {
                solutionFolder = GetProjectParentDirectoryLiveUnitTest(sb, dll, folders);
            }
            else
            {
                solutionFolder = GetProjectParentDirectory(dll);
            }

            sb.AppendLine($"Parent Directory of Project {solutionFolder}");
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

        private static string GetProjectParentDirectoryLiveUnitTest(StringBuilder sb, FileInfo dll, string[] folders)
        {
            sb.AppendLine("Checking for Live Unit Tests");
            sb.AppendLine($"Dll Path: {dll.FullName}");
            var vsIndex = Array.IndexOf(folders, ".vs");
            var lutIndex = Array.IndexOf(folders, "lut");
            if (vsIndex >= lutIndex)
            {
                return null;
            }

            var values = folders.ToList();
            values.RemoveRange(vsIndex, folders.Length - vsIndex);
            var solutionFolder = string.Join(Path.DirectorySeparatorChar + "", values);
            sb.AppendLine($"Solution folder: {solutionFolder}");
            sb.AppendLine("Finding nested project folder.");

            return GetProjectParentDirectoryLiveUnitTestFromDirectoryPath(sb, folders, lutIndex, solutionFolder)
                ?? GetProjectPathFromSolutionFile(sb, dll, solutionFolder);
        }

        private static string GetProjectParentDirectoryLiveUnitTestFromDirectoryPath(StringBuilder sb, string[] folders, int lutIndex, string solutionFolder)
        {
            var values = folders.ToList();
            var currentFolder = solutionFolder;
            foreach (var folder in values.Skip(lutIndex + 1))
            {
                if (!Directory.Exists(Path.Combine(currentFolder, folder)))
                {
                    continue;
                }

                currentFolder = Path.Combine(currentFolder, folder);
                if (Directory.GetFiles(currentFolder, "*.csproj").Length > 0)
                {
                    sb.AppendLine($"Project folder found: {currentFolder}");
                    {
                        return new DirectoryInfo(currentFolder).Parent?.FullName;
                    }
                }
            }

            return null;
        }

        private static string GetProjectPathFromSolutionFile(StringBuilder sb, FileInfo dll, string solutionFolder)
        {
            var solution = Directory.GetFiles(solutionFolder, "*.sln").FirstOrDefault();
            if (solution != null)
            {
                sb.AppendLine($"Project Folder not found.  Attempting to parse solution file {solution}.");
                var searchText = $"\\{Path.GetFileNameWithoutExtension(dll.Name)}.csproj\", \"";
                sb.AppendLine($"Searching for project file text {searchText} in solution file.");
                var line = File.ReadAllLines(solution).FirstOrDefault(l => l.ContainsIgnoreCase(searchText));
                if (line != null)
                {
                    var endIndex = line.IndexOf(searchText, StringComparison.InvariantCultureIgnoreCase);
                    var startIndex = line.Substring(0, endIndex).LastIndexOf("\"", StringComparison.InvariantCulture) + 1;
                    var currentFolder = Path.Combine(solutionFolder, line.Substring(startIndex, endIndex - startIndex));
                    sb.AppendLine("Parsed Project folder to be " + currentFolder);
                    return new DirectoryInfo(currentFolder).Parent?.FullName;
                }
            }

            return null;
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