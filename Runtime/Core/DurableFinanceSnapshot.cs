using System;
using System.Collections.Generic;

namespace TAU.President.Core
{
    /// <summary>Durable, Core-owned financial slice; no IO and no replay of postings on restore.</summary>
    public sealed class DurableFinanceSnapshot
    {
        private readonly List<DurableCommandReceipt> receipts;
        public GameStateSnapshotDto Header { get; private set; }
        public LedgerSnapshot Ledger { get; private set; }
        public IReadOnlyList<DurableCommandReceipt> Receipts { get { return receipts.AsReadOnly(); } }

        public DurableFinanceSnapshot(GameStateSnapshotDto header, LedgerSnapshot ledger,
            IEnumerable<DurableCommandReceipt> receipts)
        {
            if (header == null) throw new ArgumentNullException(nameof(header));
            if (ledger == null) throw new ArgumentNullException(nameof(ledger));
            if (receipts == null) throw new ArgumentNullException(nameof(receipts));
            if (header.OpeningBalanceKopecks != ledger.OpeningBalanceKopecks ||
                header.BalanceKopecks != ledger.CurrentBalanceKopecks)
                throw new ArgumentException("Snapshot cash and ordered ledger disagree.");
            var commandIds = new HashSet<string>(header.ProcessedCommandIds, StringComparer.Ordinal);
            var seenCommands = new HashSet<string>(StringComparer.Ordinal);
            var seenRevisions = new HashSet<long>();
            var ledgerKeys = new HashSet<string>(StringComparer.Ordinal);
            foreach (var entry in ledger.Entries) ledgerKeys.Add(entry.PostingId);
            var usedKeys = new HashSet<string>(StringComparer.Ordinal);
            this.receipts = new List<DurableCommandReceipt>();
            foreach (var receipt in receipts)
            {
                if (receipt == null) throw new ArgumentException("Null receipt.", nameof(receipts));
                if (!commandIds.Contains(receipt.CommandId) || !seenCommands.Add(receipt.CommandId) ||
                    receipt.CommittedRevision > header.Revision ||
                    !seenRevisions.Add(receipt.CommittedRevision))
                    throw new ArgumentException("Invalid command receipt identity/revision.", nameof(receipts));
                foreach (var key in receipt.PostingKeys)
                    if (!ledgerKeys.Contains(key) || !usedKeys.Add(key))
                        throw new ArgumentException("Missing or reused ledger posting key.", nameof(receipts));
                this.receipts.Add(receipt);
            }
            if (seenCommands.Count != commandIds.Count || usedKeys.Count != ledgerKeys.Count)
                throw new ArgumentException("Receipts must cover every command and ledger posting.", nameof(receipts));
            this.receipts.Sort((a, b) =>
            {
                int order = a.CommittedRevision.CompareTo(b.CommittedRevision);
                return order != 0 ? order : StringComparer.Ordinal.Compare(a.CommandId, b.CommandId);
            });
            Header = header;
            Ledger = ledger;
        }

        /// <summary>Revalidates persisted records without executing commands or changing RNG.</summary>
        public DurableFinanceSnapshot Restore()
        {
            return new DurableFinanceSnapshot(Header,
                new LedgerSnapshot(Ledger.OpeningBalanceKopecks, Ledger.Entries), receipts);
        }
    }

    /// <summary>Committed outcome with canonical fingerprint and ordered posting keys.</summary>
    public sealed class DurableCommandReceipt
    {
        private readonly List<string> postingKeys;
        public string CommandId { get; private set; }
        public string Fingerprint { get; private set; }
        public string Outcome { get { return "Committed"; } }
        public long CommittedRevision { get; private set; }
        public IReadOnlyList<string> PostingKeys { get { return postingKeys.AsReadOnly(); } }

        public DurableCommandReceipt(string commandId, string fingerprint,
            long committedRevision, IEnumerable<string> postingKeys)
        {
            CommandId = GameStateSnapshotDto.Required(commandId, nameof(commandId));
            Fingerprint = GameStateSnapshotDto.Required(fingerprint, nameof(fingerprint));
            if (committedRevision < 1) throw new ArgumentOutOfRangeException(nameof(committedRevision));
            if (postingKeys == null) throw new ArgumentNullException(nameof(postingKeys));
            CommittedRevision = committedRevision;
            this.postingKeys = new List<string>();
            var seen = new HashSet<string>(StringComparer.Ordinal);
            foreach (var key in postingKeys)
            {
                var valid = GameStateSnapshotDto.Required(key, nameof(postingKeys));
                if (!seen.Add(valid)) throw new ArgumentException("Duplicate posting key.", nameof(postingKeys));
                this.postingKeys.Add(valid);
            }
        }
    }
}
