using System;
using TAU.President.Npc;

namespace TAU.President.Tests.Npc
{
    public static class VisitorIdentityGoldenTests
    {
        public static void RunAll()
        {
            var first = Day01Visitors.Create();
            var replay = Day01Visitors.Create();
            string[] expectedVisits = { "D01_LAB", "D01_STUDENT", "D01_PARTNER" };
            string[] expectedDefinitions = { "npc_irina_korneeva", "npc_dasha_polynskaya", "npc_mark_sviridov" };
            string[] expectedDocuments = { "DOC-01", "DOC-02", "DOC-03" };
            if (first.Count != 3 || replay.Count != 3) throw new Exception("Expected three visits.");
            for (int i = 0; i < 3; i++)
            {
                if (first[i].InstanceId != expectedVisits[i] ||
                    first[i].Archetype.DefinitionId != expectedDefinitions[i] ||
                    first[i].DocumentId != expectedDocuments[i] || first[i].DayIndex != 0)
                    throw new Exception("Content mapping mismatch at " + i);
                if (first[i].InstanceId != replay[i].InstanceId)
                    throw new Exception("Replay changed stable visit identity.");
            }
            if (ReferenceEquals(first[0], replay[0]))
                throw new Exception("Factory should return fresh immutable objects.");
            try { new VisitorInstance("x", null, "DOC-01", 0); throw new Exception("Null archetype accepted."); }
            catch (ArgumentNullException) { }
            try { new VisitorInstance("x", first[0].Archetype, "DOC-01", -1); throw new Exception("Negative day accepted."); }
            catch (ArgumentOutOfRangeException) { }
        }
    }
}
