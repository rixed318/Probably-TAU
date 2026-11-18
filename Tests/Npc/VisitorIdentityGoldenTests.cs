using System;
using System.Collections.Generic;
using TAU.President.Npc;

namespace TAU.President.Tests.Npc
{
    public static class VisitorIdentityGoldenTests
    {
        public static void RunAll()
        {
            // Choice-level inspection lookup must be document-consistent even for choices
            // that do not require inspection to become eligible.
            string[][] inspectionChoiceGroups =
            {
                new[] { "LAB-1", "LAB-2", "LAB-3" },
                new[] { "STUDENT-1", "STUDENT-2", "STUDENT-3" },
                new[] { "PARTNER-1", "PARTNER-2", "PARTNER-3" }
            };
            string[] expectedInspectionIds =
                { "check_seat_map", "check_room_calendar", "check_contract_appendix" };
            string[] expectedInspectionDocuments = { "DOC-01", "DOC-02", "DOC-03" };
            for (int group = 0; group < inspectionChoiceGroups.Length; group++)
                foreach (string choiceId in inspectionChoiceGroups[group])
                {
                    Day01Visitors.InspectionPreview byChoice;
                    if (!Day01Visitors.TryGetInspectionPreviewByChoiceId(choiceId, out byChoice) ||
                        byChoice == null || byChoice.DocumentId != expectedInspectionDocuments[group] ||
                        byChoice.InspectionId != expectedInspectionIds[group] ||
                        byChoice.CostTokens != 1 || byChoice.RepeatCostTokens != 0)
                        throw new Exception("Choice inspection mapping mismatch: " + choiceId);
                }
            foreach (string invalidChoice in new[] { null, "", "lab-1", "LAB-4", "ROOM-1", " LAB-1" })
            {
                Day01Visitors.InspectionPreview invalidPreview;
                if (Day01Visitors.TryGetInspectionPreviewByChoiceId(invalidChoice, out invalidPreview) ||
                    invalidPreview != null)
                    throw new Exception("Invalid choice must not resolve an inspection preview.");
            }

            // Canonical Content Day01 inspection IDs and token costs; presentation only.
            string[] inspectionDocumentIds = { "DOC-01", "DOC-02", "DOC-03" };
            string[] inspectionActionIds = { "check_seat_map", "check_room_calendar", "check_contract_appendix" };
            for (int i = 0; i < inspectionDocumentIds.Length; i++)
            {
                Day01Visitors.InspectionPreview inspection;
                if (!Day01Visitors.TryGetInspectionPreviewByDocumentId(inspectionDocumentIds[i], out inspection) ||
                    inspection == null || inspection.DocumentId != inspectionDocumentIds[i] ||
                    inspection.InspectionId != inspectionActionIds[i] ||
                    inspection.CostTokens != 1 || inspection.RepeatCostTokens != 0)
                    throw new Exception("Day01 inspection fixture mismatch: " + inspectionDocumentIds[i]);
            }
            foreach (string invalid in new[] { null, "", "doc-01", "DOC-04", " DOC-01" })
            {
                Day01Visitors.InspectionPreview missingInspection;
                if (Day01Visitors.TryGetInspectionPreviewByDocumentId(invalid, out missingInspection) ||
                    missingInspection != null)
                    throw new Exception("Invalid Day01 inspection ID accepted.");
            }
            // Visit-scoped inspection previews must agree with canonical document routing.
            string[] inspectionVisitIds = { "D01_LAB", "D01_STUDENT", "D01_PARTNER" };
            for (int i = 0; i < inspectionVisitIds.Length; i++)
            {
                Day01Visitors.InspectionPreview byVisit;
                if (!Day01Visitors.TryGetInspectionPreviewByVisitId(inspectionVisitIds[i], out byVisit) ||
                    byVisit == null || byVisit.DocumentId != inspectionDocumentIds[i] ||
                    byVisit.InspectionId != inspectionActionIds[i] ||
                    byVisit.CostTokens != 1 || byVisit.RepeatCostTokens != 0)
                    throw new Exception("Visit inspection preview mismatch: " + inspectionVisitIds[i]);
            }
            foreach (string invalid in new[] { null, "", "d01_lab", "D01_UNKNOWN", " D01_LAB" })
            {
                Day01Visitors.InspectionPreview missingByVisit;
                if (Day01Visitors.TryGetInspectionPreviewByVisitId(invalid, out missingByVisit) ||
                    missingByVisit != null)
                    throw new Exception("Invalid Day01 visit inspection ID accepted.");
            }
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
            // Art manifest navigation uses one exact, case-sensitive ID resolver on both platforms.
            for (int i = 0; i < cards.Count; i++)
            {
                foreach (string assetId in new[] { plannedPortraitIds[i], plannedStandingIds[i], plannedDocumentIds[i] })
                {
                    Day01VisitorCard fromAsset;
                    if (!Day01Visitors.TryGetCardByPlannedAssetId(assetId, out fromAsset) ||
                        fromAsset.VisitId != cards[i].VisitId ||
                        fromAsset.DocumentId != cards[i].DocumentId ||
                        ReferenceEquals(fromAsset, cards[i]))
                        throw new Exception("Planned asset-to-card mismatch: " + assetId);
                }
            }
            Day01VisitorCard invalidAsset;
            if (Day01Visitors.TryGetCardByPlannedAssetId(null, out invalidAsset) || invalidAsset != null ||
                Day01Visitors.TryGetCardByPlannedAssetId("", out invalidAsset) || invalidAsset != null ||
                Day01Visitors.TryGetCardByPlannedAssetId("Irina_korneeva_standing", out invalidAsset) || invalidAsset != null ||
                Day01Visitors.TryGetCardByPlannedAssetId("missing_asset", out invalidAsset) || invalidAsset != null)
                throw new Exception("Unknown planned art ID must fail closed.");

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

            // Portrait/visitor UI navigation resolves canonical Content definition IDs only.
            string[] visitorDefinitions =
            {
                "npc_irina_korneeva", "npc_dasha_polynskaya", "npc_mark_sviridov"
            };
            for (int i = 0; i < visitorDefinitions.Length; i++)
            {
                Day01VisitorCard byVisitor;
                if (!Day01Visitors.TryGetCardByVisitorId(visitorDefinitions[i], out byVisitor) ||
                    byVisitor.VisitorId != visitorDefinitions[i] ||
                    byVisitor.VisitId != cards[i].VisitId ||
                    byVisitor.DocumentId != cards[i].DocumentId ||
                    ReferenceEquals(byVisitor, cards[i]))
                    throw new Exception("Visitor definition card mismatch: " + visitorDefinitions[i]);
            }
            Day01VisitorCard unknownVisitorCard;
            if (Day01Visitors.TryGetCardByVisitorId("NPC_IRINA_KORNEEVA", out unknownVisitorCard) || unknownVisitorCard != null ||
                Day01Visitors.TryGetCardByVisitorId("npc_unknown", out unknownVisitorCard) || unknownVisitorCard != null ||
                Day01Visitors.TryGetCardByVisitorId("", out unknownVisitorCard) || unknownVisitorCard != null ||
                Day01Visitors.TryGetCardByVisitorId(null, out unknownVisitorCard) || unknownVisitorCard != null)
                throw new Exception("Invalid visitor definition must fail closed.");

            // Day01 visitor menu uses canonical ordered choice IDs without mutable state.
            string[][] menuChoices =
            {
                new[] { "LAB-1", "LAB-2", "LAB-3" },
                new[] { "STUDENT-1", "STUDENT-2", "STUDENT-3" },
                new[] { "PARTNER-1", "PARTNER-2", "PARTNER-3" }
            };
            string[] menuVisits = { "D01_LAB", "D01_STUDENT", "D01_PARTNER" };
            for (int i = 0; i < menuVisits.Length; i++)
            {
                IReadOnlyList<string> menu;
                if (!Day01Visitors.TryGetChoiceIdsByVisitId(menuVisits[i], out menu) ||
                    menu == null || menu.Count != 3)
                    throw new Exception("Missing Day01 choice menu: " + menuVisits[i]);
                for (int j = 0; j < 3; j++)
                    if (menu[j] != menuChoices[i][j])
                        throw new Exception("Day01 choice menu order mismatch: " + menuVisits[i]);
                if (!(menu is System.Collections.ObjectModel.ReadOnlyCollection<string>))
                    throw new Exception("Choice menu must be immutable.");
            }
            IReadOnlyList<string> invalidMenu;
            if (Day01Visitors.TryGetChoiceIdsByVisitId("d01_lab", out invalidMenu) || invalidMenu != null ||
                Day01Visitors.TryGetChoiceIdsByVisitId("D01_UNKNOWN", out invalidMenu) || invalidMenu != null ||
                Day01Visitors.TryGetChoiceIdsByVisitId(null, out invalidMenu) || invalidMenu != null)
                throw new Exception("Invalid visit choice menu must fail closed.");

            // The shared menu adapter preserves Content order and inspection hints without UI duplication.
            for (int i = 0; i < menuVisits.Length; i++)
            {
                IReadOnlyList<Day01Visitors.ChoicePreview> previews;
                if (!Day01Visitors.TryGetChoicePreviewsByVisitId(menuVisits[i], out previews) ||
                    previews == null || previews.Count != 3 ||
                    !(previews is System.Collections.ObjectModel.ReadOnlyCollection<Day01Visitors.ChoicePreview>))
                    throw new Exception("Invalid immutable preview menu: " + menuVisits[i]);
                for (int j = 0; j < previews.Count; j++)
                {
                    string id = menuChoices[i][j];
                    if (previews[j].ChoiceId != id || previews[j].VisitorCard.VisitId != menuVisits[i] ||
                        previews[j].RequiresInspection != (id == "LAB-2" || id == "PARTNER-2"))
                        throw new Exception("Preview menu order or inspection mismatch: " + id);
                }
            }
            IReadOnlyList<Day01Visitors.ChoicePreview> invalidPreviews;
            if (Day01Visitors.TryGetChoicePreviewsByVisitId(null, out invalidPreviews) || invalidPreviews != null ||
                Day01Visitors.TryGetChoicePreviewsByVisitId("d01_lab", out invalidPreviews) || invalidPreviews != null ||
                Day01Visitors.TryGetChoicePreviewsByVisitId("D01_UNKNOWN", out invalidPreviews) || invalidPreviews != null)
                throw new Exception("Unknown preview menu must fail closed.");

            // Cross-platform selection must not show a choice from a different visitor.
            for (int i = 0; i < menuVisits.Length; i++)
            {
                foreach (string id in menuChoices[i])
                {
                    Day01Visitors.ChoicePreview matched;
                    if (!Day01Visitors.TryGetChoicePreviewForVisit(menuVisits[i], id, out matched) ||
                        matched == null || matched.ChoiceId != id ||
                        matched.VisitorCard.VisitId != menuVisits[i])
                        throw new Exception("Visit-scoped choice preview mismatch: " + id);
                    Day01Visitors.ChoicePreview wrongVisit;
                    string otherVisit = menuVisits[(i + 1) % menuVisits.Length];
                    if (Day01Visitors.TryGetChoicePreviewForVisit(otherVisit, id, out wrongVisit) ||
                        wrongVisit != null)
                        throw new Exception("Cross-visitor choice preview leaked: " + id);
                }
            }
            Day01Visitors.ChoicePreview invalidScoped;
            if (Day01Visitors.TryGetChoicePreviewForVisit(null, "LAB-1", out invalidScoped) || invalidScoped != null ||
                Day01Visitors.TryGetChoicePreviewForVisit("d01_lab", "LAB-1", out invalidScoped) || invalidScoped != null ||
                Day01Visitors.TryGetChoicePreviewForVisit("D01_LAB", "PARTNER-2", out invalidScoped) || invalidScoped != null ||
                Day01Visitors.TryGetChoicePreviewForVisit("D01_LAB", "lab-1", out invalidScoped) || invalidScoped != null ||
                Day01Visitors.TryGetChoicePreviewForVisit("D01_LAB", null, out invalidScoped) || invalidScoped != null)
                throw new Exception("Invalid visit-scoped preview must fail closed.");

            // Document menus must share the canonical visit order and immutable inspection hints.
            string[] menuDocumentIds = { "DOC-01", "DOC-02", "DOC-03" };
            for (int i = 0; i < menuDocumentIds.Length; i++)
            {
                IReadOnlyList<Day01Visitors.ChoicePreview> documentMenu;
                if (!Day01Visitors.TryGetChoicePreviewsByDocumentId(menuDocumentIds[i], out documentMenu) ||
                    documentMenu == null || documentMenu.Count != 3 ||
                    !(documentMenu is System.Collections.ObjectModel.ReadOnlyCollection<Day01Visitors.ChoicePreview>))
                    throw new Exception("Document menu missing or mutable: " + menuDocumentIds[i]);
                for (int j = 0; j < 3; j++)
                {
                    var preview = documentMenu[j];
                    if (preview.ChoiceId != menuChoices[i][j] ||
                        preview.VisitorCard.DocumentId != menuDocumentIds[i] ||
                        preview.RequiresInspection != (preview.ChoiceId == "LAB-2" || preview.ChoiceId == "PARTNER-2"))
                        throw new Exception("Document menu identity or inspection mismatch.");
                }
            }
            IReadOnlyList<Day01Visitors.ChoicePreview> unknownDocumentMenu;
            if (Day01Visitors.TryGetChoicePreviewsByDocumentId(null, out unknownDocumentMenu) || unknownDocumentMenu != null ||
                Day01Visitors.TryGetChoicePreviewsByDocumentId("doc-01", out unknownDocumentMenu) || unknownDocumentMenu != null ||
                Day01Visitors.TryGetChoicePreviewsByDocumentId("DOC-04", out unknownDocumentMenu) || unknownDocumentMenu != null)
                throw new Exception("Unknown document menu must fail closed.");

            // Document-focused Android/Windows UI must never show another document's choices.
            string[] scopedDocuments = { "DOC-01", "DOC-02", "DOC-03" };
            for (int i = 0; i < scopedDocuments.Length; i++)
            {
                foreach (string id in menuChoices[i])
                {
                    Day01Visitors.ChoicePreview matched;
                    if (!Day01Visitors.TryGetChoicePreviewForDocument(scopedDocuments[i], id, out matched) ||
                        matched == null || matched.ChoiceId != id ||
                        matched.VisitorCard.DocumentId != scopedDocuments[i])
                        throw new Exception("Document-scoped preview mismatch: " + id);
                    Day01Visitors.ChoicePreview wrongDocument;
                    if (Day01Visitors.TryGetChoicePreviewForDocument(scopedDocuments[(i + 1) % scopedDocuments.Length], id, out wrongDocument) ||
                        wrongDocument != null)
                        throw new Exception("Cross-document choice leaked: " + id);
                }
            }
            Day01Visitors.ChoicePreview invalidDocumentScope;
            if (Day01Visitors.TryGetChoicePreviewForDocument(null, "LAB-1", out invalidDocumentScope) || invalidDocumentScope != null ||
                Day01Visitors.TryGetChoicePreviewForDocument("doc-01", "LAB-1", out invalidDocumentScope) || invalidDocumentScope != null ||
                Day01Visitors.TryGetChoicePreviewForDocument("DOC-01", "PARTNER-2", out invalidDocumentScope) || invalidDocumentScope != null ||
                Day01Visitors.TryGetChoicePreviewForDocument("DOC-01", "lab-1", out invalidDocumentScope) || invalidDocumentScope != null ||
                Day01Visitors.TryGetChoicePreviewForDocument("DOC-01", null, out invalidDocumentScope) || invalidDocumentScope != null)
                throw new Exception("Invalid document-scoped preview must fail closed.");

            // A stale Android/Windows view must not combine a valid choice with a different document.
            for (int i = 0; i < menuVisits.Length; i++)
            {
                foreach (string id in menuChoices[i])
                {
                    Day01Visitors.ChoicePreview valid;
                    if (!Day01Visitors.TryGetChoicePreviewForVisitDocument(menuVisits[i], scopedDocuments[i], id, out valid) ||
                        valid == null || valid.ChoiceId != id)
                        throw new Exception("Visit/document/choice triple mismatch: " + id);
                    Day01Visitors.ChoicePreview stale;
                    if (Day01Visitors.TryGetChoicePreviewForVisitDocument(menuVisits[i], scopedDocuments[(i + 1) % 3], id, out stale) || stale != null ||
                        Day01Visitors.TryGetChoicePreviewForVisitDocument(menuVisits[(i + 1) % 3], scopedDocuments[i], id, out stale) || stale != null)
                        throw new Exception("Stale Day01 selection leaked: " + id);
                }
            }
            Day01Visitors.ChoicePreview invalidTriple;
            if (Day01Visitors.TryGetChoicePreviewForVisitDocument(null, "DOC-01", "LAB-1", out invalidTriple) || invalidTriple != null ||
                Day01Visitors.TryGetChoicePreviewForVisitDocument("D01_LAB", null, "LAB-1", out invalidTriple) || invalidTriple != null ||
                Day01Visitors.TryGetChoicePreviewForVisitDocument("D01_LAB", "DOC-01", "lab-1", out invalidTriple) || invalidTriple != null)
                throw new Exception("Invalid Day01 triple must fail closed.");

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
