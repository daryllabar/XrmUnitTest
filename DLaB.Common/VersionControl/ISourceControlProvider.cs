using System;
using System.Collections.Generic;
using System.Text;

namespace DLaB.Common.VersionControl
{
    /// <summary>
    /// Source Control Provider for Internacting with Source Control Versioning
    /// </summary>
    public interface ISourceControlProvider
    {
        /// <summary>
        /// Adds the file out.
        /// </summary>
        /// <param name="filePath">The file path.</param>
        void Add(string filePath);

        /// <summary>
        /// Checks the file out.
        /// </summary>
        /// <param name="filePath">The file path.</param>
        void Checkout(string filePath);

        /// <summary>
        /// Returns true if the file was unchanged and an undo operation was performed
        /// </summary>
        /// <param name="filePath"></param>
        bool UndoCheckoutIfUnchanged(string filePath);    

        /// <summary>
        /// Returns true if the file was unchanged and an it was checked out
        /// </summary>
        /// <param name="filePath">The file path.</param>
        /// <param name="contents">The contents.</param>
        /// <returns></returns>
        bool CheckoutAndUpdateIfDifferent(string filePath, string contents);
    }
}
