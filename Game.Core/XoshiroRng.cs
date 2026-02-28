namespace Game.Core;

/// <summary>
/// xoshiro128** pseudo-random number generator.
/// Fast, small-state (128-bit) PRNG with good statistical quality.
/// </summary>
public sealed class XoshiroRng
{
    private uint _s0, _s1, _s2, _s3;

    /// <summary>
    /// Create a new RNG seeded from a 64-bit value via SplitMix64.
    /// </summary>
    public XoshiroRng(ulong seed)
    {
        // Use SplitMix64 to derive four 32-bit state values
        _s0 = (uint)SplitMix64(ref seed);
        _s1 = (uint)SplitMix64(ref seed);
        _s2 = (uint)SplitMix64(ref seed);
        _s3 = (uint)SplitMix64(ref seed);

        // Ensure state is not all-zero
        if (_s0 == 0 && _s1 == 0 && _s2 == 0 && _s3 == 0)
        {
            _s0 = 1;
        }
    }

    private XoshiroRng(uint s0, uint s1, uint s2, uint s3)
    {
        _s0 = s0;
        _s1 = s1;
        _s2 = s2;
        _s3 = s3;
    }

    /// <summary>
    /// Generate the next 32-bit unsigned integer (xoshiro128** algorithm).
    /// </summary>
    public uint NextUint()
    {
        uint result = RotateLeft(_s1 * 5, 7) * 9;

        uint t = _s1 << 9;

        _s2 ^= _s0;
        _s3 ^= _s1;
        _s1 ^= _s2;
        _s0 ^= _s3;

        _s2 ^= t;
        _s3 = RotateLeft(_s3, 11);

        return result;
    }

    /// <summary>
    /// Returns a non-negative integer less than <paramref name="max"/>.
    /// </summary>
    public int Next(int max)
    {
        if (max <= 0)
            throw new ArgumentOutOfRangeException(nameof(max), "max must be positive.");

        // Unbiased integer in [0, max) via rejection sampling
        uint threshold = (uint)((0x1_0000_0000UL - (uint)max) % (uint)max);
        while (true)
        {
            uint r = NextUint();
            if (r >= threshold)
                return (int)(r % (uint)max);
        }
    }

    /// <summary>
    /// Returns an integer in [<paramref name="min"/>, <paramref name="max"/>).
    /// </summary>
    public int Next(int min, int max)
    {
        if (min >= max)
            throw new ArgumentOutOfRangeException(nameof(min), "min must be less than max.");

        return min + Next(max - min);
    }

    /// <summary>
    /// Snapshot the internal state as a uint[4] array (for serialization).
    /// </summary>
    public uint[] GetState() => [_s0, _s1, _s2, _s3];

    /// <summary>
    /// Restore an RNG instance from a previously saved state.
    /// </summary>
    public static XoshiroRng FromState(uint[] state)
    {
        if (state == null || state.Length != 4)
            throw new ArgumentException("State must be a uint[4] array.", nameof(state));

        return new XoshiroRng(state[0], state[1], state[2], state[3]);
    }

    // ── Helpers ──

    private static uint RotateLeft(uint value, int count)
        => (value << count) | (value >> (32 - count));

    private static ulong SplitMix64(ref ulong state)
    {
        ulong z = state += 0x9E3779B97F4A7C15UL;
        z = (z ^ (z >> 30)) * 0xBF58476D1CE4E5B9UL;
        z = (z ^ (z >> 27)) * 0x94D049BB133111EBUL;
        return z ^ (z >> 31);
    }
}
