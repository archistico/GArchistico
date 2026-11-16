namespace GenerativeArt.Core.Randomness;

/// <summary>Generatore deterministico locale. Non condividere la stessa istanza tra thread.</summary>
public interface IRandomSource
{
    ulong NextUInt64();
    double NextDouble();
    float NextFloat();
    int NextInt32Inclusive(int minInclusive, int maxInclusive);
    IRandomSource Fork(string streamId);
}
