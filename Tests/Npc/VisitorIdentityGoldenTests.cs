using System;
using System.Collections.Generic;
using TAU.President.Npc;

namespace TAU.President.Tests.Npc
{
    public static class VisitorIdentityGoldenTests
    {
        public static void RunAll()
        {
            // Document-scoped art routing must reject cross-document images on both platforms.
            string[][] artByDocument =
            {
                new[] { "irina_korneeva_portrait_neutral", "irina_korneeva_standing", "doc01_procurement_blank" },
                new[] { "dasha_polynskaya_portrait_neutral", "dasha_polynskaya_standing", "doc02_student_proposal_blank" },
                new[] { "mark_sviridov_portrait_neutral", "mark_sviridov_standing", "doc03_partner_letter_blank" }
            };
            string[] artDocuments = { "DOC-01", "DOC-02", "DOC-03" };
            for (int i = 0; i < artByDocument.Length; i++)
                foreach (string artId in artByDocument[i])
                    for (int j = 0; j < artDocuments.Length; j++)
                    {
                        Day01Visitors.VisitScreenPreview screen;
                        bool found = Day01Visitors.TryGetVisitScreenPreviewForDocumentAsset(
                            artDocuments[j], artId, out screen);
                        if (found != (i == j) || (found &&
                            (screen == null || screen.Card.DocumentId != artDocuments[i])) ||
                            (!found && screen != null))
                            throw new Exception("Document/art routing mismatch: " + artDocuments[j] + "/" + artId);
                    }
            // A document-scoped art tap must preserve the complete canonical screen,
            // including inspection hint and ordered choices, not just the visitor card.
            for (int scopedIndex = 0; scopedIndex < artDocuments.Length; scopedIndex++)
            {
                Day01Visitors.VisitScreenPreview scopedCanonicalScreen;
                if (!Day01Visitors.TryGetVisitScreenPreviewByDocumentId(
                    artDocuments[scopedIndex], out scopedCanonicalScreen) || scopedCanonicalScreen == null)
                    throw new Exception("Canonical document screen missing: " + artDocuments[scopedIndex]);
                foreach (string scopedAssetId in artByDocument[scopedIndex])
                {
                    Day01Visitors.VisitScreenPreview scopedArtScreen;
                    if (!Day01Visitors.TryGetVisitScreenPreviewForDocumentAsset(
                        artDocuments[scopedIndex], scopedAssetId, out scopedArtScreen) ||
                        scopedArtScreen == null ||
                        scopedArtScreen.Card.VisitId != scopedCanonicalScreen.Card.VisitId ||
                        scopedArtScreen.Inspection.InspectionId != scopedCanonicalScreen.Inspection.InspectionId ||
                        scopedArtScreen.Inspection.CostTokens != scopedCanonicalScreen.Inspection.CostTokens ||
                        scopedArtScreen.Choices.Count != scopedCanonicalScreen.Choices.Count)
                        throw new Exception("Document art screen metadata mismatch: " + scopedAssetId);
                    for (int scopedChoiceIndex = 0; scopedChoiceIndex < scopedCanonicalScreen.Choices.Count; scopedChoiceIndex++)
                        if (scopedArtScreen.Choices[scopedChoiceIndex].ChoiceId !=
                            scopedCanonicalScreen.Choices[scopedChoiceIndex].ChoiceId)
                            throw new Exception("Document art choice order mismatch: " + scopedAssetId);
                }
            }
            foreach (string invalid in new[] { null, "", "doc-01", "DOC-04", " DOC-01", "DOC-01 " })
            {
                Day01Visitors.VisitScreenPreview missingDocumentArtScreen;
                if (Day01Visitors.TryGetVisitScreenPreviewForDocumentAsset(
                    invalid, artByDocument[0][0], out missingDocumentArtScreen) || missingDocumentArtScreen != null)
                    throw new Exception("Invalid document/art route accepted.");
            }
            // Missing and case-mismatched art IDs must fail closed on Windows and Android.
            foreach (string invalidDocumentAssetId in new[] { null, "", "unknown_asset", "IRINA_KORNEEVA_PORTRAIT_NEUTRAL", " irina_korneeva_portrait_neutral", "irina_korneeva_portrait_neutral " })
            {
                Day01Visitors.VisitScreenPreview invalidArtScreen;
                if (Day01Visitors.TryGetVisitScreenPreviewForDocumentAsset(
                    "DOC-01", invalidDocumentAssetId, out invalidArtScreen) || invalidArtScreen != null)
                    throw new Exception("Invalid document art ID accepted.");
            }

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
            foreach (string invalidChoice in new[] { null, "", "lab-1", "LAB-4", "ROOM-1", " LAB-1", "LAB-1 " })
            {
                Day01Visitors.InspectionPreview invalidChoiceInspection;
                if (Day01Visitors.TryGetInspectionPreviewByChoiceId(invalidChoice, out invalidChoiceInspection) ||
                    invalidChoiceInspection != null)
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
            // Canonical Content IDs must be globally collision-free across the Day01 screen routes.
            // A collision could silently open the wrong NPC on Windows or Android.
            var uniqueVisits = new HashSet<string>(StringComparer.Ordinal);
            var uniqueVisitors = new HashSet<string>(StringComparer.Ordinal);
            var uniqueDocuments = new HashSet<string>(StringComparer.Ordinal);
            var uniqueAssets = new HashSet<string>(StringComparer.Ordinal);
            for (int i = 0; i < cards.Count; i++)
            {
                if (!uniqueVisits.Add(cards[i].VisitId) ||
                    !uniqueVisitors.Add(cards[i].VisitorId) ||
                    !uniqueDocuments.Add(cards[i].DocumentId) ||
                    !uniqueAssets.Add(cards[i].PlannedPortraitAssetId) ||
                    !uniqueAssets.Add(cards[i].PlannedStandingAssetId) ||
                    !uniqueAssets.Add(cards[i].PlannedDocumentAssetId))
                    throw new Exception("Duplicate canonical Day01 identity or planned art ID.");
            }
            if (uniqueAssets.Count != 9)
                throw new Exception("Day01 art manifest must have nine distinct planned IDs.");
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

            // Planned art navigation must resolve the same read-only screen as canonical visit routing.
            for (int i = 0; i < cards.Count; i++)
            {
                Day01Visitors.VisitScreenPreview canonical;
                if (!Day01Visitors.TryGetVisitScreenPreview(cards[i].VisitId, out canonical))
                    throw new Exception("Canonical screen missing for planned asset test.");
                foreach (string assetId in new[] { plannedPortraitIds[i], plannedStandingIds[i], plannedDocumentIds[i] })
                {
                    Day01Visitors.VisitScreenPreview byAsset;
                    if (!Day01Visitors.TryGetVisitScreenPreviewByPlannedAssetId(assetId, out byAsset) ||
                        byAsset == null || byAsset.Card.VisitId != canonical.Card.VisitId ||
                        byAsset.Card.DocumentId != canonical.Card.DocumentId ||
                        byAsset.Inspection.InspectionId != canonical.Inspection.InspectionId ||
                        byAsset.Choices.Count != canonical.Choices.Count)
                        throw new Exception("Planned asset screen mismatch: " + assetId);
                    for (int j = 0; j < canonical.Choices.Count; j++)
                        if (byAsset.Choices[j].ChoiceId != canonical.Choices[j].ChoiceId)
                            throw new Exception("Planned asset screen choice order mismatch: " + assetId);
                }
            }
            foreach (string invalid in new[] { null, "", "Irina_korneeva_standing", "missing_asset", " doc01_procurement_blank" })
            {
                Day01Visitors.VisitScreenPreview missingScreen;
                if (Day01Visitors.TryGetVisitScreenPreviewByPlannedAssetId(invalid, out missingScreen) ||
                    missingScreen != null)
                    throw new Exception("Invalid planned asset must not resolve a screen.");
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

            // Inspection metadata must not leak across a stale visit/document pairing.
            string[] inspectionVisits = { "D01_LAB", "D01_STUDENT", "D01_PARTNER" };
            string[] inspectionDocuments = { "DOC-01", "DOC-02", "DOC-03" };
            for (int i = 0; i < inspectionVisits.Length; i++)
            {
                Day01Visitors.InspectionPreview matched;
                if (!Day01Visitors.TryGetInspectionPreviewForVisitDocument(
                    inspectionVisits[i], inspectionDocuments[i], out matched) ||
                    matched == null || matched.DocumentId != inspectionDocuments[i])
                    throw new Exception("Visit/document inspection preview mismatch: " + inspectionVisits[i]);
                Day01Visitors.InspectionPreview stale;
                if (Day01Visitors.TryGetInspectionPreviewForVisitDocument(
                    inspectionVisits[i], inspectionDocuments[(i + 1) % 3], out stale) || stale != null)
                    throw new Exception("Cross-document inspection preview leaked.");
            }
            Day01Visitors.InspectionPreview invalidInspectionPair;
            if (Day01Visitors.TryGetInspectionPreviewForVisitDocument(null, "DOC-01", out invalidInspectionPair) || invalidInspectionPair != null ||
                Day01Visitors.TryGetInspectionPreviewForVisitDocument("D01_LAB", null, out invalidInspectionPair) || invalidInspectionPair != null ||
                Day01Visitors.TryGetInspectionPreviewForVisitDocument("d01_lab", "DOC-01", out invalidInspectionPair) || invalidInspectionPair != null ||
                Day01Visitors.TryGetInspectionPreviewForVisitDocument("D01_LAB", "doc-01", out invalidInspectionPair) || invalidInspectionPair != null ||
                Day01Visitors.TryGetInspectionPreviewForVisitDocument("D01_UNKNOWN", "DOC-01", out invalidInspectionPair) || invalidInspectionPair != null)
                throw new Exception("Invalid visit/document inspection preview must fail closed.");

            // Triple-scoped inspection metadata must never leak to a stale UI selection.
            string[] tripleVisits = { "D01_LAB", "D01_STUDENT", "D01_PARTNER" };
            string[] tripleDocs = { "DOC-01", "DOC-02", "DOC-03" };
            string[] tripleChoices = { "LAB-1", "STUDENT-2", "PARTNER-3" };
            for (int i = 0; i < tripleVisits.Length; i++)
            {
                Day01Visitors.InspectionPreview ok;
                if (!Day01Visitors.TryGetInspectionPreviewForVisitDocumentChoice(
                    tripleVisits[i], tripleDocs[i], tripleChoices[i], out ok) ||
                    ok == null || ok.DocumentId != tripleDocs[i])
                    throw new Exception("Triple-scoped inspection metadata mismatch.");
                Day01Visitors.InspectionPreview wrong;
                if (Day01Visitors.TryGetInspectionPreviewForVisitDocumentChoice(
                    tripleVisits[i], tripleDocs[i], tripleChoices[(i + 1) % 3], out wrong) || wrong != null ||
                    Day01Visitors.TryGetInspectionPreviewForVisitDocumentChoice(
                    tripleVisits[i], tripleDocs[(i + 1) % 3], tripleChoices[i], out wrong) || wrong != null ||
                    Day01Visitors.TryGetInspectionPreviewForVisitDocumentChoice(
                    tripleVisits[(i + 1) % 3], tripleDocs[i], tripleChoices[i], out wrong) || wrong != null)
                    throw new Exception("Stale triple leaked inspection metadata.");
            }
            Day01Visitors.InspectionPreview invalidTripleInspection;
            if (Day01Visitors.TryGetInspectionPreviewForVisitDocumentChoice(null, "DOC-01", "LAB-1", out invalidTripleInspection) || invalidTripleInspection != null ||
                Day01Visitors.TryGetInspectionPreviewForVisitDocumentChoice("D01_LAB", null, "LAB-1", out invalidTripleInspection) || invalidTripleInspection != null ||
                Day01Visitors.TryGetInspectionPreviewForVisitDocumentChoice("D01_LAB", "DOC-01", "lab-1", out invalidTripleInspection) || invalidTripleInspection != null)
                throw new Exception("Invalid triple inspection metadata must fail closed.");

            // NPC definition IDs also resolve inspection hints for Windows/Android cards.
            string[] inspectionNpcIds = { "npc_irina_korneeva", "npc_dasha_polynskaya", "npc_mark_sviridov" };
            string[] inspectionNpcDocs = { "DOC-01", "DOC-02", "DOC-03" };
            string[] inspectionNpcChecks = { "check_seat_map", "check_room_calendar", "check_contract_appendix" };
            for (int i = 0; i < inspectionNpcIds.Length; i++)
            {
                Day01Visitors.InspectionPreview hint;
                if (!Day01Visitors.TryGetInspectionPreviewByVisitorId(inspectionNpcIds[i], out hint) ||
                    hint == null || hint.DocumentId != inspectionNpcDocs[i] ||
                    hint.InspectionId != inspectionNpcChecks[i] ||
                    hint.CostTokens != 1 || hint.RepeatCostTokens != 0)
                    throw new Exception("NPC inspection hint mismatch: " + inspectionNpcIds[i]);
            }
            Day01Visitors.InspectionPreview invalidNpcHint;
            if (Day01Visitors.TryGetInspectionPreviewByVisitorId(null, out invalidNpcHint) || invalidNpcHint != null ||
                Day01Visitors.TryGetInspectionPreviewByVisitorId("", out invalidNpcHint) || invalidNpcHint != null ||
                Day01Visitors.TryGetInspectionPreviewByVisitorId("NPC_IRINA_KORNEEVA", out invalidNpcHint) || invalidNpcHint != null ||
                Day01Visitors.TryGetInspectionPreviewByVisitorId("npc_unknown", out invalidNpcHint) || invalidNpcHint != null)
                throw new Exception("Unknown NPC inspection hint must fail closed.");

            // A single immutable screen snapshot keeps Android and Windows visit routing consistent.
            string[] screenVisits = { "D01_LAB", "D01_STUDENT", "D01_PARTNER" };
            string[] screenDocs = { "DOC-01", "DOC-02", "DOC-03" };
            string[] screenChecks = { "check_seat_map", "check_room_calendar", "check_contract_appendix" };
            string[][] screenChoices = {
                new[] { "LAB-1", "LAB-2", "LAB-3" },
                new[] { "STUDENT-1", "STUDENT-2", "STUDENT-3" },
                new[] { "PARTNER-1", "PARTNER-2", "PARTNER-3" }
            };
            for (int i = 0; i < screenVisits.Length; i++)
            {
                Day01Visitors.VisitScreenPreview screen;
                if (!Day01Visitors.TryGetVisitScreenPreview(screenVisits[i], out screen) ||
                    screen == null || screen.Card.VisitId != screenVisits[i] ||
                    screen.Card.DocumentId != screenDocs[i] ||
                    screen.Inspection.DocumentId != screenDocs[i] ||
                    screen.Inspection.InspectionId != screenChecks[i] ||
                    screen.Choices.Count != 3)
                    throw new Exception("Day01 screen snapshot identity mismatch: " + screenVisits[i]);
                for (int j = 0; j < 3; j++)
                    if (screen.Choices[j].ChoiceId != screenChoices[i][j] ||
                        screen.Choices[j].VisitorCard.DocumentId != screenDocs[i])
                        throw new Exception("Day01 screen snapshot choice mismatch.");
                if (screen.Choices is System.Collections.Generic.IList<Day01Visitors.ChoicePreview>)
                {
                    var list = (System.Collections.Generic.IList<Day01Visitors.ChoicePreview>)screen.Choices;
                    try { list[0] = null; throw new Exception("Screen choices must be read-only."); }
                    catch (NotSupportedException) { }
                }
            }
            Day01Visitors.VisitScreenPreview unknownScreen;
            if (Day01Visitors.TryGetVisitScreenPreview(null, out unknownScreen) || unknownScreen != null ||
                Day01Visitors.TryGetVisitScreenPreview("", out unknownScreen) || unknownScreen != null ||
                Day01Visitors.TryGetVisitScreenPreview("d01_lab", out unknownScreen) || unknownScreen != null ||
                Day01Visitors.TryGetVisitScreenPreview("D01_UNKNOWN", out unknownScreen) || unknownScreen != null)
                throw new Exception("Unknown Day01 screen must fail closed.");

            // Document-driven navigation must resolve the identical immutable Day01 screen
            // on Windows and Android, without guessing a visit or touching Core state.
            string[] screenDocumentIds = { "DOC-01", "DOC-02", "DOC-03" };
            string[] screenVisitIds = { "D01_LAB", "D01_STUDENT", "D01_PARTNER" };
            for (int i = 0; i < screenDocumentIds.Length; i++)
            {
                Day01Visitors.VisitScreenPreview byDocument;
                Day01Visitors.VisitScreenPreview byVisit;
                if (!Day01Visitors.TryGetVisitScreenPreviewByDocumentId(screenDocumentIds[i], out byDocument) ||
                    !Day01Visitors.TryGetVisitScreenPreview(screenVisitIds[i], out byVisit) ||
                    byDocument == null || byVisit == null ||
                    byDocument.Card.VisitId != byVisit.Card.VisitId ||
                    byDocument.Card.DocumentId != screenDocumentIds[i] ||
                    byDocument.Inspection.InspectionId != byVisit.Inspection.InspectionId ||
                    byDocument.Choices.Count != byVisit.Choices.Count)
                    throw new Exception("Document screen route differs from visit screen route.");
                for (int j = 0; j < byVisit.Choices.Count; j++)
                    if (byDocument.Choices[j].ChoiceId != byVisit.Choices[j].ChoiceId)
                        throw new Exception("Document screen route changed Content choice order.");
            }
            Day01Visitors.VisitScreenPreview invalidDocumentScreen;
            if (Day01Visitors.TryGetVisitScreenPreviewByDocumentId(null, out invalidDocumentScreen) || invalidDocumentScreen != null ||
                Day01Visitors.TryGetVisitScreenPreviewByDocumentId("", out invalidDocumentScreen) || invalidDocumentScreen != null ||
                Day01Visitors.TryGetVisitScreenPreviewByDocumentId("doc-01", out invalidDocumentScreen) || invalidDocumentScreen != null ||
                Day01Visitors.TryGetVisitScreenPreviewByDocumentId("DOC-04", out invalidDocumentScreen) || invalidDocumentScreen != null)
                throw new Exception("Unknown document screen route must fail closed.");

            // NPC ID, visit ID and document ID must produce identical read-only Day01 screens.
            string[] canonicalNpcIds = { "npc_irina_korneeva", "npc_dasha_polynskaya", "npc_mark_sviridov" };
            string[] canonicalVisitIds = { "D01_LAB", "D01_STUDENT", "D01_PARTNER" };
            for (int i = 0; i < canonicalNpcIds.Length; i++)
            {
                Day01Visitors.VisitScreenPreview byNpc;
                Day01Visitors.VisitScreenPreview byVisit;
                if (!Day01Visitors.TryGetVisitScreenPreviewByVisitorId(canonicalNpcIds[i], out byNpc) ||
                    !Day01Visitors.TryGetVisitScreenPreview(canonicalVisitIds[i], out byVisit) ||
                    byNpc == null || byVisit == null ||
                    byNpc.Card.VisitorId != canonicalNpcIds[i] ||
                    byNpc.Card.VisitId != byVisit.Card.VisitId ||
                    byNpc.Card.DocumentId != byVisit.Card.DocumentId ||
                    byNpc.Inspection.InspectionId != byVisit.Inspection.InspectionId ||
                    byNpc.Choices.Count != byVisit.Choices.Count)
                    throw new Exception("NPC-routed screen differs from canonical visit screen.");
                for (int j = 0; j < byVisit.Choices.Count; j++)
                    if (byNpc.Choices[j].ChoiceId != byVisit.Choices[j].ChoiceId)
                        throw new Exception("NPC-routed screen changed choice order.");
            }
            Day01Visitors.VisitScreenPreview invalidNpcScreen;
            if (Day01Visitors.TryGetVisitScreenPreviewByVisitorId(null, out invalidNpcScreen) || invalidNpcScreen != null ||
                Day01Visitors.TryGetVisitScreenPreviewByVisitorId("", out invalidNpcScreen) || invalidNpcScreen != null ||
                Day01Visitors.TryGetVisitScreenPreviewByVisitorId("NPC_IRINA_KORNEEVA", out invalidNpcScreen) || invalidNpcScreen != null ||
                Day01Visitors.TryGetVisitScreenPreviewByVisitorId("npc_unknown", out invalidNpcScreen) || invalidNpcScreen != null)
                throw new Exception("Unknown NPC screen route must fail closed.");

            // Every canonical decision opens the identical read-only Day01 screen on Windows/Android.
            string[] choiceScreenVisits = { "D01_LAB", "D01_STUDENT", "D01_PARTNER" };
            string[][] choiceScreenIds = {
                new[] { "LAB-1", "LAB-2", "LAB-3" },
                new[] { "STUDENT-1", "STUDENT-2", "STUDENT-3" },
                new[] { "PARTNER-1", "PARTNER-2", "PARTNER-3" }
            };
            for (int i = 0; i < choiceScreenVisits.Length; i++)
            {
                Day01Visitors.VisitScreenPreview byVisit;
                if (!Day01Visitors.TryGetVisitScreenPreview(choiceScreenVisits[i], out byVisit))
                    throw new Exception("Missing canonical visit screen.");
                foreach (string choiceId in choiceScreenIds[i])
                {
                    Day01Visitors.VisitScreenPreview byChoice;
                    if (!Day01Visitors.TryGetVisitScreenPreviewByChoiceId(choiceId, out byChoice) ||
                        byChoice == null || byChoice.Card.VisitId != byVisit.Card.VisitId ||
                        byChoice.Card.DocumentId != byVisit.Card.DocumentId ||
                        byChoice.Inspection.InspectionId != byVisit.Inspection.InspectionId ||
                        byChoice.Choices.Count != byVisit.Choices.Count)
                        throw new Exception("Choice-routed Day01 screen mismatch: " + choiceId);
                    for (int j = 0; j < byVisit.Choices.Count; j++)
                        if (byChoice.Choices[j].ChoiceId != byVisit.Choices[j].ChoiceId)
                            throw new Exception("Choice-routed Day01 menu order changed.");
                }
            }
            Day01Visitors.VisitScreenPreview invalidChoiceScreen;
            if (Day01Visitors.TryGetVisitScreenPreviewByChoiceId(null, out invalidChoiceScreen) || invalidChoiceScreen != null ||
                Day01Visitors.TryGetVisitScreenPreviewByChoiceId("", out invalidChoiceScreen) || invalidChoiceScreen != null ||
                Day01Visitors.TryGetVisitScreenPreviewByChoiceId("lab-1", out invalidChoiceScreen) || invalidChoiceScreen != null ||
                Day01Visitors.TryGetVisitScreenPreviewByChoiceId("ROOM-1", out invalidChoiceScreen) || invalidChoiceScreen != null ||
                Day01Visitors.TryGetVisitScreenPreviewByChoiceId("UNKNOWN", out invalidChoiceScreen) || invalidChoiceScreen != null)
                throw new Exception("Invalid choice-routed Day01 screen must fail closed.");

            // Triple-scoped screen routing must reject stale/mixed UI identities.
            string[] scopedVisits = { "D01_LAB", "D01_STUDENT", "D01_PARTNER" };
            // Reuse scopedDocuments declared above for the same canonical Day01 fixture.
            string[][] scopedChoices =
            {
                new[] { "LAB-1", "LAB-2", "LAB-3" },
                new[] { "STUDENT-1", "STUDENT-2", "STUDENT-3" },
                new[] { "PARTNER-1", "PARTNER-2", "PARTNER-3" }
            };
            for (int i = 0; i < scopedVisits.Length; i++)
            {
                foreach (string id in scopedChoices[i])
                {
                    Day01Visitors.VisitScreenPreview scoped;
                    if (!Day01Visitors.TryGetVisitScreenPreviewForVisitDocumentChoice(
                            scopedVisits[i], scopedDocuments[i], id, out scoped) ||
                        scoped == null || scoped.Card.VisitId != scopedVisits[i] ||
                        scoped.Card.DocumentId != scopedDocuments[i] ||
                        scoped.Choices.Count != 3)
                        throw new Exception("Triple-scoped screen mapping mismatch: " + id);
                    Day01Visitors.VisitScreenPreview mixed;
                    if (Day01Visitors.TryGetVisitScreenPreviewForVisitDocumentChoice(
                            scopedVisits[i], scopedDocuments[(i + 1) % 3], id, out mixed) || mixed != null ||
                        Day01Visitors.TryGetVisitScreenPreviewForVisitDocumentChoice(
                            scopedVisits[(i + 1) % 3], scopedDocuments[i], id, out mixed) || mixed != null ||
                        Day01Visitors.TryGetVisitScreenPreviewForVisitDocumentChoice(
                            scopedVisits[i], scopedDocuments[i], scopedChoices[(i + 1) % 3][0], out mixed) || mixed != null)
                        throw new Exception("Mixed triple must not resolve a screen: " + id);
                }
            }
            Day01Visitors.VisitScreenPreview absentScoped;
            if (Day01Visitors.TryGetVisitScreenPreviewForVisitDocumentChoice(null, "DOC-01", "LAB-1", out absentScoped) || absentScoped != null ||
                Day01Visitors.TryGetVisitScreenPreviewForVisitDocumentChoice("D01_LAB", null, "LAB-1", out absentScoped) || absentScoped != null ||
                Day01Visitors.TryGetVisitScreenPreviewForVisitDocumentChoice("D01_LAB", "DOC-01", null, out absentScoped) || absentScoped != null ||
                Day01Visitors.TryGetVisitScreenPreviewForVisitDocumentChoice("d01_lab", "DOC-01", "LAB-1", out absentScoped) || absentScoped != null)
                throw new Exception("Invalid triple must fail closed.");

            // Planned art links must not route a different visitor's screen on either platform.
            foreach (var card in Day01Visitors.CreateReadOnlyCards())
            {
                foreach (var assetId in new[] { card.PlannedPortraitAssetId, card.PlannedStandingAssetId, card.PlannedDocumentAssetId })
                {
                    Day01Visitors.VisitScreenPreview matched;
                    if (!Day01Visitors.TryGetVisitScreenPreviewForVisitAsset(card.VisitId, assetId, out matched) ||
                        matched == null || matched.Card.VisitId != card.VisitId ||
                        matched.Card.DocumentId != card.DocumentId || matched.Choices.Count != 3)
                        throw new Exception("Matching visit/asset route failed: " + assetId);
                    foreach (var other in Day01Visitors.CreateReadOnlyCards())
                    {
                        if (other.VisitId == card.VisitId) continue;
                        Day01Visitors.VisitScreenPreview mismatch;
                        if (Day01Visitors.TryGetVisitScreenPreviewForVisitAsset(other.VisitId, assetId, out mismatch) ||
                            mismatch != null)
                            throw new Exception("Cross-visitor asset route accepted: " + assetId);
                    }
                }
            }
            Day01Visitors.VisitScreenPreview missingAssetScreen;
            if (Day01Visitors.TryGetVisitScreenPreviewForVisitAsset(null, null, out missingAssetScreen) || missingAssetScreen != null ||
                Day01Visitors.TryGetVisitScreenPreviewForVisitAsset("D01_LAB", "unknown_asset", out missingAssetScreen) || missingAssetScreen != null ||
                Day01Visitors.TryGetVisitScreenPreviewForVisitAsset("d01_lab", Day01Visitors.CreateReadOnlyCards()[0].PlannedPortraitAssetId, out missingAssetScreen) || missingAssetScreen != null)
                throw new Exception("Invalid visit/asset pair must fail closed.");

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
