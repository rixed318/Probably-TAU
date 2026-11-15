using System;
using System.Collections.Generic;

namespace TAU.President.Core
{
    /// <summary>Persisted occurrence identity and scheduling metadata; no NPC payload or IO.</summary>
    public sealed class ScheduledEventOccurrence
    {
        public string InstanceId { get; private set; }
        public string DefinitionId { get; private set; }
        public int DueDayIndex { get; private set; }
        public int Priority { get; private set; }

        public ScheduledEventOccurrence(string instanceId, string definitionId, int dueDayIndex, int priority)
        {
            InstanceId = GameStateSnapshotDto.Required(instanceId, nameof(instanceId));
            DefinitionId = GameStateSnapshotDto.Required(definitionId, nameof(definitionId));
            if (dueDayIndex < 0) throw new ArgumentOutOfRangeException(nameof(dueDayIndex));
            DueDayIndex = dueDayIndex;
            Priority = priority;
        }
    }

    /// <summary>
    /// Immutable Core-owned scheduled occurrence slice for ADR-C0-002.
    /// DueStage captures a fixed sorted set; processing and child creation belong to
    /// the future atomic AdvanceDay transaction, not this read-only snapshot.
    /// </summary>
    public sealed class ScheduledEventQueueSnapshot
    {
        private readonly List<ScheduledEventOccurrence> pending;
        private readonly List<string> completed;
        public IReadOnlyList<ScheduledEventOccurrence> Pending { get { return pending.AsReadOnly(); } }
        public IReadOnlyList<string> CompletedIds { get { return completed.AsReadOnly(); } }

        public ScheduledEventQueueSnapshot(IEnumerable<ScheduledEventOccurrence> pending,
            IEnumerable<string> completedIds)
        {
            if (pending == null) throw new ArgumentNullException(nameof(pending));
            if (completedIds == null) throw new ArgumentNullException(nameof(completedIds));
            this.pending = new List<ScheduledEventOccurrence>();
            this.completed = new List<string>();
            var ids = new HashSet<string>(StringComparer.Ordinal);
            foreach (var id in completedIds)
            {
                var valid = GameStateSnapshotDto.Required(id, nameof(completedIds));
                if (!ids.Add(valid)) throw new ArgumentException("Duplicate completed event ID.", nameof(completedIds));
                completed.Add(valid);
            }
            foreach (var occurrence in pending)
            {
                if (occurrence == null) throw new ArgumentException("Null occurrence.", nameof(pending));
                if (!ids.Add(occurrence.InstanceId))
                    throw new ArgumentException("Repeated or already completed event ID.", nameof(pending));
                this.pending.Add(occurrence);
            }
            this.pending.Sort(Compare);
            this.completed.Sort(StringComparer.Ordinal);
        }

        /// <summary>Fixed due-stage view: due ASC, priority DESC, instance ID ordinal ASC.</summary>
        public IReadOnlyList<ScheduledEventOccurrence> CaptureDueStage(int closingDayIndex)
        {
            if (closingDayIndex < 0) throw new ArgumentOutOfRangeException(nameof(closingDayIndex));
            var due = new List<ScheduledEventOccurrence>();
            foreach (var occurrence in pending)
            {
                if (occurrence.DueDayIndex > closingDayIndex) break;
                due.Add(occurrence);
            }
            return due.AsReadOnly();
        }

        /// <summary>ADR-C0-002: a child generated during closing cannot be due now or earlier.</summary>
        public static void ValidateChildDueDay(int closingDayIndex, int childDueDayIndex)
        {
            if (closingDayIndex < 0) throw new ArgumentOutOfRangeException(nameof(closingDayIndex));
            if (childDueDayIndex <= closingDayIndex)
                throw new ArgumentOutOfRangeException(nameof(childDueDayIndex));
        }

        public ScheduledEventQueueSnapshot Restore()
        {
            return new ScheduledEventQueueSnapshot(pending, completed);
        }

        private static int Compare(ScheduledEventOccurrence a, ScheduledEventOccurrence b)
        {
            int due = a.DueDayIndex.CompareTo(b.DueDayIndex);
            if (due != 0) return due;
            int priority = b.Priority.CompareTo(a.Priority);
            return priority != 0 ? priority : StringComparer.Ordinal.Compare(a.InstanceId, b.InstanceId);
        }
    }
}
