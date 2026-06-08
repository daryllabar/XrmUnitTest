using System;

namespace IdGenerator;

public class SeededGuidGenerator : IGuidGenerator
{
    private long _counter;

    public SeededGuidGenerator(int seed = 1)
    {
        _counter = seed;
    }

    public Guid Create()
    {
        var counterBytes = BitConverter.GetBytes(_counter);
        var guidBytes = new byte[16];
        Array.Copy(counterBytes, 0, guidBytes, 0, 8);
        _counter++;
        return new Guid(guidBytes);
    }
}
