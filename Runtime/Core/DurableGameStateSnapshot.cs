using System;
using System.Collections.Generic;

namespace TAU.President.Core
{
    /// <summary>
    /// Core-owned durable composition boundary. Does not serialize, execute events,
    /// post ledger entries or advance the clock.
    /// </summary>
    public sealed class DurableGameStateSnapshot
    {
        public DurableFinanceSnapshot Finance { get; private set; }
        public ScheduledEventQueueSnapshot Events { get; private set; }
        public GameStateSnapshotDto Header { get { return Finance.Header; } }

        public DurableGameStateSnapshot(DurableFinanceSnapshot finance,
            ScheduledEventQueueSnapshot events)
        {
            if (finance == null) throw new ArgumentNullException(nameof(finance));
            if (events == null) throw new ArgumentNullException(nameof(events));
            ValidateIds(finance.Header.PendingEventIds, events.Pending, true);
            ValidateIds(finance.Header.CompletedEventIds, events.CompletedIds, false);
            Finance = finance.Restore();
            Events = events.Restore();
        }

        public DurableGameStateSnapshot Restore()
        {
            return new DurableGameStateSnapshot(Finance, Events);
        }

        private static void ValidateIds(IEnumerable<string> headerIds,
            IReadOnlyList<ScheduledEventOccurrence> occurrences, bool pending)
        {
            var expected = new HashSet<string>(headerIds, StringComparer.Ordinal);
            if (expected.Count != occurrences.Count)
                throw new ArgumentException("Header and typed pending event IDs disagree.");
            foreach (var item in occurrences)
                if (!expected.Remove(item.InstanceId))
                    throw new ArgumentException("Header and typed pending event IDs disagree.");
            if (expected.Count != 0)
                throw new ArgumentException("Header and typed pending event IDs disagree.");
        }

        private static void ValidateIds(IEnumerable<string> headerIds,
            IReadOnlyList<string> completedIds, bool pending)
        {
            var expected = new HashSet<string>(headerIds, StringComparer.Ordinal);
            if (expected.Count != completedIds.Count)
                throw new ArgumentException("Header and typed completed event IDs disagree.");
            foreach (var id in completedIds)
                if (!expected.Remove(id))
                    throw new ArgumentException("Header and typed completed event IDs disagree.");
            if (expected.Count != 0)
                throw new ArgumentException("Header and typed completed event IDs disagree.");
        }
    }
}
