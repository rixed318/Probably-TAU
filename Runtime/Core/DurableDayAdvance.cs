using System;
using System.Collections.Generic;
using System.Globalization;

namespace TAU.President.Core
{
    /// <summary>Fail-closed first EndDay transaction boundary; caller publishes result atomically.</summary>
    public static class DurableDayAdvance
    {
        public static DurableGameStateSnapshot PrepareQuietDay(
            DurableGameStateSnapshot current, long expectedRevision, int expectedDayIndex)
        {
            if (current == null) throw new ArgumentNullException(nameof(current));
            var h = current.Header;
            if (h.IsFinished) throw new InvalidOperationException("Campaign finished.");
            if (h.Revision != expectedRevision || h.DayIndex != expectedDayIndex)
                throw new InvalidOperationException("Stale EndDay revision/day.");
            if (current.Events.CaptureDueStage(expectedDayIndex).Count != 0 ||
                current.Payables.CaptureDueStage(expectedDayIndex).Count != 0)
                throw new InvalidOperationException("Due effects or payables require settlement handler.");
            if ((expectedDayIndex + 1) % 5 == 0)
                throw new InvalidOperationException("Weekly settlement handler not integrated.");
            string key = "end-day:" + h.CampaignId + ":" +
                expectedDayIndex.ToString(CultureInfo.InvariantCulture);
            foreach (var processed in h.ProcessedDayKeys)
                if (StringComparer.Ordinal.Equals(processed, key))
                    throw new InvalidOperationException("Day already processed.");
            var keys = new List<string>(h.ProcessedDayKeys);
            keys.Add(key);
            var next = new GameStateSnapshotDto(
                checked(h.Revision + 1), h.CampaignId, h.CampaignSeed,
                h.DifficultyId, h.ContentVersion, checked(h.DayIndex + 1),
                false, h.Metrics, h.OpeningBalanceKopecks, h.BalanceKopecks,
                h.RandomStreams, h.ProcessedCommandIds, keys,
                h.PendingEventIds, h.CompletedEventIds);
            return new DurableGameStateSnapshot(
                new DurableFinanceSnapshot(next, current.Finance.Ledger, current.Finance.Receipts),
                current.Events, current.Payables);
        }
    }
}
