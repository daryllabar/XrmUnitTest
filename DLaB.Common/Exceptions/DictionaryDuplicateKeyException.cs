using System;


namespace DLaB.Common.Exceptions
{
    /// <summary>
    /// The exception that is thrown when an attempt is made to add a duplicate key to a dictionary.
    /// It's primary purpose is to provide a better error message than ArgumentException, An item with the smae key has already been added.
    /// </summary>
    [Serializable()]
    public class DictionaryDuplicateKeyException : Exception
    {
        public DictionaryDuplicateKeyException() { }
        public DictionaryDuplicateKeyException(string message) : base(message) { }
        public DictionaryDuplicateKeyException(string message, Exception inner) : base(message, inner) { }

        // A constructor is needed for serialization when an
        // exception propagates from a remoting server to the client. 
        protected DictionaryDuplicateKeyException(System.Runtime.Serialization.SerializationInfo info,
            System.Runtime.Serialization.StreamingContext context) { }


    }
}
