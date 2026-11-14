using System;
using TAU.President.Tests;
using TAU.President.Tests.Core;

internal static class Program
{
    private static int Main()
    {
        int failures = 0;
        failures += Run("PCG32 golden vectors and replay", Pcg32GoldenTests.RunAll);
        failures += Run("GameClock five-day calendar", GameClockGoldenTests.RunAll);
        failures += Run("GameState snapshot invariants", GameStateSnapshotGoldenTests.RunAll);
        Console.WriteLine(failures == 0
            ? "PASS: all 3 Core golden suites."
            : "FAIL: " + failures + " Core golden suite(s).");
        return failures == 0 ? 0 : 1;
    }

    private static int Run(string label, Action suite)
    {
        try
        {
            suite();
            Console.WriteLine("PASS: " + label);
            return 0;
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine("FAIL: " + label + ": " + ex);
            return 1;
        }
    }
}
