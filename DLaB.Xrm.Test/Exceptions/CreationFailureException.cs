using System;
using System.Linq;
using System.Text;
using DLaB.Common;

namespace DLaB.Xrm.Test.Exceptions
{
    /// <summary>
    /// Exception Type for when an EnvironmentBuilder.Create action is performed and an exception occurs
    /// /// </summary>
    public class CreationFailureException : Exception
    {
        /// <summary>
        /// Gets a message that describes the current exception.
        /// </summary>
        public override string Message
        {
            get
            {

                var newLineTab = Environment.NewLine + "\t";
                var sb = new StringBuilder(string.IsNullOrWhiteSpace(base.Message) ? string.Empty : base.Message + Environment.NewLine);
                sb.Append("There was an error attempting to create an entity.");
                //if (EntityDependency.Mapper.ConflictingEntityOrder.Any())
                //{
                //    sb.AppendFormat("{0} This could be due to a conflict.  The conflicts are : {1}", Environment.NewLine, newLineTab +
                //               string.Join(newLineTab, EntityDependency.Mapper.ConflictingEntityOrder));
                //}
                sb.AppendLine();
                sb.Append("Additional details in the EntityDependency.Mapper Log: ");
                sb.Append(newLineTab + string.Join(newLineTab, EntityDependency.Mapper.Log));
                sb.AppendLine();
                sb.Append("Final Order: ");
                sb.Append(newLineTab + EntityDependency.Mapper.EntityCreationOrder.ToCsv());


                return sb.ToString();
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CreationFailureException"/> class.
        /// </summary>
        public CreationFailureException()
        {
        }


        /// <summary>
        /// Initializes a new instance of the <see cref="CreationFailureException"/> class.
        /// </summary>
        /// <param name="message">The message that describes the error.</param>
        public CreationFailureException(string message) : base(message)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CreationFailureException"/> class.
        /// </summary>
        /// <param name="innerException">The inner exception.</param>
        public CreationFailureException(Exception innerException) : base(String.Empty, innerException) {}

        /// <summary>
        /// Initializes a new instance of the <see cref="CreationFailureException"/> class.
        /// </summary>
        /// <param name="message">The error message that explains the reason for the exception.</param>
        /// <param name="innerException">The exception that is the cause of the current exception, or a null reference (Nothing in Visual Basic) if no inner exception is specified.</param>
        public CreationFailureException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}
