using System;

namespace IdGenerator
{
    public class GuidGenerator : IGuidGenerator
    {
        public Guid Create()
        {
            return Guid.NewGuid();
        }
    }

    public interface IGuidGenerator
    {
        public Guid Create();
    }
}
