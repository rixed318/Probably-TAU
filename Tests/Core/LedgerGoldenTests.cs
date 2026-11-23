using System;
using System.Collections.Generic;
using TAU.President.Core;

namespace TAU.President.Tests.Core
{
    public static class LedgerGoldenTests
    {
        public static void RunAll()
        {
            var input = new List<LedgerEntry> {
                new LedgerEntry("expense/1", "request/1", -2500),
                new LedgerEntry("income/1", "earned/1", 800)
            };
            var initial = new LedgerSnapshot(10000, input);
            Assert(initial.CurrentBalanceKopecks == 8300);
            Assert(initial.OpeningBalanceKopecks == 10000);
            input.Clear();
            Assert(initial.Entries.Count == 2); // defensive copy
            Throws<NotSupportedException>(() => ((IList<LedgerEntry>)initial.Entries).Add(
                new LedgerEntry("extra", "source", 1)));

            var posted = initial.WithPosting(new LedgerEntry("expense/2", "request/2", -300));
            Assert(posted.CurrentBalanceKopecks == 8000 && posted.Entries.Count == 3);
            Assert(initial.CurrentBalanceKopecks == 8300 && initial.Entries.Count == 2);
            Throws<ArgumentException>(() => initial.WithPosting(
                new LedgerEntry("expense/1", "different-source", 1)));
            Throws<InvalidOperationException>(() => initial.WithPosting(
                new LedgerEntry("overdraft", "request/3", -8301)));
            Assert(initial.Entries.Count == 2 && initial.CurrentBalanceKopecks == 8300);

            Throws<ArgumentException>(() => new LedgerSnapshot(10000, new[] {
                new LedgerEntry("same", "a", -1),
                new LedgerEntry("same", "b", 1)
            }));
            Throws<ArgumentException>(() => new LedgerSnapshot(1, new[] {
                new LedgerEntry("too-large", "a", -2)
            }));
            Throws<OverflowException>(() => new LedgerSnapshot(long.MaxValue, new[] {
                new LedgerEntry("overflow", "a", 1)
            }));
            Throws<OverflowException>(() => new LedgerSnapshot(long.MaxValue, new[] {
                new LedgerEntry("positive-overflow", "a", long.MaxValue)
            }));
            Throws<OverflowException>(() => new LedgerSnapshot(long.MaxValue,
                new LedgerEntry[0]).WithPosting(new LedgerEntry("overflow-append", "a", 1)));
            Throws<ArgumentOutOfRangeException>(() => new LedgerSnapshot(-1, new LedgerEntry[0]));
            Throws<ArgumentOutOfRangeException>(() => new LedgerEntry("zero", "a", 0));
            Throws<ArgumentException>(() => new LedgerEntry(" ", "a", 1));
            Throws<ArgumentNullException>(() => initial.WithPosting(null));
            
            // A prepared batch must be all-or-nothing and preserve posting order.
            var batch = initial.WithPostings(new[] {
                new LedgerEntry("income/batch", "earned/batch", 2000),
                new LedgerEntry("expense/batch", "request/batch", -1500)
            });
            Assert(batch.CurrentBalanceKopecks == 8800 && batch.Entries.Count == 4);
            Assert(batch.Entries[2].PostingId == "income/batch");
            Assert(batch.Entries[3].PostingId == "expense/batch");
            Assert(initial.CurrentBalanceKopecks == 8300 && initial.Entries.Count == 2);
            var emptyBatch = initial.WithPostings(new LedgerEntry[0]);
            Assert(emptyBatch.CurrentBalanceKopecks == initial.CurrentBalanceKopecks);
            Assert(emptyBatch.Entries.Count == initial.Entries.Count);
            Throws<ArgumentNullException>(() => initial.WithPostings(null));
            Throws<ArgumentException>(() => initial.WithPostings(new LedgerEntry[] { null }));
            Throws<ArgumentException>(() => initial.WithPostings(new[] {
                new LedgerEntry("new", "a", 1),
                new LedgerEntry("new", "b", 1)
            }));
            Throws<ArgumentException>(() => initial.WithPostings(new[] {
                new LedgerEntry("new", "a", 1),
                new LedgerEntry("income/1", "b", 1)
            }));
            Throws<InvalidOperationException>(() => initial.WithPostings(new[] {
                new LedgerEntry("income/temporary", "a", 100),
                new LedgerEntry("expense/too-much", "b", -8501)
            }));
            Throws<InvalidOperationException>(() => initial.WithPostings(new[] {
                new LedgerEntry("expense/first", "a", -8301),
                new LedgerEntry("income/too-late", "b", 10000)
            }));
            Throws<OverflowException>(() => new LedgerSnapshot(long.MaxValue,
                new LedgerEntry[0]).WithPostings(new[] {
                    new LedgerEntry("first", "a", -1),
                    new LedgerEntry("overflow", "b", 2)
                }));
            Assert(initial.CurrentBalanceKopecks == 8300 && initial.Entries.Count == 2);
            Assert(initial.WithPosting(new LedgerEntry("income/temporary", "a", 1))
                .CurrentBalanceKopecks == 8301);

        }

        private static void Assert(bool value)
        {
            if (!value) throw new Exception("Ledger invariant assertion failed.");
        }

        private static void Throws<T>(Action action) where T : Exception
        {
            try { action(); }
            catch (T) { return; }
            throw new Exception("Expected " + typeof(T).Name);
        }
    }
}
