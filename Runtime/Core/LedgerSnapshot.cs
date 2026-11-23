using System;
using System.Collections.Generic;

namespace TAU.President.Core
{
    /// <summary>One committed financial movement; IDs are stable and unique per ledger.</summary>
    public sealed class LedgerEntry
    {
        public string PostingId { get; private set; }
        public string SourceId { get; private set; }
        public long DeltaKopecks { get; private set; }

        public LedgerEntry(string postingId, string sourceId, long deltaKopecks)
        {
            PostingId = GameStateSnapshotDto.Required(postingId, nameof(postingId));
            SourceId = GameStateSnapshotDto.Required(sourceId, nameof(sourceId));
            if (deltaKopecks == 0)
                throw new ArgumentOutOfRangeException(nameof(deltaKopecks), "Zero-value posting is not permitted.");
            DeltaKopecks = deltaKopecks;
        }
    }

    /// <summary>
    /// Immutable, append-only ledger slice for future Core command transactions.
    /// No IO, no direct GameState mutation, no payable/obligation policy in this slice.
    /// All cash arithmetic is checked Int64 kopecks.
    /// </summary>
    public sealed class LedgerSnapshot
    {
        private readonly List<LedgerEntry> entries;
        private readonly HashSet<string> postingIds;

        public long OpeningBalanceKopecks { get; private set; }
        public long CurrentBalanceKopecks { get; private set; }
        public IReadOnlyList<LedgerEntry> Entries { get { return entries.AsReadOnly(); } }

        public LedgerSnapshot(long openingBalanceKopecks, IEnumerable<LedgerEntry> entries)
        {
            if (openingBalanceKopecks < 0)
                throw new ArgumentOutOfRangeException(nameof(openingBalanceKopecks));
            if (entries == null) throw new ArgumentNullException(nameof(entries));

            OpeningBalanceKopecks = openingBalanceKopecks;
            this.entries = new List<LedgerEntry>();
            postingIds = new HashSet<string>(StringComparer.Ordinal);
            long balance = openingBalanceKopecks;
            foreach (var entry in entries)
            {
                if (entry == null) throw new ArgumentException("Null ledger entry.", nameof(entries));
                if (!postingIds.Add(entry.PostingId))
                    throw new ArgumentException("Duplicate posting ID: " + entry.PostingId, nameof(entries));
                balance = checked(balance + entry.DeltaKopecks);
                if (balance < 0)
                    throw new ArgumentException("Cash balance cannot become negative.", nameof(entries));
                this.entries.Add(entry);
            }
            CurrentBalanceKopecks = balance;
        }

        /// <summary>
        /// Prepares an independent ledger snapshot. Invalid/duplicate/overdraft entries
        /// throw without changing this instance; CommandProcessor will own atomic commit.
        /// </summary>
        public LedgerSnapshot WithPosting(LedgerEntry entry)
        {
            if (entry == null) throw new ArgumentNullException(nameof(entry));
            if (postingIds.Contains(entry.PostingId))
                throw new ArgumentException("Duplicate posting ID: " + entry.PostingId, nameof(entry));
            // Fail before allocation, including checked overflow and cash policy.
            long next = checked(CurrentBalanceKopecks + entry.DeltaKopecks);
            if (next < 0)
                throw new InvalidOperationException("Insufficient cash for posting.");
            var nextEntries = new List<LedgerEntry>(entries);
            nextEntries.Add(entry);
            return new LedgerSnapshot(OpeningBalanceKopecks, nextEntries);
        }
        /// <summary>
        /// Prepares a whole ordered batch against the current ledger. All entries are
        /// validated in order; any failure leaves this snapshot unchanged and publishes
        /// no partial result. CommandProcessor remains responsible for committing it.
        /// </summary>
        public LedgerSnapshot WithPostings(IEnumerable<LedgerEntry> postings)
        {
            if (postings == null) throw new ArgumentNullException(nameof(postings));

            var candidate = new List<LedgerEntry>(entries);
            var seen = new HashSet<string>(postingIds, StringComparer.Ordinal);
            long balance = CurrentBalanceKopecks;
            foreach (var entry in postings)
            {
                if (entry == null)
                    throw new ArgumentException("Null ledger entry.", nameof(postings));
                if (!seen.Add(entry.PostingId))
                    throw new ArgumentException("Duplicate posting ID: " + entry.PostingId, nameof(postings));
                balance = checked(balance + entry.DeltaKopecks);
                if (balance < 0)
                    throw new InvalidOperationException("Insufficient cash for posting.");
                candidate.Add(entry);
            }
            return new LedgerSnapshot(OpeningBalanceKopecks, candidate);
        }

    }
}
