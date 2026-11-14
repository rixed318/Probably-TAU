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
            Throws<OverflowException>(() => new LedgerSnapshot(0, new[] {
                new LedgerEntry("negative-overflow", "a", long.MinValue),
                new LedgerEntry("overflow-again", "a", -1)
            }));
            Throws<OverflowException>(() => new LedgerSnapshot(long.MaxValue,
                new LedgerEntry[0]).WithPosting(new LedgerEntry("overflow-append", "a", 1)));
            Throws<ArgumentOutOfRangeException>(() => new LedgerSnapshot(-1, new LedgerEntry[0]));
            Throws<ArgumentOutOfRangeException>(() => new LedgerEntry("zero", "a", 0));
            Throws<ArgumentException>(() => new LedgerEntry(" ", "a", 1));
            Throws<ArgumentNullException>(() => initial.WithPosting(null));
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
