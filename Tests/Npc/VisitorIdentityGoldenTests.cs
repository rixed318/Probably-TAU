using System;
using TAU.President.Npc;

namespace TAU.President.Tests.Npc
{
    public static class VisitorIdentityGoldenTests
    {
        public static void RunAll()
        {
            var cards = Day01Visitors.CreateReadOnlyCards();
            var repeatedCards = Day01Visitors.CreateReadOnlyCards();
            // Content 04 DAY01_ART_LINKS.json: planned IDs only, not proof of PNG availability.
            string[] plannedPortraitIds =
            {
                "irina_korneeva_portrait_neutral",
                "dasha_polynskaya_portrait_neutral",
                "mark_sviridov_portrait_neutral"
            };
            string[] plannedStandingIds =
            {
                "irina_korneeva_standing",
                "dasha_polynskaya_standing",
                "mark_sviridov_standing"
            };
            string[] plannedDocumentIds =
            {
                "doc01_procurement_blank",
                "doc02_student_proposal_blank",
                "doc03_partner_letter_blank"
            };
            if (cards.Count != 3 || repeatedCards.Count != 3)
                throw new Exception("Read-only visitor card count mismatch.");
            for (int i = 0; i < cards.Count; i++)
            {
                var visit = Day01Visitors.Create()[i];
                if (cards[i].VisitId != visit.InstanceId ||
                    cards[i].VisitorId != visit.Archetype.DefinitionId ||
                    cards[i].DocumentId != visit.DocumentId ||
                    cards[i].Role != visit.Archetype.Role ||
                    cards[i].DayIndex != 0 ||
                    cards[i].PlannedPortraitAssetId != plannedPortraitIds[i] ||
                    cards[i].PlannedDocumentAssetId != plannedDocumentIds[i] ||
                    cards[i].PlannedStandingAssetId != plannedStandingIds[i] ||
                    cards[i].PlannedStandingAssetId != repeatedCards[i].PlannedStandingAssetId ||
                    cards[i].PlannedDocumentAssetId != repeatedCards[i].PlannedDocumentAssetId ||
                    cards[i].PlannedPortraitAssetId != repeatedCards[i].PlannedPortraitAssetId ||
                    cards[i].VisitId != repeatedCards[i].VisitId ||
                    ReferenceEquals(cards[i], repeatedCards[i]))
                    throw new Exception("Android/Windows visitor card identity mismatch at " + i);
            }
            var mutableCards = cards as System.Collections.Generic.IList<Day01VisitorCard>;
            if (mutableCards == null || !mutableCards.IsReadOnly)
                throw new Exception("Visitor cards must be read-only.");

            // Android and Windows use the same canonical choice-to-card identity.
            string[][] cardChoiceIds =
            {
                new[] { "LAB-1", "LAB-2", "LAB-3" },
                new[] { "STUDENT-1", "STUDENT-2", "STUDENT-3" },
                new[] { "PARTNER-1", "PARTNER-2", "PARTNER-3" }
            };
            for (int i = 0; i < cardChoiceIds.Length; i++)
                foreach (string choiceId in cardChoiceIds[i])
                {
                    Day01VisitorCard byChoiceCard;
                    if (!Day01Visitors.TryGetCardByChoiceId(choiceId, out byChoiceCard) ||
                        byChoiceCard.VisitId != cards[i].VisitId ||
                        byChoiceCard.VisitorId != cards[i].VisitorId ||
                        byChoiceCard.DocumentId != cards[i].DocumentId)
                        throw new Exception("Choice-to-card mismatch: " + choiceId);
                }
            Day01VisitorCard missingCard;
            if (Day01Visitors.TryGetCardByChoiceId("ROOM-1", out missingCard) || missingCard != null ||
                Day01Visitors.TryGetCardByChoiceId("lab-1", out missingCard) || missingCard != null ||
                Day01Visitors.TryGetCardByChoiceId(null, out missingCard) || missingCard != null)
                throw new Exception("Invalid choice must not resolve to a visitor card.");

            // A single read-only preview includes the canonical card and inspection hint.
            foreach (string[] group in cardChoiceIds)
                foreach (string id in group)
                {
                    Day01Visitors.ChoicePreview preview;
                    if (!Day01Visitors.TryGetChoicePreview(id, out preview) ||
                        preview.ChoiceId != id ||
                        preview.VisitorCard == null ||
                        preview.VisitorCard.VisitId != cards[Array.IndexOf(cardChoiceIds, group)].VisitId ||
                        preview.RequiredInspectionDocumentId !=
                            (id == "LAB-2" ? "DOC-01" : id == "PARTNER-2" ? "DOC-03" : null) ||
                        preview.RequiresInspection != (id == "LAB-2" || id == "PARTNER-2"))
                        throw new Exception("Choice preview identity/inspection mismatch: " + id);
                }
            Day01Visitors.ChoicePreview invalidPreview;
            if (Day01Visitors.TryGetChoicePreview("ROOM-1", out invalidPreview) || invalidPreview != null ||
                Day01Visitors.TryGetChoicePreview("lab-2", out invalidPreview) || invalidPreview != null ||
                Day01Visitors.TryGetChoicePreview(null, out invalidPreview) || invalidPreview != null)
                throw new Exception("Unknown choice preview must fail closed.");

            // Save/restore navigation uses canonical visit IDs on both platforms.
            string[] restoreVisits = { "D01_LAB", "D01_STUDENT", "D01_PARTNER" };
            for (int i = 0; i < restoreVisits.Length; i++)
            {
                Day01VisitorCard restored;
                if (!Day01Visitors.TryGetCardByVisitId(restoreVisits[i], out restored) ||
                    restored.VisitId != cards[i].VisitId ||
                    restored.DocumentId != cards[i].DocumentId ||
                    restored.PlannedPortraitAssetId != cards[i].PlannedPortraitAssetId ||
                    ReferenceEquals(restored, cards[i]))
                    throw new Exception("Visit restore card mismatch: " + restoreVisits[i]);
            }
            Day01VisitorCard invalidRestore;
            if (Day01Visitors.TryGetCardByVisitId("d01_lab", out invalidRestore) || invalidRestore != null ||
                Day01Visitors.TryGetCardByVisitId("D01_UNKNOWN", out invalidRestore) || invalidRestore != null ||
                Day01Visitors.TryGetCardByVisitId(null, out invalidRestore) || invalidRestore != null)
                throw new Exception("Unknown visit restore must fail closed.");

            // Document-tap route must resolve the same immutable card on Windows/Android.
            string[] tapDocuments = { "DOC-01", "DOC-02", "DOC-03" };
            for (int i = 0; i < tapDocuments.Length; i++)
            {
                Day01VisitorCard tapped;
                if (!Day01Visitors.TryGetCardByDocumentId(tapDocuments[i], out tapped) ||
                    tapped.VisitId != cards[i].VisitId ||
                    tapped.VisitorId != cards[i].VisitorId ||
                    tapped.PlannedDocumentAssetId != cards[i].PlannedDocumentAssetId)
                    throw new Exception("Document tap card mismatch: " + tapDocuments[i]);
            }
            Day01VisitorCard invalidTap;
            if (Day01Visitors.TryGetCardByDocumentId("doc-01", out invalidTap) || invalidTap != null ||
                Day01Visitors.TryGetCardByDocumentId("DOC-04", out invalidTap) || invalidTap != null ||
                Day01Visitors.TryGetCardByDocumentId(null, out invalidTap) || invalidTap != null)
                throw new Exception("Unknown document tap must fail closed.");

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
            // Content PR #11: only LAB-2 and PARTNER-2 require an inspection.
            string[] inspectedChoices = { "LAB-2", "PARTNER-2" };
            string[] inspectedDocuments = { "DOC-01", "DOC-03" };
            for (int i = 0; i < inspectedChoices.Length; i++)
            {
                Day01Visitors.ChoiceDescriptor descriptor;
                if (!Day01Visitors.TryDescribeChoice(inspectedChoices[i], out descriptor) ||
                    descriptor.ChoiceId != inspectedChoices[i] ||
                    descriptor.RequiredInspectionDocumentId != inspectedDocuments[i] ||
                    descriptor.Visitor.DocumentId != inspectedDocuments[i])
                    throw new Exception("Inspection requirement mismatch: " + inspectedChoices[i]);
            }
            foreach (string[] group in choiceIds)
                foreach (string id in group)
                {
                    Day01Visitors.ChoiceDescriptor descriptor;
                    if (!Day01Visitors.TryDescribeChoice(id, out descriptor) ||
                        (id != "LAB-2" && id != "PARTNER-2" && descriptor.RequiredInspectionDocumentId != null))
                        throw new Exception("Unexpected inspection prerequisite: " + id);
                }
            Day01Visitors.ChoiceDescriptor invalidDescriptor;
            if (Day01Visitors.TryDescribeChoice("ROOM-1", out invalidDescriptor) || invalidDescriptor != null ||
                Day01Visitors.TryDescribeChoice("lab-2", out invalidDescriptor) || invalidDescriptor != null ||
                Day01Visitors.TryDescribeChoice(null, out invalidDescriptor) || invalidDescriptor != null)
                throw new Exception("Invalid choice descriptor must fail closed.");
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
