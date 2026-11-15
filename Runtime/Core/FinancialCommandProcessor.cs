using System;
using System.Collections.Generic;

namespace TAU.President.Core
{
    /// <summary>Immutable financial-only transaction state; not the complete GameState.</summary>
    public sealed class FinancialCommandState
    {
        private readonly Dictionary<string, FinancialCommandReceipt> receipts;
        public LedgerSnapshot Ledger { get; private set; }
        public long Revision { get; private set; }
        public int DayIndex { get; private set; }
        public int ReceiptCount { get { return receipts.Count; } }

        public FinancialCommandState(LedgerSnapshot ledger, long revision, int dayIndex)
            : this(ledger, revision, dayIndex, new Dictionary<string, FinancialCommandReceipt>(StringComparer.Ordinal)) { }

        private FinancialCommandState(LedgerSnapshot ledger, long revision, int dayIndex,
            Dictionary<string, FinancialCommandReceipt> receipts)
        {
            if (ledger == null) throw new ArgumentNullException(nameof(ledger));
            if (revision < 0) throw new ArgumentOutOfRangeException(nameof(revision));
            if (dayIndex < 0 || dayIndex == int.MaxValue) throw new ArgumentOutOfRangeException(nameof(dayIndex));
            Ledger = ledger;
            Revision = revision;
            DayIndex = dayIndex;
            this.receipts = new Dictionary<string, FinancialCommandReceipt>(receipts, StringComparer.Ordinal);
        }

        internal bool TryReceipt(string commandId, out FinancialCommandReceipt receipt)
        {
            return receipts.TryGetValue(commandId, out receipt);
        }

        internal FinancialCommandState WithCommit(LedgerSnapshot nextLedger, FinancialCommandReceipt receipt)
        {
            var nextReceipts = new Dictionary<string, FinancialCommandReceipt>(receipts, StringComparer.Ordinal);
            nextReceipts.Add(receipt.CommandId, receipt);
            return new FinancialCommandState(nextLedger, checked(Revision + 1), DayIndex, nextReceipts);
        }
    }

    public sealed class FinancialPostingCommand
    {
        private readonly List<LedgerEntry> postings;
        public string CommandId { get; private set; }
        public string Fingerprint { get; private set; }
        public long ExpectedRevision { get; private set; }
        public int ExpectedDayIndex { get; private set; }
        public IReadOnlyList<LedgerEntry> Postings { get { return postings.AsReadOnly(); } }

        /// <summary>Fingerprint must be canonicalized/verified by the future command boundary.</summary>
        public FinancialPostingCommand(string commandId, string fingerprint,
            long expectedRevision, int expectedDayIndex, IEnumerable<LedgerEntry> postings)
        {
            CommandId = GameStateSnapshotDto.Required(commandId, nameof(commandId));
            Fingerprint = GameStateSnapshotDto.Required(fingerprint, nameof(fingerprint));
            if (expectedRevision < 0) throw new ArgumentOutOfRangeException(nameof(expectedRevision));
            if (expectedDayIndex < 0) throw new ArgumentOutOfRangeException(nameof(expectedDayIndex));
            if (postings == null) throw new ArgumentNullException(nameof(postings));
            this.postings = new List<LedgerEntry>();
            foreach (var entry in postings)
            {
                if (entry == null) throw new ArgumentException("Null posting.", nameof(postings));
                this.postings.Add(entry);
            }
            ExpectedRevision = expectedRevision;
            ExpectedDayIndex = expectedDayIndex;
        }
    }

    public sealed class FinancialCommandReceipt
    {
        public string CommandId { get; private set; }
        public string Fingerprint { get; private set; }
        public long CommittedRevision { get; private set; }
        public long BalanceKopecks { get; private set; }

        internal FinancialCommandReceipt(string commandId, string fingerprint, long revision, long balance)
        {
            CommandId = commandId;
            Fingerprint = fingerprint;
            CommittedRevision = revision;
            BalanceKopecks = balance;
        }
    }

    public enum FinancialCommandStatus
    {
        Committed, Replayed, CommandIdCollision, StaleState, Rejected
    }

    public sealed class FinancialCommandResult
    {
        public FinancialCommandState State { get; private set; }
        public FinancialCommandReceipt Receipt { get; private set; }
        public FinancialCommandStatus Status { get; private set; }
        public string Error { get; private set; }

        internal FinancialCommandResult(FinancialCommandState state, FinancialCommandReceipt receipt,
            FinancialCommandStatus status, string error)
        {
            State = state;
            Receipt = receipt;
            Status = status;
            Error = error;
        }
    }

    /// <summary>
    /// Pure prepare/commit prototype for voluntary financial postings only.
    /// Not yet a durable CommandProcessor, GameState writer or mandatory-payment policy.
    /// </summary>
    public static class FinancialCommandProcessor
    {
        public static FinancialCommandResult Execute(FinancialCommandState state, FinancialPostingCommand command)
        {
            if (state == null) throw new ArgumentNullException(nameof(state));
            if (command == null) throw new ArgumentNullException(nameof(command));

            FinancialCommandReceipt prior;
            if (state.TryReceipt(command.CommandId, out prior))
                return new FinancialCommandResult(state,
                    StringComparer.Ordinal.Equals(prior.Fingerprint, command.Fingerprint) ? prior : null,
                    StringComparer.Ordinal.Equals(prior.Fingerprint, command.Fingerprint)
                        ? FinancialCommandStatus.Replayed : FinancialCommandStatus.CommandIdCollision,
                    StringComparer.Ordinal.Equals(prior.Fingerprint, command.Fingerprint) ? null : "CommandIdCollision");

            if (state.Revision != command.ExpectedRevision || state.DayIndex != command.ExpectedDayIndex)
                return new FinancialCommandResult(state, null, FinancialCommandStatus.StaleState, "StaleState");

            // No external state is modified until all checks and the new receipt are prepared.
            try
            {
                LedgerSnapshot next = state.Ledger.WithPostings(command.Postings);
                long revision = checked(state.Revision + 1);
                var receipt = new FinancialCommandReceipt(command.CommandId, command.Fingerprint,
                    revision, next.CurrentBalanceKopecks);
                var committed = state.WithCommit(next, receipt);
                return new FinancialCommandResult(committed, receipt, FinancialCommandStatus.Committed, null);
            }
            catch (Exception ex) when (ex is ArgumentException || ex is InvalidOperationException || ex is OverflowException)
            {
                return new FinancialCommandResult(state, null, FinancialCommandStatus.Rejected, ex.GetType().Name);
            }
        }
    }
}
