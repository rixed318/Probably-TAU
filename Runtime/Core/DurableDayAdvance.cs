using System;
using System.Collections.Generic;
using System.Globalization;

namespace TAU.President.Core
{
    /// <summary>Fail-closed first EndDay transaction boundary; caller publishes result atomically.</summary>
    public static class DurableDayAdvance
    {

        /// <summary>Prepare a single atomic day close with affordable mandatory payments.
        /// Unaffordable liabilities remain outstanding; events and weekly closes fail closed.</summary>
        public static DurableGameStateSnapshot PreparePayablesDay(
            DurableGameStateSnapshot current, long expectedRevision, int expectedDayIndex)
        {
            if (current == null) throw new ArgumentNullException(nameof(current));
            var h = current.Header;
            if (h.IsFinished || h.Revision != expectedRevision || h.DayIndex != expectedDayIndex)
                throw new InvalidOperationException("Stale or finished EndDay.");
            if ((expectedDayIndex + 1) % 5 == 0 ||
                current.Events.CaptureDueStage(expectedDayIndex).Count != 0)
                throw new InvalidOperationException("Event or weekly settlement handler required.");
            string dayKey = "end-day:" + h.CampaignId + ":" +
                expectedDayIndex.ToString(CultureInfo.InvariantCulture);
            foreach (var key in h.ProcessedDayKeys)
                if (StringComparer.Ordinal.Equals(key, dayKey))
                    throw new InvalidOperationException("Day already processed.");

            var selected = current.Payables.CaptureAffordableDueStage(
                expectedDayIndex, h.BalanceKopecks);
            var postings = new List<LedgerEntry>();
            var postingKeys = new List<string>();
            var settled = new HashSet<string>(StringComparer.Ordinal);
            foreach (var payable in selected)
            {
                string postingKey = "payable:" + payable.DueKey;
                postings.Add(new LedgerEntry(postingKey, dayKey, checked(-payable.AmountKopecks)));
                postingKeys.Add(postingKey);
                settled.Add(payable.DueKey);
            }
            var nextLedger = current.Finance.Ledger.WithPostings(postings);
            var remaining = new List<DurablePayable>();
            foreach (var payable in current.Payables.Outstanding)
                if (!settled.Contains(payable.DueKey)) remaining.Add(payable);
            var commandIds = new List<string>(h.ProcessedCommandIds);
            var receipts = new List<DurableCommandReceipt>(current.Finance.Receipts);
            if (selected.Count != 0)
            {
                commandIds.Add(dayKey);
                // Canonical length-prefixed due keys prevent ambiguous fingerprint inputs.
                var fingerprint = new System.Text.StringBuilder("end-day-payables-v1:");
                foreach (var payable in selected)
                    fingerprint.Append(payable.DueKey.Length.ToString(CultureInfo.InvariantCulture))
                        .Append(':').Append(payable.DueKey).Append(':')
                        .Append(payable.AmountKopecks.ToString(CultureInfo.InvariantCulture)).Append(';');
                receipts.Add(new DurableCommandReceipt(dayKey, fingerprint.ToString(),
                    checked(h.Revision + 1), postingKeys));
            }
            var days = new List<string>(h.ProcessedDayKeys);
            days.Add(dayKey);
            var next = new GameStateSnapshotDto(
                checked(h.Revision + 1), h.CampaignId, h.CampaignSeed,
                h.DifficultyId, h.ContentVersion, checked(h.DayIndex + 1),
                false, h.Metrics, h.OpeningBalanceKopecks,
                nextLedger.CurrentBalanceKopecks, h.RandomStreams,
                commandIds, days, h.PendingEventIds, h.CompletedEventIds);
            return new DurableGameStateSnapshot(
                new DurableFinanceSnapshot(next, nextLedger, receipts),
                current.Events, new DurablePayablesSnapshot(remaining));
        }

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
