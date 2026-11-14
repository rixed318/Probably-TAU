using System;
using TAU.President.Tests.Npc;

internal static class Program
{
    private static int Main()
    {
        try
        {
            EventConditionGoldenTests.RunAll();
            Console.WriteLine("PASS: EventConditionGoldenTests.RunAll (16 assertions)");
            return 0;
        }
        catch (Exception exception)
        {
            Console.Error.WriteLine("FAIL: EventConditionGoldenTests.RunAll");
            Console.Error.WriteLine(exception);
            return 1;
        }
    }
}
