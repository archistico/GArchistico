using GenerativeArt.Core.Randomness;

namespace GenerativeArt.Algorithms.Noise;

/// <summary>
/// Permutazione Fisher-Yates deterministica a 256 elementi, condivisa dal noise seeded.
/// La sequenza di chiamate a SplitMix64-v1 è parte del contratto di compatibilità.
/// </summary>
internal static class NoisePermutation256
{
    public static byte[] FromSeed(ulong seed)
    {
        byte[] result = new byte[256];
        for (int i = 0; i < result.Length; i++) result[i] = (byte)i;
        var random = new SplitMix64Random(seed);
        for (int i = result.Length - 1; i > 0; i--)
        {
            int j = random.NextInt32Inclusive(0, i);
            (result[i], result[j]) = (result[j], result[i]);
        }
        return result;
    }
}
