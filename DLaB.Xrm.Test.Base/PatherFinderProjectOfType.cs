using System;
using System.Collections.Generic;
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
        private string FallBackProjectDirectory { get; }
        private Func<string, string, string> MapAssumedProjectParentPathToActual { get; }
        private string ProjectPath { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="PatherFinderProjectOfType"/> class.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <param name="projectRelativePath">The project relative path.</param>
        /// <param name="fallBackProjectDirectory">The fallback project directory path to use.  Useful for Build Pipelines.</param>
        /// <param name="mapAssumedProjectParentPathToActual">Function to map from the assumed path to the correct one.  First Parameter is the Assumed Project Parent Path.  The Second Parameter is the Project Name.</param>
        public PatherFinderProjectOfType(Type type, string projectRelativePath = null, string fallBackProjectDirectory = null, Func<string, string, string> mapAssumedProjectParentPathToActual = null)
        {
            FallBackProjectDirectory = fallBackProjectDirectory;
            MapAssumedProjectParentPathToActual = mapAssumedProjectParentPathToActual;

            var projectPath = FindProjectOfType(type);
            if (projectRelativePath != null)
            {
                projectPath = Path.Combine(projectPath, projectRelativePath);
            }
            ProjectPath = projectPath;
        }

        private string FindProjectOfType(Type type)
        {
            var log = new List<string> { "Log: " };
            var projectName = type.AssemblyQualifiedName?.Split(',')[1].Trim();
            log.Add($"Looking for project folder for {projectName}");

            var fileNamesToCheck = new List<string>();
            fileNamesToCheck.Add(type.Assembly.Location);
#if !NET
            // XUnit moves the location of the assembly to a temp location, use CodeBase instead
            fileNamesToCheck.Add(type.Assembly.CodeBase.Substring(8));
#endif
            string projectParentDirectory = null;
            var sb = new StringBuilder();
            foreach (var fileName in fileNamesToCheck)
            {
                projectParentDirectory = GetProjectParentDirectory(fileName, sb);
                if (projectParentDirectory != null)
                {
                    break;
                }
            }
            log.Add("GetProjectParentDirectory Trace: " + sb.Replace(Environment.NewLine, Environment.NewLine + "\t"));

            if (string.IsNullOrWhiteSpace(projectParentDirectory))
            {
                throw new Exception($"Unable to find Project Path for {type.FullName}.  Assembly Located at {type.Assembly.Location}{Environment.NewLine}Files Checked:{fileNamesToCheck.ToCsv()}{GenerateLogText()}");
            }

            log.Add("Project Name " + projectName);
            log.Add("Project Parent Folder " + projectParentDirectory);
            var projectPath = Path.Combine(projectParentDirectory, projectName ?? "");

            if (!Directory.Exists(projectPath))
            {
                if(MapAssumedProjectParentPathToActual == null)
                {
                    log.Add($"Assumed Project Folder {projectPath} not found! Consider using a different IPathFinder like PathFinderAbsolute, PathFinderEnvironmentFolder, or utilizing the {nameof(PatherFinderProjectOfType)} mapAssumedProjectParentPathToActual constructor parameter to map to the correct path." );
                }
                else
                {
                    log.Add($"Assumed Project Folder {projectPath} not found! Attempting mapAssumedProjectParentPathToActual to map to the correct path.");
                    projectPath = MapAssumedProjectParentPathToActual(projectParentDirectory, projectName);
                }
            }
            log.Add("Project Folder " + projectPath);
            if (!Directory.Exists(projectPath))
            {
                throw new Exception($"Unable to find Project Path for {type.FullName} at {projectPath}. {GenerateLogText()}");
            }

            return projectPath;

            string GenerateLogText()
            {
                return Environment.NewLine + string.Join(Environment.NewLine + " - ", log);
            }
        }

        private string GetProjectParentDirectory(string dllFilePath, StringBuilder sb)
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

            if (folders.Contains("lut"))
            {
                if (folders.Contains(".vs"))
                {
                    solutionFolder = GetProjectParentDirectoryLiveUnitTest(sb, dll, folders);

                }
                else if (folders.Contains("v2"))
                {
                    solutionFolder = GetProjectParentDirectoryLiveUnitTestV2(sb, dll, folders);
                }
                else
                {
                    solutionFolder = GetProjectParentDirectory(dll);
                }
            }
            else
            {
                solutionFolder = GetProjectParentDirectory(dll);
            }

            if (solutionFolder == null && !string.IsNullOrWhiteSpace(FallBackProjectDirectory))
            {
                sb.AppendLine($"Fallback project directory used {FallBackProjectDirectory}");
                return Directory.GetParent(FallBackProjectDirectory)?.FullName;
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

        private string GetProjectParentDirectoryLiveUnitTestV2(StringBuilder sb, FileInfo dll, string[] folders)
        {
            sb.AppendLine("Checking for Live Unit Tests");
            sb.AppendLine($"Dll Path: {dll.FullName}");
            var lutDataFolderIndex = Array.IndexOf(folders, "v2");
            var lutIndex = Array.IndexOf(folders, "lut");
            if (lutDataFolderIndex <= lutIndex)
            {
                return null;
            }

            var values = folders.ToList();
            values.RemoveRange(lutDataFolderIndex, folders.Length - lutDataFolderIndex);
            var lutProjectPath = string.Join(Path.DirectorySeparatorChar + "", values);
            var lutDataPath = Path.Combine(lutProjectPath, "coverage.lutdata");
            sb.AppendLine($"Checking for .lutdata file: {lutDataPath}");
            if (!File.Exists(lutDataPath))
            {
                if (MapAssumedProjectParentPathToActual != null)
                {
                    sb.AppendLine("No coverage.lutdata file was found.  Attempting mapping of path");
                    var path = MapAssumedProjectParentPathToActual(lutProjectPath, Path.GetFileNameWithoutExtension(dll.Name));
                    if(path != null && Directory.Exists(path))
                    {
                        sb.AppendLine($"Mapped path found: {path}");
                        return path;
                    }
                }
                sb.AppendLine(@"No coverage.lutdata file was found.  In Visual Studio, first disable Live Unit Testing, then try selecting ""Test"" --> ""Analyze Code Coverage for All Tests"" to generate the coverage.lutdata file to read the project path from.");
                return null;
            }

            var lutData = File.ReadAllText(lutDataPath);
            var projectName = Path.GetFileNameWithoutExtension(dll.FullName) + ".csproj";
            var projectPathEndIndex = lutData.IndexOf(projectName, StringComparison.Ordinal);
            var projectPathStartIndex = lutData.Substring(0, projectPathEndIndex).LastIndexOf(@":\", StringComparison.Ordinal) - 1;
            var a = lutData.Substring(projectPathStartIndex, projectPathEndIndex - projectPathStartIndex);
            return GetProjectParentDirectory(a, sb);
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
            foreach (var solution in Directory.GetFiles(solutionFolder, "*.sln"))
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