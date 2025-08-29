using IdGenerator;

namespace IdGeneratorTests
{
    public class IncrementedGuidGenerator : IGuidGenerator
    {
        private long _counter = 0;

        public Guid Create()
        {
            // Convert the counter to a byte array (8 bytes)
            byte[] counterBytes = BitConverter.GetBytes(_counter);

            // Create a 16-byte array for the Guid
            byte[] guidBytes = new byte[16];

            // Copy the counter bytes into the first 8 bytes
            Array.Copy(counterBytes, 0, guidBytes, 0, 8);

            // The remaining 8 bytes can be zero or random, here we use zero
            // Optionally, you could use random bytes for more uniqueness

            // Increment the counter for next call
            _counter++;

            return new Guid(guidBytes);
        }
    }
}
