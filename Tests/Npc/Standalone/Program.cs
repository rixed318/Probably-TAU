using System;
using TAU.President.Tests.Npc;

internal static class Program
{
    private static int Main()
    {
        try
        {
            EventConditionGoldenTests.RunAll();
            EventConditionJsonGoldenTests.RunAll();
            VisitorIdentityGoldenTests.RunAll();
            Console.WriteLine("PASS: EventConditionGoldenTests, EventConditionJsonGoldenTests, VisitorIdentityGoldenTests");
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
