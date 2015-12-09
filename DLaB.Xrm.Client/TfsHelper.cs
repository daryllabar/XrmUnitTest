using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using Microsoft.TeamFoundation.Client;
using Microsoft.TeamFoundation.VersionControl.Client;

namespace DLaB.Xrm.Client
{
    /// <summary>
    /// Class to handle Checkingout from TFS
    /// </summary>
    public class TfsHelper
    {
        /// <summary>
        /// Checkouts the and update file if different.
        /// </summary>
        /// <param name="filePath">The file path.</param>
        /// <param name="contents">The contents.</param>
        /// <exception cref="System.ArgumentException">File Path cannot be contain a directory that is null or empty!;filePath</exception>
        public static void CheckoutAndUpdateFileIfDifferent(string filePath, string contents)
        {
            var dir = Path.GetDirectoryName(filePath);
            if (String.IsNullOrWhiteSpace(dir))
            {
                throw new ArgumentException("File Path cannot be contain a directory that is null or empty!", "filePath");
            }

            if (!Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
            }

            if (!File.Exists(filePath))
            {
                File.WriteAllText(filePath, contents);
                return;
            }

            if (!FileDiffers(filePath, contents)) { return; }

            CheckoutFile(filePath);
            File.WriteAllText(filePath, contents);
        }

        private static void CheckoutFile(string filePath)
        {
            if (!File.GetAttributes(filePath).HasFlag(FileAttributes.ReadOnly))
            {
                return;
            }

            try
            {
                var workspaceInfo = Workstation.Current.GetLocalWorkspaceInfo(filePath);
                var server = new TfsTeamProjectCollection(workspaceInfo.ServerUri);
                var workspace = workspaceInfo.GetWorkspace(server);

                workspace.PendEdit(filePath);
            }
            catch (Exception ex)
            {
                throw new Exception("Unable to check out file " + filePath + Environment.NewLine + ex);
            }

            if (File.GetAttributes(filePath).HasFlag(FileAttributes.ReadOnly))
            {
                throw new Exception("File \"" + filePath + "\" is read only, please checkout the file before running");
            }
        }

        private static bool FileDiffers(string filePath, string contents)
        {
            var workspaceInfo = Workstation.Current.GetLocalWorkspaceInfo(filePath);
            var server = new TfsTeamProjectCollection(workspaceInfo.ServerUri);
            var workspace = workspaceInfo.GetWorkspace(server);
            var items = workspace.VersionControlServer.GetItems(filePath, new WorkspaceVersionSpec(workspace), RecursionType.None).Items;
            if (items.Length == 0) { return true; }
            using (var md5 = MD5.Create())
            {
                return !items[0].HashValue.SequenceEqual(md5.ComputeHash(Encoding.UTF8.GetBytes(contents)));
            }
        }

        /// <summary>
        /// Returns true if the file was unchanged and an undo operation was performed
        /// </summary>
        /// <param name="filePath"></param>
        /// <returns></returns>
        protected bool UndoCheckoutIfUnchanged(string filePath)
        {
            var workspaceInfo = Workstation.Current.GetLocalWorkspaceInfo(filePath);
            var server = new TfsTeamProjectCollection(workspaceInfo.ServerUri);
            var workspace = workspaceInfo.GetWorkspace(server);
            var item = workspace.VersionControlServer.GetItems(filePath, new WorkspaceVersionSpec(workspace), RecursionType.None).Items[0];
            bool unchanged;

            using (var fs = File.OpenRead(filePath))
            {
                unchanged = item.HashValue.SequenceEqual(MD5.Create().ComputeHash(fs));
            }

            if (unchanged)
            {
                workspace.Undo(filePath);
            }

            return unchanged;
        }
    }
}
