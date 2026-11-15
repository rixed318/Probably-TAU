using System;
using System.Collections.Generic;
using TAU.President.Core;

namespace TAU.President.Tests.Core
{
    /// <summary>Standalone deterministic checks; invoke RunAll from future NUnit wrapper.</summary>
    public static class GameStateSnapshotGoldenTests
    {
        public static void RunAll()
        {
            var rng = Pcg32.FromSeed(42UL, 54UL);
            var streams = new List<RandomStreamSnapshotDto> {
                new RandomStreamSnapshotDto("events", rng.State, rng.Increment)
            };
            var commands = new List<string> { "command-1" };
            var snapshot = Create(streams, commands, new string[] { "event-pending" },
                new string[] { "event-done" });
            Assert(snapshot.SchemaVersion == 1 && snapshot.DayIndex == 0);
            Assert(snapshot.Metrics.PublicReputation == 50);
            Assert(snapshot.BalanceKopecks == 9000L);
            Assert(snapshot.RandomStreams[0].Restore().NextUInt32() == rng.NextUInt32());
            streams.Clear();
            commands.Clear();
            Assert(snapshot.RandomStreams.Count == 1 && snapshot.ProcessedCommandIds.Count == 1);
            Throws<NotSupportedException>(() => ((IList<string>)snapshot.ProcessedCommandIds).Add("x"));
            Throws<ArgumentOutOfRangeException>(() => new AcademyMetricsDto(101, 0, 0, 0, 0));
            Throws<ArgumentOutOfRangeException>(() => CreateWithCash(-1));
            Throws<ArgumentException>(() => Create(
                new List<RandomStreamSnapshotDto> { streamsItem(), streamsItem() },
                new string[0], new string[0], new string[0]));
            Throws<ArgumentException>(() => Create(new RandomStreamSnapshotDto[0],
                new string[] { "same", "same" }, new string[0], new string[0]));
            Throws<ArgumentException>(() => Create(new RandomStreamSnapshotDto[0],
                new string[0], new string[] { "event" }, new string[] { "event" }));
            Throws<ArgumentException>(() => new RandomStreamSnapshotDto("events", 0UL, 2UL));
            DurableFinanceRoundTrip();
        }

        private static void DurableFinanceRoundTrip()
        {
            var header = new GameStateSnapshotDto(2, "campaign", 42UL, "normal", "dayone", 0, false,
                new AcademyMetricsDto(50, 50, 50, 50, 50), 10000L, 9000L,
                new RandomStreamSnapshotDto[0], new[] { "cmd-1" }, new string[0],
                new string[0], new string[0]);
            var ledger = new LedgerSnapshot(10000L,
                new[] { new LedgerEntry("post-1", "cmd-1", -1000L) });
            var receipt = new DurableCommandReceipt("cmd-1", "sha256-fingerprint", 1,
                new[] { "post-1" });
            var durable = new DurableFinanceSnapshot(header, ledger, new[] { receipt });
            var restored = durable.Restore();
            Assert(restored.Ledger.CurrentBalanceKopecks == 9000L);
            Assert(restored.Receipts.Count == 1 && restored.Receipts[0].PostingKeys[0] == "post-1");
            Assert(restored.Receipts[0].Fingerprint == "sha256-fingerprint");
            Throws<ArgumentException>(() => new DurableFinanceSnapshot(header, ledger,
                new[] { new DurableCommandReceipt("cmd-1", "sha256-fingerprint", 1, new[] { "missing" }) }));
            Throws<ArgumentException>(() => new DurableFinanceSnapshot(header, ledger,
                new DurableCommandReceipt[0]));
            // A valid posting key cannot be claimed by a different command receipt.
            var foreignHeader = new GameStateSnapshotDto(3, "campaign", 42UL, "normal", "dayone", 0, false,
                new AcademyMetricsDto(50, 50, 50, 50, 50), 10000L, 9000L,
                new RandomStreamSnapshotDto[0], new[] { "cmd-2" }, new string[0],
                new string[0], new string[0]);
            Throws<ArgumentException>(() => new DurableFinanceSnapshot(foreignHeader, ledger,
                new[] { new DurableCommandReceipt("cmd-2", "sha256-fingerprint", 2, new[] { "post-1" }) }));
        }

        private static RandomStreamSnapshotDto streamsItem()
        {
            return new RandomStreamSnapshotDto("same", 0UL, 1UL);
        }

        private static GameStateSnapshotDto CreateWithCash(long cash)
        {
            return new GameStateSnapshotDto(0, "campaign", 42UL, "normal", "dayone", 0, false,
                new AcademyMetricsDto(50, 50, 50, 50, 50), 10000L, cash,
                new RandomStreamSnapshotDto[0], new string[0], new string[0],
                new string[0], new string[0]);
        }

        private static GameStateSnapshotDto Create(
            IEnumerable<RandomStreamSnapshotDto> streams, IEnumerable<string> commands,
            IEnumerable<string> pending, IEnumerable<string> completed)
        {
            return new GameStateSnapshotDto(0, "campaign", 42UL, "normal", "dayone", 0, false,
                new AcademyMetricsDto(50, 50, 50, 50, 50), 10000L, 9000L,
                streams, commands, new string[0], pending, completed);
        }

        private static void Assert(bool condition)
        {
            if (!condition) throw new Exception("GameState snapshot assertion failed.");
        }

        private static void Throws<T>(Action action) where T : Exception
        {
            try { action(); }
            catch (T) { return; }
            throw new Exception("Expected " + typeof(T).Name);
        }
    }
}
