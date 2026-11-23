using System;
using TAU.President.Core;

namespace TAU.President.Tests
{
    /// <summary>Standalone golden tests; invoke RunAll() from a future NUnit/EditMode wrapper.</summary>
    public static class Pcg32GoldenTests
    {
        public static void RunAll()
        {
            // Published PCG reference example: initstate=42, initseq=54.
            uint[] expected =
            {
                0xa15c02b7U, 0x7b47f409U, 0xba1d3330U,
                0x83d2f293U, 0xbfa4784bU, 0xcbed606eU
            };
            var rng = Pcg32.FromSeed(42UL, 54UL);
            for (int i = 0; i < expected.Length; i++)
                Require(rng.NextUInt32() == expected[i], "PCG reference vector " + i);

            var original = Pcg32.FromSeed(2026UL, 17UL);
            original.NextUInt32();
            var restored = Pcg32.Restore(original.State, original.Increment);
            for (int i = 0; i < 128; i++)
                Require(original.NextUInt32() == restored.NextUInt32(), "Save→Load replay " + i);

            var copy = original.Copy();
            Require(copy.State == original.State && copy.Increment == original.Increment,
                "Copy preserves RNG state");
            copy.NextUInt32();
            Require(copy.State != original.State, "Copy does not mutate original");

            var bounded = Pcg32.FromSeed(42UL, 54UL);
            for (int i = 0; i < 4096; i++)
            {
                uint n = bounded.NextUInt32(7U);
                Require(n < 7U, "Bounded range");
            }
            Require(Pcg32.FromSeed(1UL, 1UL).NextInt(1) == 0, "Single-element range");
            // Invalid bounds and invalid restored increment must not consume RNG state.
            var rejected = Pcg32.FromSeed(2026UL, 17UL);
            ulong stateBeforeReject = rejected.State;
            ulong incrementBeforeReject = rejected.Increment;
            ExpectArgument(() => rejected.NextUInt32(0U));
            ExpectArgument(() => rejected.NextInt(0));
            ExpectArgument(() => rejected.NextInt(-1));
            Require(rejected.State == stateBeforeReject &&
                    rejected.Increment == incrementBeforeReject,
                "Rejected RNG bounds do not consume draws");
            var referenceAfterReject = Pcg32.Restore(stateBeforeReject, incrementBeforeReject);
            Require(rejected.NextUInt32() == referenceAfterReject.NextUInt32(),
                "Rejected RNG bounds preserve next draw");
            ExpectArgument(() => Pcg32.Restore(stateBeforeReject, 0UL));
            Require(rejected.State == referenceAfterReject.State,
                "Invalid restore does not mutate existing RNG");

            ExpectArgument(() => Pcg32.Restore(1UL, 2UL));
            ExpectArgument(() => Pcg32.FromSeed(1UL, 1UL).NextUInt32(0U));
            ExpectArgument(() => Pcg32.FromSeed(1UL, 1UL).NextInt(0));
        }

        private static void Require(bool ok, string message)
        {
            if (!ok) throw new InvalidOperationException(message);
        }

        private static void ExpectArgument(Action action)
        {
            try { action(); }
            catch (ArgumentException) { return; }
            throw new InvalidOperationException("Expected ArgumentException.");
        }
    }
}
