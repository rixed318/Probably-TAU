using System;
using System.Collections.Generic;
using System.Globalization;

namespace TAU.President.Core
{
    /// <summary>Fail-closed first EndDay transaction boundary; caller publishes result atomically.</summary>
    public static class DurableDayAdvance
    {
        /// <summary>Single Core-owned EndDay entrypoint for days without due events
        /// or weekly settlement. Selects mandatory-payment settlement when due,
        /// otherwise the quiet path. Both return detached snapshots.</summary>
        public static DurableGameStateSnapshot PrepareDay(
            DurableGameStateSnapshot current, long expectedRevision, int expectedDayIndex)
        {
            if (current == null) throw new ArgumentNullException(nameof(current));
            // Check caller expectations before inspecting due work; no state is mutated.
            if (current.Header.IsFinished ||
                current.Header.Revision != expectedRevision ||
                current.Header.DayIndex != expectedDayIndex)
                throw new InvalidOperationException("Stale or finished EndDay.");
            if (current.Events.CaptureDueStage(expectedDayIndex).Count != 0 ||
                (expectedDayIndex + 1) % 5 == 0)
                throw new InvalidOperationException("Event or weekly settlement handler required.");
            return current.Payables.CaptureDueStage(expectedDayIndex).Count == 0
                ? PrepareQuietDay(current, expectedRevision, expectedDayIndex)
                : PreparePayablesDay(current, expectedRevision, expectedDayIndex);
        }

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
            {
                // Even a zero-payment EndDay is a committed command with a durable receipt.
                // This preserves replay provenance when no payable is affordable.
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
            var commandIds = new List<string>(h.ProcessedCommandIds);
            commandIds.Add(key);
            var receipts = new List<DurableCommandReceipt>(current.Finance.Receipts);
            receipts.Add(new DurableCommandReceipt(key, "end-day-quiet-v1",
                checked(h.Revision + 1), new string[0]));
            var next = new GameStateSnapshotDto(
                checked(h.Revision + 1), h.CampaignId, h.CampaignSeed,
                h.DifficultyId, h.ContentVersion, checked(h.DayIndex + 1),
                false, h.Metrics, h.OpeningBalanceKopecks, h.BalanceKopecks,
                h.RandomStreams, commandIds, keys,
                h.PendingEventIds, h.CompletedEventIds);
            return new DurableGameStateSnapshot(
                new DurableFinanceSnapshot(next, current.Finance.Ledger, receipts),
                current.Events, current.Payables);
        }
    }
}
