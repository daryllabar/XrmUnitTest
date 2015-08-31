using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DLaB.Xrm.Exceptions
{
    /// <summary>
    /// Thrown when an Entity is expected to contain an Attribute, but the attribute isn't found
    /// </summary>
    [Serializable]
    public class MissingAttributeException : Exception
    {
        #region Constructors

        public MissingAttributeException()
        {

        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MissingAttributeException"/> class.
        /// </summary>
        /// <param name="message">The message that describes the error.</param>
        public MissingAttributeException(string message) : base(message)
        {

        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MissingAttributeException"/> class.
        /// </summary>
        /// <param name="messageFormat">The message format.</param>
        /// <param name="messageFormatArgs">The message format arguments.</param>
        public MissingAttributeException(string messageFormat, params object[] messageFormatArgs)
            : base(String.Format(messageFormat, messageFormatArgs))
        {

        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MissingAttributeException"/> class.
        /// </summary>
        /// <param name="message">The error message that explains the reason for the exception.</param>
        /// <param name="innerException">The exception that is the cause of the current exception, or a null reference (Nothing in Visual Basic) if no inner exception is specified.</param>
        public MissingAttributeException(string message, Exception innerException) : base(message, innerException)
        {

        }

        #endregion Constructors
    }
}
