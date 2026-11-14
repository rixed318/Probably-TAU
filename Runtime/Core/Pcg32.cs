using System;

namespace TAU.President.Core
{
    /// <summary>
    /// PCG-XSH-RR 64/32, algorithmId pcg32-v1. No Unity or platform RNG dependency.
    /// Mutable instance is owned by a single deterministic command transaction.
    /// </summary>
    public sealed class Pcg32
    {
        public const string AlgorithmId = "pcg32-v1";
        private const ulong Multiplier = 6364136223846793005UL;

        public ulong State { get; private set; }
        public ulong Increment { get; private set; }

        private Pcg32(ulong state, ulong increment)
        {
            if ((increment & 1UL) == 0UL)
                throw new ArgumentException("PCG increment must be odd.", nameof(increment));
            State = state;
            Increment = increment;
        }

        /// <summary>Reference PCG seeding sequence: zero, advance, add seed, advance.</summary>
        public static Pcg32 FromSeed(ulong seed, ulong sequence)
        {
            var rng = new Pcg32(0UL, unchecked((sequence << 1) | 1UL));
            rng.NextUInt32();
            rng.State = unchecked(rng.State + seed);
            rng.NextUInt32();
            return rng;
        }

        /// <summary>Restore exact state without drawing a value; safe for Save→Load.</summary>
        public static Pcg32 Restore(ulong state, ulong increment)
        {
            return new Pcg32(state, increment);
        }

        public Pcg32 Copy()
        {
            return Restore(State, Increment);
        }

        public uint NextUInt32()
        {
            ulong oldState = State;
            State = unchecked(oldState * Multiplier + Increment);
            uint xorshifted = (uint)(((oldState >> 18) ^ oldState) >> 27);
            int rotation = (int)(oldState >> 59);
            return (xorshifted >> rotation) |
                   (xorshifted << ((-rotation) & 31));
        }

        /// <summary>Unbiased bounded sampling via rejection (no modulo bias).</summary>
        public uint NextUInt32(uint maxExclusive)
        {
            if (maxExclusive == 0U)
                throw new ArgumentOutOfRangeException(nameof(maxExclusive));
            uint threshold = unchecked(0U - maxExclusive) % maxExclusive;
            while (true)
            {
                uint value = NextUInt32();
                if (value >= threshold)
                    return value % maxExclusive;
            }
        }

        public int NextInt(int maxExclusive)
        {
            if (maxExclusive <= 0)
                throw new ArgumentOutOfRangeException(nameof(maxExclusive));
            return (int)NextUInt32((uint)maxExclusive);
        }
    }
}
