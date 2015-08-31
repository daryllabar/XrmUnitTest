using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xrm.Sdk;

namespace DLaB.Xrm.Exceptions
{
    /// <summary>
    /// The Exception that is thrown when a Pre/Post Image or attribute on the image isn't registered for the plugin
    /// </summary>
    [Serializable]
    public class InvalidPluginStepRegistrationException : Exception
    {
        public enum ImageCollection
        {
            Pre,
            Post
        }

        #region Constructors

        public InvalidPluginStepRegistrationException()
        {

        }

        /// <summary>
        /// Initializes a new instance of the <see cref="InvalidPluginStepRegistrationException"/> class.
        /// </summary>
        /// <param name="message">The message that describes the error.</param>
        public InvalidPluginStepRegistrationException(string message) : base(message)
        {

        }

        /// <summary>
        /// Initializes a new instance of the <see cref="InvalidPluginStepRegistrationException"/> class.
        /// </summary>
        /// <param name="messageFormat">The message format.</param>
        /// <param name="messageFormatArgs">The message format arguments.</param>
        public InvalidPluginStepRegistrationException(string messageFormat, params object[] messageFormatArgs)
            : base(String.Format(messageFormat, messageFormatArgs))
        {

        }

        /// <summary>
        /// Initializes a new instance of the <see cref="InvalidPluginStepRegistrationException"/> class.
        /// </summary>
        /// <param name="message">The error message that explains the reason for the exception.</param>
        /// <param name="innerException">The exception that is the cause of the current exception, or a null reference (Nothing in Visual Basic) if no inner exception is specified.</param>
        public InvalidPluginStepRegistrationException(string message, Exception innerException) : base(message, innerException)
        {

        }

        #endregion Constructors

        public static InvalidPluginStepRegistrationException ImageMissing(string imageKeyName, ImageCollection? image = null)
        {
            return image.HasValue ? new InvalidPluginStepRegistrationException("Invalid Plugin Step Registration.  Missing Required {0} Image, \"{1}\"", image, imageKeyName) : 
                                    new InvalidPluginStepRegistrationException("Invalid Plugin Step Registration.  Missing Required Pre or Post Image, \"{0}\"", imageKeyName);
        }

        public static InvalidPluginStepRegistrationException ImageMissingRequiredAttribute(ImageCollection image, string imageKeyName, string attributeName)
        {
            return new InvalidPluginStepRegistrationException("{0} Entity Image \"{1}\" is missing required parameter {2}!", image, imageKeyName, attributeName);
        }

        public static InvalidPluginStepRegistrationException ImageMissingRequiredAttribute(IPluginExecutionContext context, Entity entity, string attributeName)
        {
            var image = ImageCollection.Post;
            var keyValue = context.PostEntityImages.FirstOrDefault(v => v.Value == entity && !v.Value.Contains(attributeName));
            if (keyValue.Key == null)
            {
                keyValue = context.PreEntityImages.FirstOrDefault(v => v.Value == entity && !v.Value.Contains(attributeName));
                image = ImageCollection.Pre;
            }
            if (keyValue.Key == null)
            {
                throw new Exception("Neither the PostEntityImages or PreEntityImages collections contain the given entity " + entity.GetNameId());
            }

            return new InvalidPluginStepRegistrationException("{0} Entity Image \"{1}\" is missing required parameter {2}!", image, keyValue.Key, attributeName);
        }

        public static InvalidPluginStepRegistrationException ImageMissingRequiredAttributes(ImageCollection image, string imageKeyName, IEnumerable<string> attributes)
        {
            var local = attributes.ToArray();
            return new InvalidPluginStepRegistrationException("{0} Entity Image \"{1}\" is missing required parameter{2} {3}!", image, imageKeyName, local.Count() > 1 ? "s" : String.Empty, String.Join(", ", local));
        }
    }
}
