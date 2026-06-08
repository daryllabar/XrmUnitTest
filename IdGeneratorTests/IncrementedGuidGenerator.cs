using IdGenerator;

namespace IdGeneratorTests
{
    public class IncrementedGuidGenerator : SeededGuidGenerator
    {
        public IncrementedGuidGenerator() : base(1)
        {
        }
    }
}
