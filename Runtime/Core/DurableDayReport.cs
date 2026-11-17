using System;
using System.Collections.Generic;
using System.Globalization;

namespace TAU.President.Core
{
    /// <summary>Immutable read model of one already-prepared EndDay transaction; no settlement policy.</summary>
    public sealed class DurableDayReport
    {
        private readonly List<LedgerEntry> postings;
        private readonly List<DurablePayable> unpaid;
        public int ClosedDayIndex { get; private set; }
        public long OpeningCashKopecks { get; private set; }
        public long ClosingCashKopecks { get; private set; }
        public string CommandId { get; private set; }
        public IReadOnlyList<LedgerEntry> Postings { get { return postings.AsReadOnly(); } }
        public IReadOnlyList<DurablePayable> UnpaidPayables { get { return unpaid.AsReadOnly(); } }

        private DurableDayReport(int day, long opening, long closing, string commandId,
            List<LedgerEntry> entries, List<DurablePayable> liabilities)
        {
            ClosedDayIndex = day;
            OpeningCashKopecks = opening;
            ClosingCashKopecks = closing;
            CommandId = commandId;
            postings = entries;
            unpaid = liabilities;
        }

        /// <summary>Verify and project a single EndDay transition without replaying effects.</summary>
        public static DurableDayReport Capture(DurableGameStateSnapshot before,
            DurableGameStateSnapshot after)
        {
            if (before == null) throw new ArgumentNullException(nameof(before));
            if (after == null) throw new ArgumentNullException(nameof(after));
            var old = before.Header;
            var next = after.Header;
            string commandId = "end-day:" + old.CampaignId + ":" +
                old.DayIndex.ToString(CultureInfo.InvariantCulture);
            if (!StringComparer.Ordinal.Equals(old.CampaignId, next.CampaignId) ||
                next.DayIndex != checked(old.DayIndex + 1) ||
                next.Revision != checked(old.Revision + 1) ||
                next.ProcessedDayKeys.Count != old.ProcessedDayKeys.Count + 1 ||
                !StringComparer.Ordinal.Equals(next.ProcessedDayKeys[next.ProcessedDayKeys.Count - 1], commandId))
                throw new InvalidOperationException("Not a single committed EndDay transition.");
            var previous = before.Finance.Ledger.Entries;
            var following = after.Finance.Ledger.Entries;
            if (following.Count < previous.Count)
                throw new InvalidOperationException("Ledger history was truncated.");
            for (int i = 0; i < previous.Count; i++)
                if (!StringComparer.Ordinal.Equals(previous[i].PostingId, following[i].PostingId) ||
                    !StringComparer.Ordinal.Equals(previous[i].SourceId, following[i].SourceId) ||
                    previous[i].DeltaKopecks != following[i].DeltaKopecks)
                    throw new InvalidOperationException("Ledger history changed.");
            DurableCommandReceipt receipt = null;
            foreach (var item in after.Finance.Receipts)
                if (StringComparer.Ordinal.Equals(item.CommandId, commandId)) receipt = item;
            if (receipt == null || receipt.CommittedRevision != next.Revision ||
                receipt.PostingKeys.Count != following.Count - previous.Count)
                throw new InvalidOperationException("EndDay receipt does not match ledger delta.");
            var entries = new List<LedgerEntry>();
            for (int i = previous.Count; i < following.Count; i++)
            {
                var entry = following[i];
                if (!StringComparer.Ordinal.Equals(entry.SourceId, commandId) ||
                    !StringComparer.Ordinal.Equals(entry.PostingId, receipt.PostingKeys[i - previous.Count]))
                    throw new InvalidOperationException("EndDay posting does not match receipt.");
                entries.Add(entry);
            }
            if (checked(old.BalanceKopecks + Sum(entries)) != next.BalanceKopecks)
                throw new InvalidOperationException("EndDay cash delta does not match postings.");
            return new DurableDayReport(old.DayIndex, old.BalanceKopecks, next.BalanceKopecks,
                commandId, entries, new List<DurablePayable>(after.Payables.Outstanding));
        }

        private static long Sum(IEnumerable<LedgerEntry> entries)
        {
            long total = 0;
            foreach (var entry in entries) total = checked(total + entry.DeltaKopecks);
            return total;
        }
    }
}
