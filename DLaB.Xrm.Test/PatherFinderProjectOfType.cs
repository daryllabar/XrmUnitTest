using System;
using System.IO;
using System.Text;

namespace DLaB.Xrm.Test
{
    public class PatherFinderProjectOfType : IPathFinder
    {
        private String ProjectPath { get; set; }

        public PatherFinderProjectOfType(Type type, string projectRelativePath = null)
        {
            var projectPath = FindProjectOfType(type);
            if (projectRelativePath != null)
            {
                projectPath = Path.Combine(projectPath, projectRelativePath);
            }
            ProjectPath = projectPath;
        }

        private string FindProjectOfType(Type type)
        {
            var sb = new StringBuilder();
            // XUnit moves the location of the assmbley to a temp location, use CodeBase instead
            var solutionFolder = GetSolutionFolder(type.Assembly.Location, sb) ?? GetSolutionFolder(type.Assembly.CodeBase.Substring(8), sb);

            if (string.IsNullOrWhiteSpace(solutionFolder))
            {
                throw new Exception($"Unable to find Project Path for {type.FullName}.  Assembly Located at {type.Assembly.Location}{Environment.NewLine}{sb}");
            }

            // Class Name, Project Name, Version, Culture, PublicKeyTyoken
            // ReSharper disable once PossibleNullReferenceException
            var projectName = type.AssemblyQualifiedName.Split(',')[1].Trim();
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

        private static string GetSolutionFolder(string dllFilePath, StringBuilder sb)
        {
            var dll = new FileInfo(dllFilePath);
            string solutionFolder = null;

            if (dll.Directory?.Parent?.Parent?.Parent == null) // ...\Solution
            {
                sb.AppendLine("Checking for VSOnline");
                sb.AppendLine(dll.DirectoryName);
                if (dll.DirectoryName == @"C:\a\bin")
                {
                    // Build is on VSOnline.  Redirect to other c:\a\src\Branch Name
                    var s = new System.Diagnostics.StackTrace(true);
                    sb.AppendLine(s.ToString());
                    for (var i = 0; i < s.FrameCount; i++)
                    {
                        var fileName = s.GetFrame(i).GetFileName();
                        sb.AppendLine(fileName ?? String.Empty);
                        if (!String.IsNullOrEmpty(fileName))
                        {
                            // File name will be in the form of c:\a\src\Branch Name\project\filename.  Get everything up to and including the Branch Name
                            var parts = fileName.Split(Path.DirectorySeparatorChar);
                            solutionFolder = Path.Combine(parts[0] + Path.DirectorySeparatorChar + parts[1], parts[2], parts[3]);
                            sb.AppendLine(solutionFolder);
                            break;
                        }
                    }
                }
            }
            else
            {
                // Check for XUnit Temp Directory
                if (dll.Directory.Parent.Parent.Parent.Name.ToLower() == "assembly" && dll.Directory.Parent.Parent.Name.ToLower() == "dl3")
                {
                    // Return null and let recall happen with CodeBase rather than location
                    return null;
                }
                //  ..\Solution\Project\bin\Build 
                solutionFolder = dll.Directory.Parent.Parent.Parent.FullName;
            }
            return solutionFolder;
        }

        public string GetPath()
        {
            return ProjectPath;
        }
    }
}
