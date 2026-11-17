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
