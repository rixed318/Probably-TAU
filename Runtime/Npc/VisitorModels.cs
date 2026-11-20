using System;
using System.Collections.Generic;

namespace TAU.President.Npc
{
    /// <summary>Immutable definition data; IDs match Content Day01 visitorId keys.</summary>
    public sealed class VisitorArchetype
    {
        public string DefinitionId { get; private set; }
        public string Role { get; private set; }

        public VisitorArchetype(string definitionId, string role)
        {
            DefinitionId = Require(definitionId, nameof(definitionId));
            Role = Require(role, nameof(role));
        }

        internal static string Require(string value, string name)
        {
            if (string.IsNullOrWhiteSpace(value) || value != value.Trim())
                throw new ArgumentException("Canonical nonempty value required.", name);
            return value;
        }
    }

    /// <summary>Immutable visit identity. This is a content occurrence, not a Core scheduled event.</summary>
    public sealed class VisitorInstance
    {
        public string InstanceId { get; private set; }
        public VisitorArchetype Archetype { get; private set; }
        public string DocumentId { get; private set; }
        public int DayIndex { get; private set; }

        public VisitorInstance(string instanceId, VisitorArchetype archetype, string documentId, int dayIndex)
        {
            InstanceId = VisitorArchetype.Require(instanceId, nameof(instanceId));
            Archetype = archetype ?? throw new ArgumentNullException(nameof(archetype));
            DocumentId = VisitorArchetype.Require(documentId, nameof(documentId));
            if (dayIndex < 0) throw new ArgumentOutOfRangeException(nameof(dayIndex));
            DayIndex = dayIndex;
        }
    }


    /// <summary>Read-only, Unity-free presentation snapshot for Windows and Android.
    /// Identity only: never a Core command, event receipt or authorization.</summary>
    public sealed class Day01VisitorCard
    {
        public string VisitId { get; private set; }
        public string VisitorId { get; private set; }
        public string DocumentId { get; private set; }
        public string Role { get; private set; }
        /// <summary>Planned Content art-link ID; does not imply an existing PNG.</summary>
        public string PlannedPortraitAssetId { get; private set; }
        /// <summary>Planned blank document art-link ID; document content stays in UI.</summary>
        public string PlannedDocumentAssetId { get; private set; }
        /// <summary>Planned standing sprite ID; not proof that a PNG exists.</summary>
        public string PlannedStandingAssetId { get; private set; }
        public int DayIndex { get; private set; }

        internal Day01VisitorCard(VisitorInstance source)
        {
            VisitId = source.InstanceId;
            VisitorId = source.Archetype.DefinitionId;
            DocumentId = source.DocumentId;
            Role = source.Archetype.Role;
            switch (source.InstanceId)
            {
                case "D01_LAB": PlannedPortraitAssetId = "irina_korneeva_portrait_neutral"; PlannedDocumentAssetId = "doc01_procurement_blank"; PlannedStandingAssetId = "irina_korneeva_standing"; break;
                case "D01_STUDENT": PlannedPortraitAssetId = "dasha_polynskaya_portrait_neutral"; PlannedDocumentAssetId = "doc02_student_proposal_blank"; PlannedStandingAssetId = "dasha_polynskaya_standing"; break;
                case "D01_PARTNER": PlannedPortraitAssetId = "mark_sviridov_portrait_neutral"; PlannedDocumentAssetId = "doc03_partner_letter_blank"; PlannedStandingAssetId = "mark_sviridov_standing"; break;
                default: throw new ArgumentException("Unknown Day01 portrait identity.", nameof(source));
            }
            DayIndex = source.DayIndex;
        }
    }

    /// <summary>Pure canonical Day01 mapping from Content/DAY01_UI_FIXTURE.json; no state or RNG.</summary>
    public static class Day01Visitors
    {
        /// <summary>Resolve a canonical Day01 visit ID; ordinal matching, no mutable cache.</summary>
        public static bool TryGetByVisitId(string visitId, out VisitorInstance visitor)
        {
            visitor = null;
            if (string.IsNullOrEmpty(visitId)) return false;
            foreach (var candidate in Create())
            {
                if (string.Equals(candidate.InstanceId, visitId, StringComparison.Ordinal))
                {
                    visitor = candidate;
                    return true;
                }
            }
            return false;
        }

        /// <summary>Canonical Content inspection metadata for a Day01 document.
        /// This is a read-only UI hint, not an inspection receipt or Core command.</summary>
        public sealed class InspectionPreview
        {
            public string DocumentId { get; private set; }
            public string InspectionId { get; private set; }
            public int CostTokens { get; private set; }
            public int RepeatCostTokens { get; private set; }

            internal InspectionPreview(string documentId, string inspectionId, int costTokens, int repeatCostTokens)
            {
                DocumentId = documentId;
                InspectionId = inspectionId;
                CostTokens = costTokens;
                RepeatCostTokens = repeatCostTokens;
            }
        }

        /// <summary>Read-only inspection metadata from Content Day01 fixture.
        /// The UI must ask Core for actual token balance and inspection completion.</summary>
        public static bool TryGetInspectionPreviewByDocumentId(string documentId, out InspectionPreview preview)
        {
            preview = null;
            switch (documentId)
            {
                case "DOC-01":
                    preview = new InspectionPreview("DOC-01", "check_seat_map", 1, 0);
                    return true;
                case "DOC-02":
                    preview = new InspectionPreview("DOC-02", "check_room_calendar", 1, 0);
                    return true;
                case "DOC-03":
                    preview = new InspectionPreview("DOC-03", "check_contract_appendix", 1, 0);
                    return true;
                default:
                    return false;
            }
        }

        /// <summary>Read-only inspection metadata for a canonical Day01 visit.
        /// Resolves through the Content document ID; never spends tokens or confirms a receipt.</summary>
        public static bool TryGetInspectionPreviewByVisitId(string visitId, out InspectionPreview preview)
        {
            preview = null;
            VisitorInstance visitor;
            if (!TryGetByVisitId(visitId, out visitor)) return false;
            return TryGetInspectionPreviewByDocumentId(visitor.DocumentId, out preview);
        }

        /// <summary>Read-only inspection metadata for a canonical Day01 choice.
        /// This describes the associated document's optional inspection, not a requirement
        /// to perform it, a receipt, or permission to execute the choice.</summary>
        public static bool TryGetInspectionPreviewByChoiceId(string choiceId, out InspectionPreview preview)
        {
            preview = null;
            ChoiceDescriptor choice;
            if (!TryDescribeChoice(choiceId, out choice)) return false;
            return TryGetInspectionPreviewByDocumentId(choice.Visitor.DocumentId, out preview);
        }

        /// <summary>Read-only inspection metadata for a canonical Content NPC definition ID.
        /// Useful when a Windows/Android card is keyed by npcId rather than visitId;
        /// never spends tokens, validates eligibility or confirms an inspection receipt.</summary>
        public static bool TryGetInspectionPreviewByVisitorId(string visitorId, out InspectionPreview preview)
        {
            preview = null;
            Day01VisitorCard card;
            if (!TryGetCardByVisitorId(visitorId, out card)) return false;
            return TryGetInspectionPreviewByDocumentId(card.DocumentId, out preview);
        }

        /// <summary>Resolve a canonical document to its Day01 visitor without mutable state.</summary>
        public static bool TryGetByDocumentId(string documentId, out VisitorInstance visitor)
        {
            visitor = null;
            if (string.IsNullOrEmpty(documentId)) return false;
            foreach (var candidate in Create())
            {
                if (string.Equals(candidate.DocumentId, documentId, StringComparison.Ordinal))
                {
                    visitor = candidate;
                    return true;
                }
            }
            return false;
        }

        /// <summary>Immutable Content Day01 choice identity and inspection prerequisite; not an authorization.</summary>
        public sealed class ChoiceDescriptor
        {
            public string ChoiceId { get; private set; }
            public VisitorInstance Visitor { get; private set; }
            public string RequiredInspectionDocumentId { get; private set; }

            internal ChoiceDescriptor(string choiceId, VisitorInstance visitor, string requiredInspectionDocumentId)
            {
                ChoiceId = choiceId;
                Visitor = visitor;
                RequiredInspectionDocumentId = requiredInspectionDocumentId;
            }
        }

        /// <summary>Read-only fixture metadata. Callers must independently verify inspection and Core state.</summary>
        public static bool TryDescribeChoice(string choiceId, out ChoiceDescriptor choice)
        {
            choice = null;
            string visitId;
            string inspection = null;
            switch (choiceId)
            {
                case "LAB-1":
                case "LAB-3":
                    visitId = "D01_LAB"; break;
                case "LAB-2":
                    visitId = "D01_LAB"; inspection = "DOC-01"; break;
                case "STUDENT-1":
                case "STUDENT-2":
                case "STUDENT-3":
                    visitId = "D01_STUDENT"; break;
                case "PARTNER-1":
                case "PARTNER-3":
                    visitId = "D01_PARTNER"; break;
                case "PARTNER-2":
                    visitId = "D01_PARTNER"; inspection = "DOC-03"; break;
                default:
                    return false;
            }
            VisitorInstance visitor;
            if (!TryGetByVisitId(visitId, out visitor)) return false;
            choice = new ChoiceDescriptor(choiceId, visitor, inspection);
            return true;
        }

        /// <summary>Resolve a Day01 choice to its canonical visitor; does not validate eligibility or apply effects.</summary>
        public static bool TryGetByChoiceId(string choiceId, out VisitorInstance visitor)
        {
            visitor = null;
            ChoiceDescriptor choice;
            if (!TryDescribeChoice(choiceId, out choice)) return false;
            visitor = choice.Visitor;
            return true;
        }

        /// <summary>Read-only presentation lookup for a canonical Content choice; no eligibility or Core mutation.</summary>
        public static bool TryGetCardByChoiceId(string choiceId, out Day01VisitorCard card)
        {
            card = null;
            VisitorInstance visitor;
            if (!TryGetByChoiceId(choiceId, out visitor)) return false;
            card = new Day01VisitorCard(visitor);
            return true;
        }

        /// <summary>Immutable UI-only choice preview: canonical visitor and inspection hint.
        /// The hint is not proof of inspection, eligibility or Core authorization.</summary>
        public sealed class ChoicePreview
        {
            public string ChoiceId { get; private set; }
            public Day01VisitorCard VisitorCard { get; private set; }
            public string RequiredInspectionDocumentId { get; private set; }
            /// <summary>Presentation hint only; Core must still authorize the decision.</summary>
            public bool RequiresInspection { get { return RequiredInspectionDocumentId != null; } }

            internal ChoicePreview(ChoiceDescriptor descriptor)
            {
                ChoiceId = descriptor.ChoiceId;
                VisitorCard = new Day01VisitorCard(descriptor.Visitor);
                RequiredInspectionDocumentId = descriptor.RequiredInspectionDocumentId;
            }
        }

        /// <summary>Resolve a Day01 choice to a platform-neutral preview; reject room/unknown IDs.</summary>
        public static bool TryGetChoicePreview(string choiceId, out ChoicePreview preview)
        {
            preview = null;
            ChoiceDescriptor descriptor;
            if (!TryDescribeChoice(choiceId, out descriptor)) return false;
            preview = new ChoicePreview(descriptor);
            return true;
        }

        /// <summary>Resolve a saved canonical visit ID to a fresh read-only card; no state mutation.</summary>
        public static bool TryGetCardByVisitId(string visitId, out Day01VisitorCard card)
        {
            card = null;
            VisitorInstance visitor;
            if (!TryGetByVisitId(visitId, out visitor)) return false;
            card = new Day01VisitorCard(visitor);
            return true;
        }

        /// <summary>Resolve a document tap to a read-only card; exact Content ID, no state mutation.</summary>
        public static bool TryGetCardByDocumentId(string documentId, out Day01VisitorCard card)
        {
            card = null;
            VisitorInstance visitor;
            if (!TryGetByDocumentId(documentId, out visitor)) return false;
            card = new Day01VisitorCard(visitor);
            return true;
        }

        /// <summary>Resolve a canonical Content visitor definition ID to a read-only card.
        /// UI identity only; never authorizes a Core decision or confirms art availability.</summary>
        public static bool TryGetCardByVisitorId(string visitorId, out Day01VisitorCard card)
        {
            card = null;
            if (string.IsNullOrEmpty(visitorId)) return false;
            foreach (var visitor in Create())
            {
                if (string.Equals(visitor.Archetype.DefinitionId, visitorId, StringComparison.Ordinal))
                {
                    card = new Day01VisitorCard(visitor);
                    return true;
                }
            }
            return false;
        }

        /// <summary>Resolve a planned Day01 art-link ID to its read-only visitor card.
        /// Works for portrait, standing sprite and document blank IDs; never verifies PNG existence.</summary>
        public static bool TryGetCardByPlannedAssetId(string assetId, out Day01VisitorCard card)
        {
            card = null;
            if (string.IsNullOrEmpty(assetId)) return false;
            foreach (var candidate in CreateReadOnlyCards())
            {
                if (string.Equals(candidate.PlannedPortraitAssetId, assetId, StringComparison.Ordinal) ||
                    string.Equals(candidate.PlannedStandingAssetId, assetId, StringComparison.Ordinal) ||
                    string.Equals(candidate.PlannedDocumentAssetId, assetId, StringComparison.Ordinal))
                {
                    card = candidate;
                    return true;
                }
            }
            return false;
        }

        /// <summary>Canonical choice IDs for one Day01 visit, in Content fixture order.
        /// Read-only UI navigation only; not an eligibility or Core command check.</summary>
        public static bool TryGetChoiceIdsByVisitId(string visitId, out IReadOnlyList<string> choiceIds)
        {
            choiceIds = null;
            switch (visitId)
            {
                case "D01_LAB":
                    choiceIds = Array.AsReadOnly(new[] { "LAB-1", "LAB-2", "LAB-3" });
                    return true;
                case "D01_STUDENT":
                    choiceIds = Array.AsReadOnly(new[] { "STUDENT-1", "STUDENT-2", "STUDENT-3" });
                    return true;
                case "D01_PARTNER":
                    choiceIds = Array.AsReadOnly(new[] { "PARTNER-1", "PARTNER-2", "PARTNER-3" });
                    return true;
                default:
                    return false;
            }
        }

        /// <summary>Ordered, read-only choice previews for a Day01 visitor menu on Windows/Android.
        /// UI hints only; Core owns eligibility, inspection receipts and state mutations.</summary>
        public static bool TryGetChoicePreviewsByVisitId(string visitId, out IReadOnlyList<ChoicePreview> previews)
        {
            previews = null;
            IReadOnlyList<string> choiceIds;
            if (!TryGetChoiceIdsByVisitId(visitId, out choiceIds)) return false;
            var result = new ChoicePreview[choiceIds.Count];
            for (int i = 0; i < choiceIds.Count; i++)
            {
                ChoicePreview preview;
                if (!TryGetChoicePreview(choiceIds[i], out preview)) return false;
                result[i] = preview;
            }
            previews = Array.AsReadOnly(result);
            return true;
        }

        /// <summary>Ordered, read-only choice previews for the currently displayed Day01 document.
        /// Presentation routing only; inspection and command authorization remain Core-owned.</summary>
        public static bool TryGetChoicePreviewsByDocumentId(string documentId, out IReadOnlyList<ChoicePreview> previews)
        {
            previews = null;
            VisitorInstance visitor;
            if (!TryGetByDocumentId(documentId, out visitor)) return false;
            return TryGetChoicePreviewsByVisitId(visitor.InstanceId, out previews);
        }

        /// <summary>Resolve a choice only when it belongs to the currently displayed visit.
        /// Read-only UI guard; Core still owns all command eligibility and inspection checks.</summary>
        public static bool TryGetChoicePreviewForVisit(string visitId, string choiceId, out ChoicePreview preview)
        {
            preview = null;
            if (string.IsNullOrEmpty(visitId)) return false;
            ChoicePreview candidate;
            if (!TryGetChoicePreview(choiceId, out candidate) ||
                !string.Equals(candidate.VisitorCard.VisitId, visitId, StringComparison.Ordinal))
                return false;
            preview = candidate;
            return true;
        }

        /// <summary>Resolve a Day01 choice only for the document currently displayed.
        /// Read-only Windows/Android routing guard; not inspection or Core authorization.</summary>
        public static bool TryGetChoicePreviewForDocument(string documentId, string choiceId, out ChoicePreview preview)
        {
            preview = null;
            if (string.IsNullOrEmpty(documentId)) return false;
            ChoicePreview candidate;
            if (!TryGetChoicePreview(choiceId, out candidate) ||
                !string.Equals(candidate.VisitorCard.DocumentId, documentId, StringComparison.Ordinal))
                return false;
            preview = candidate;
            return true;
        }

        /// <summary>Resolve a choice only when both displayed visit and document match Content.
        /// Presentation consistency guard, never Core authorization or inspection proof.</summary>
        public static bool TryGetChoicePreviewForVisitDocument(string visitId, string documentId,
            string choiceId, out ChoicePreview preview)
        {
            preview = null;
            ChoicePreview candidate;
            if (!TryGetChoicePreviewForVisit(visitId, choiceId, out candidate) ||
                !string.Equals(candidate.VisitorCard.DocumentId, documentId, StringComparison.Ordinal))
                return false;
            preview = candidate;
            return true;
        }

        /// <summary>Read-only inspection metadata only when the visit and document agree.
        /// This does not spend tokens, prove inspection or authorize a Core command.</summary>
        public static bool TryGetInspectionPreviewForVisitDocument(string visitId, string documentId,
            out InspectionPreview preview)
        {
            preview = null;
            VisitorInstance visitor;
            if (!TryGetByVisitId(visitId, out visitor) ||
                !string.Equals(visitor.DocumentId, documentId, StringComparison.Ordinal))
                return false;
            return TryGetInspectionPreviewByDocumentId(documentId, out preview);
        }

        /// <summary>Read-only inspection metadata for a matching visit, document and choice.
        /// This is presentation consistency only, not inspection eligibility or a Core receipt.</summary>
        public static bool TryGetInspectionPreviewForVisitDocumentChoice(string visitId, string documentId,
            string choiceId, out InspectionPreview preview)
        {
            preview = null;
            ChoicePreview choice;
            if (!TryGetChoicePreviewForVisitDocument(visitId, documentId, choiceId, out choice))
                return false;
            return TryGetInspectionPreviewByDocumentId(documentId, out preview);
        }

        /// <summary>One immutable Day01 screen snapshot for Windows/Android adapters.
        /// All fields are Content identity/presentation hints, never Core eligibility or state.</summary>
        public sealed class VisitScreenPreview
        {
            public Day01VisitorCard Card { get; private set; }
            public InspectionPreview Inspection { get; private set; }
            public IReadOnlyList<ChoicePreview> Choices { get; private set; }

            internal VisitScreenPreview(Day01VisitorCard card, InspectionPreview inspection,
                IReadOnlyList<ChoicePreview> choices)
            {
                Card = card;
                Inspection = inspection;
                Choices = choices;
            }
        }

        /// <summary>Build a consistent read-only screen from a canonical visit ID.
        /// Reject unknown visits; no token spending, event scheduling or GameState access.</summary>
        public static bool TryGetVisitScreenPreview(string visitId, out VisitScreenPreview screen)
        {
            screen = null;
            Day01VisitorCard card;
            InspectionPreview inspection;
            IReadOnlyList<ChoicePreview> choices;
            if (!TryGetCardByVisitId(visitId, out card) ||
                !TryGetInspectionPreviewByDocumentId(card.DocumentId, out inspection) ||
                !TryGetChoicePreviewsByVisitId(visitId, out choices))
                return false;
            screen = new VisitScreenPreview(card, inspection, choices);
            return true;
        }

        /// <summary>Build the same read-only Windows/Android visit screen from a canonical document ID.
        /// A stale or unknown document fails closed; this is not Core command authorization.</summary>
        public static bool TryGetVisitScreenPreviewByDocumentId(string documentId, out VisitScreenPreview screen)
        {
            screen = null;
            VisitorInstance visitor;
            if (!TryGetByDocumentId(documentId, out visitor)) return false;
            return TryGetVisitScreenPreview(visitor.InstanceId, out screen);
        }

        /// <summary>Resolve the same read-only Day01 screen from the canonical Content NPC ID.
        /// Identity routing only; Core remains responsible for decisions and receipts.</summary>
        public static bool TryGetVisitScreenPreviewByVisitorId(string visitorId, out VisitScreenPreview screen)
        {
            screen = null;
            Day01VisitorCard card;
            if (!TryGetCardByVisitorId(visitorId, out card)) return false;
            return TryGetVisitScreenPreview(card.VisitId, out screen);
        }

        /// <summary>Route a canonical Day01 decision ID to the same immutable screen as its visit.
        /// Navigation only: not a Core command, inspection receipt or eligibility check.</summary>
        public static bool TryGetVisitScreenPreviewByChoiceId(string choiceId, out VisitScreenPreview screen)
        {
            screen = null;
            VisitorInstance visitor;
            if (!TryGetByChoiceId(choiceId, out visitor)) return false;
            return TryGetVisitScreenPreview(visitor.InstanceId, out screen);
        }

        /// <summary>Resolve a Day01 screen only when visit, document and choice agree.
        /// Read-only presentation routing; not Core eligibility or a decision command.</summary>
        public static bool TryGetVisitScreenPreviewForVisitDocumentChoice(
            string visitId, string documentId, string choiceId, out VisitScreenPreview screen)
        {
            screen = null;
            ChoicePreview choice;
            if (!TryGetChoicePreviewForVisitDocument(visitId, documentId, choiceId, out choice))
                return false;
            return TryGetVisitScreenPreview(visitId, out screen);
        }

        /// <summary>Open the same Day01 read-only screen from a planned portrait, standing or document art ID.
        /// Asset IDs are routing hints only; this does not assert any PNG exists or authorize a decision.</summary>
        public static bool TryGetVisitScreenPreviewByPlannedAssetId(string assetId, out VisitScreenPreview screen)
        {
            screen = null;
            Day01VisitorCard card;
            if (!TryGetCardByPlannedAssetId(assetId, out card)) return false;
            return TryGetVisitScreenPreview(card.VisitId, out screen);
        }

        /// <summary>Resolve a planned art ID only for its matching canonical Day01 visit.
        /// Prevents cross-visitor artwork navigation; no PNG existence or Core authorization implied.</summary>
        public static bool TryGetVisitScreenPreviewForVisitAsset(
            string visitId, string assetId, out VisitScreenPreview screen)
        {
            screen = null;
            Day01VisitorCard card;
            if (!TryGetCardByPlannedAssetId(assetId, out card) ||
                !string.Equals(card.VisitId, visitId, StringComparison.Ordinal))
                return false;
            return TryGetVisitScreenPreview(visitId, out screen);
        }

        /// <summary>Route a planned Day01 art ID only for its matching canonical document.
        /// Windows/Android navigation consistency only; not proof of PNG or Core authorization.</summary>
        public static bool TryGetVisitScreenPreviewForDocumentAsset(
            string documentId, string assetId, out VisitScreenPreview screen)
        {
            screen = null;
            Day01VisitorCard card;
            if (!TryGetCardByPlannedAssetId(assetId, out card) ||
                !string.Equals(card.DocumentId, documentId, StringComparison.Ordinal))
                return false;
            return TryGetVisitScreenPreview(card.VisitId, out screen);
        }

        /// <summary>Resolve a planned art ID only for its matching canonical Content NPC ID.
        /// Android/Windows read-only routing; no PNG existence or Core authorization implied.</summary>
        public static bool TryGetVisitScreenPreviewForVisitorAsset(
            string visitorId, string assetId, out VisitScreenPreview screen)
        {
            screen = null;
            Day01VisitorCard card;
            if (!TryGetCardByPlannedAssetId(assetId, out card) ||
                !string.Equals(card.VisitorId, visitorId, StringComparison.Ordinal))
                return false;
            return TryGetVisitScreenPreview(card.VisitId, out screen);
        }

        /// <summary>Fresh immutable identity snapshots, with stable Content IDs on every platform.</summary>
        public static IReadOnlyList<Day01VisitorCard> CreateReadOnlyCards()
        {
            var visits = Create();
            var cards = new Day01VisitorCard[visits.Count];
            for (int i = 0; i < visits.Count; i++)
                cards[i] = new Day01VisitorCard(visits[i]);
            return Array.AsReadOnly(cards);
        }

        public static IReadOnlyList<VisitorInstance> Create()
        {
            return Array.AsReadOnly(new[]
            {
                new VisitorInstance("D01_LAB", new VisitorArchetype("npc_irina_korneeva", "координатор цифровых дисциплин"), "DOC-01", 0),
                new VisitorInstance("D01_STUDENT", new VisitorArchetype("npc_dasha_polynskaya", "студенческий организатор"), "DOC-02", 0),
                new VisitorInstance("D01_PARTNER", new VisitorArchetype("npc_mark_sviridov", "представитель вымышленной фирмы «Пульс Инжиниринг»"), "DOC-03", 0)
            });
        }
    }
}
