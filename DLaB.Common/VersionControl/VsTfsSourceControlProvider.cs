using System;
using System.IO;

namespace DLaB.Common.VersionControl
{
    /// <summary>
    /// Class to handle Checkingout from TFS
    /// </summary>
    public class VsTfsSourceControlProvider : ISourceControlProvider
    {
        private string TfPath { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="VsTfsSourceControlProvider"/> class.
        /// </summary>
        /// <param name="tfPath">The tf path.</param>
        public VsTfsSourceControlProvider(string tfPath = null)
        {
            TfPath = tfPath ?? Config.GetAppSettingOrDefault("DLaB.Common.VersionControl.TfsPath", GetDefaultTfPath);
        }

        private string GetDefaultTfPath()
        {
            var programFiles = Environment.GetEnvironmentVariable("ProgramFiles(x86)");
            if (programFiles == null)
            {
                throw new Exception($"Error in TfsSourceOntorlProvider.GetDefaultTfPath: Unable to get Environment Variable ProgramFiles(x86).");
            }
            return Path.Combine(programFiles, @"Microsoft Visual Studio 14.0\Common7\IDE\TF.exe");
        }

        private string WrapPathInQuotes(string filePath)
        {
            if (filePath == null)
            {
                return null;
            }
            filePath = filePath.Trim();
            if (filePath.EndsWith("\"") && filePath.StartsWith("\""))
            {
                return filePath;
            }

            return $"\"{filePath}\"";
        }

        /// <summary>
        /// Adds the file.
        /// </summary>
        /// <param name="filePath">The file path.</param>
        public void Add(string filePath)
        {
            try
            {
                var info = new ProcessExecutorInfo($"\"{TfPath}\"", $"add {WrapPathInQuotes(filePath)}")
                {
                    WorkingDirectory = Directory.GetParent(filePath).FullName
                };

                ProcessExecutor.ExecuteCmd(info);
            }
            catch (Exception ex)
            {
                throw new Exception("Unable to Add the file " + filePath + Environment.NewLine + ex);
            }
        }

        /// <summary>
        /// Checks the file out.
        /// </summary>
        /// <param name="filePath">The file path.</param>
        /// <exception cref="System.Exception">
        /// Unable to check out file
        /// or
        /// File is read only, please checkout the file before running
        /// </exception>
        public void Checkout(string filePath)
        {

            if (!File.GetAttributes(filePath).HasFlag(FileAttributes.ReadOnly))
            {
                return;
            }

            string output;
            try
            {
                var info = new ProcessExecutorInfo($"\"{TfPath}\"", $"checkout {WrapPathInQuotes(filePath)}")
                {
                    WorkingDirectory = Directory.GetParent(filePath).FullName
                };

                output = ProcessExecutor.ExecuteCmd(info);
            }
            catch (Exception ex)
            {
                throw new Exception("Unable to check out file " + filePath + Environment.NewLine + ex);
            }

            if (File.GetAttributes(filePath).HasFlag(FileAttributes.ReadOnly))
            {
                throw new Exception("File \"" + filePath + "\" is read only even though it should have been checked out, please checkout the file before running.  Output: " + output);
            }
        }

        /// <summary>
        /// Returns true if the file was unchanged and an it was checked out
        /// </summary>
        /// <param name="filePath">The file path.</param>
        /// <param name="contents">The contents.</param>
        /// <returns></returns>
        public bool CheckoutAndUpdateIfDifferent(string filePath, string contents)
        {
            Checkout(filePath);
            File.WriteAllText(filePath, contents);

            return UndoCheckoutIfUnchanged(filePath);
        }

        /// <summary>
        /// Returns true if the file was unchanged and an undo operation was performed
        /// </summary>
        /// <param name="filePath"></param>
        /// <returns></returns>
        public bool UndoCheckoutIfUnchanged(string filePath)
        {

            try
            {
                var info = new ProcessExecutorInfo($"\"{TfPath}\"", $"Diff {WrapPathInQuotes(filePath)}")
                {
                    WorkingDirectory = Directory.GetParent(filePath).FullName
                };

                var output = ProcessExecutor.ExecuteCmd(info);

                if (output.Trim() != "edit: " + filePath.Trim())
                {
                    return false;
                }

                Undo(filePath);
                return true;
            }
            catch (Exception ex)
            {
                throw new Exception("Unable to Undo Checkout If Unchanged for file " + filePath + Environment.NewLine + ex);
            }
        }

        /// <summary>
        /// Un-does the specified file path.
        /// </summary>
        /// <param name="filePath">The file path.</param>
        /// <exception cref="System.Exception">Unable to Undo Checkout If Unchanged for file  + filePath + Environment.NewLine + ex</exception>
        public void Undo(string filePath)
        {
            try
            {
                var info = new ProcessExecutorInfo($"\"{TfPath}\"", $"undo {WrapPathInQuotes(filePath)} /noprompt")
                {
                    WorkingDirectory = Directory.GetParent(filePath).FullName
                };

                ProcessExecutor.ExecuteCmd(info);
            }
            catch (Exception ex)
            {
                throw new Exception("Unable to Undo Checkout for file " + filePath + Environment.NewLine + ex);
            }
        }
    }
}