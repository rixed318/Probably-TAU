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
