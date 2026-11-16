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
            for (int i = 0; i < expectedVisits.Length; i++)
            {
                VisitorInstance resolved;
                if (!Day01Visitors.TryGetByVisitId(expectedVisits[i], out resolved) ||
                    resolved.InstanceId != expectedVisits[i] ||
                    resolved.DocumentId != expectedDocuments[i] ||
                    resolved.Archetype.DefinitionId != expectedDefinitions[i])
                    throw new Exception("Canonical visit lookup mismatch at " + i);
            }
            VisitorInstance missing;
            if (Day01Visitors.TryGetByVisitId("d01_lab", out missing) || missing != null ||
                Day01Visitors.TryGetByVisitId("D01_UNKNOWN", out missing) || missing != null ||
                Day01Visitors.TryGetByVisitId(null, out missing) || missing != null)
                throw new Exception("Unknown/case-mismatched visit must not resolve.");
            for (int i = 0; i < expectedDocuments.Length; i++)
            {
                VisitorInstance byDocument;
                if (!Day01Visitors.TryGetByDocumentId(expectedDocuments[i], out byDocument) ||
                    byDocument.InstanceId != expectedVisits[i] ||
                    byDocument.Archetype.DefinitionId != expectedDefinitions[i])
                    throw new Exception("Canonical document lookup mismatch at " + i);
            }
            VisitorInstance absentDocument;
            if (Day01Visitors.TryGetByDocumentId("doc-01", out absentDocument) || absentDocument != null ||
                Day01Visitors.TryGetByDocumentId("DOC-04", out absentDocument) || absentDocument != null ||
                Day01Visitors.TryGetByDocumentId(null, out absentDocument) || absentDocument != null)
                throw new Exception("Unknown/case-mismatched document must not resolve.");
            string[][] choiceIds =
            {
                new[] { "LAB-1", "LAB-2", "LAB-3" },
                new[] { "STUDENT-1", "STUDENT-2", "STUDENT-3" },
                new[] { "PARTNER-1", "PARTNER-2", "PARTNER-3" }
            };
            for (int i = 0; i < choiceIds.Length; i++)
            {
                foreach (string choiceId in choiceIds[i])
                {
                    VisitorInstance byChoice;
                    if (!Day01Visitors.TryGetByChoiceId(choiceId, out byChoice) ||
                        byChoice.InstanceId != expectedVisits[i] ||
                        byChoice.DocumentId != expectedDocuments[i])
                        throw new Exception("Canonical choice-to-visitor mapping mismatch: " + choiceId);
                }
            }
            VisitorInstance absentChoice;
            if (Day01Visitors.TryGetByChoiceId("LAB-4", out absentChoice) || absentChoice != null ||
                Day01Visitors.TryGetByChoiceId("lab-1", out absentChoice) || absentChoice != null ||
                Day01Visitors.TryGetByChoiceId("ROOM-1", out absentChoice) || absentChoice != null ||
                Day01Visitors.TryGetByChoiceId(null, out absentChoice) || absentChoice != null)
                throw new Exception("Unknown, room, or case-mismatched choice must not resolve.");
            if (ReferenceEquals(first[0], replay[0]))
                throw new Exception("Factory should return fresh immutable objects.");
            try { new VisitorInstance("x", null, "DOC-01", 0); throw new Exception("Null archetype accepted."); }
            catch (ArgumentNullException) { }
            try { new VisitorInstance("x", first[0].Archetype, "DOC-01", -1); throw new Exception("Negative day accepted."); }
            catch (ArgumentOutOfRangeException) { }
        }
    }
}
