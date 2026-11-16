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
            ScheduledEventQueueRoundTrip();
            DurableCompositionRoundTrip();
            DurablePayablesRoundTrip();
            QuietDayAdvanceGolden();
            AffordablePayablesGolden();
            PayablesDayAdvanceGolden();
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

        private static void ScheduledEventQueueRoundTrip()
        {
            var input = new List<ScheduledEventOccurrence> {
                new ScheduledEventOccurrence("z", "event.z", 2, 0),
                new ScheduledEventOccurrence("b", "event.b", 0, 1),
                new ScheduledEventOccurrence("a", "event.a", 0, 1),
                new ScheduledEventOccurrence("high", "event.high", 0, 3),
                new ScheduledEventOccurrence("later", "event.later", 1, int.MinValue)
            };
            var queue = new ScheduledEventQueueSnapshot(input, new[] { "finished" });
            input.Clear();
            var due = queue.CaptureDueStage(0);
            Assert(due.Count == 3 && due[0].InstanceId == "high" &&
                due[1].InstanceId == "a" && due[2].InstanceId == "b");
            Assert(queue.Restore().CaptureDueStage(2).Count == 5);
            Assert(queue.Pending.Count == 5 && queue.CompletedIds[0] == "finished");
            Throws<NotSupportedException>(() => ((IList<ScheduledEventOccurrence>)due).Add(
                new ScheduledEventOccurrence("new", "event.new", 3, 0)));
            Throws<ArgumentException>(() => new ScheduledEventQueueSnapshot(
                new[] { new ScheduledEventOccurrence("finished", "event.x", 1, 0) },
                new[] { "finished" }));
            Throws<ArgumentOutOfRangeException>(() =>
                ScheduledEventQueueSnapshot.ValidateChildDueDay(0, 0));
            ScheduledEventQueueSnapshot.ValidateChildDueDay(0, 1);
        }

        private static void DurableCompositionRoundTrip()
        {
            var header = new GameStateSnapshotDto(1, "campaign", 42UL, "normal", "dayone", 0, false,
                new AcademyMetricsDto(50, 50, 50, 50, 50), 10000L, 9000L,
                new RandomStreamSnapshotDto[0], new[] { "cmd-1" }, new string[0],
                new[] { "due-1" }, new[] { "done-1" });
            var finance = new DurableFinanceSnapshot(header,
                new LedgerSnapshot(10000L, new[] { new LedgerEntry("post-1", "cmd-1", -1000L) }),
                new[] { new DurableCommandReceipt("cmd-1", "fingerprint", 1, new[] { "post-1" }) });
            var events = new ScheduledEventQueueSnapshot(
                new[] { new ScheduledEventOccurrence("due-1", "event.x", 2, 5) },
                new[] { "done-1" });
            var restored = new DurableGameStateSnapshot(finance, events).Restore();
            Assert(restored.Header.DayIndex == 0 && restored.Finance.Ledger.CurrentBalanceKopecks == 9000L);
            Assert(restored.Events.Pending[0].InstanceId == "due-1");
            Assert(restored.Events.CompletedIds[0] == "done-1");
            Throws<ArgumentException>(() => new DurableGameStateSnapshot(finance,
                new ScheduledEventQueueSnapshot(
                    new[] { new ScheduledEventOccurrence("wrong", "event.x", 2, 5) },
                    new[] { "done-1" })));
            Throws<ArgumentException>(() => new DurableGameStateSnapshot(finance,
                new ScheduledEventQueueSnapshot(
                    new[] { new ScheduledEventOccurrence("due-1", "event.x", 2, 5) },
                    new[] { "wrong" })));
        }

        private static void DurablePayablesRoundTrip()
        {
            var input = new List<DurablePayable> {
                new DurablePayable("due-z", 3, 1200L),
                new DurablePayable("due-b", 1, 500L),
                new DurablePayable("due-a", 1, 700L)
            };
            var payables = new DurablePayablesSnapshot(input);
            input.Clear();
            var restored = payables.Restore();
            Assert(restored.Outstanding.Count == 3);
            Assert(restored.Outstanding[0].DueKey == "due-a" &&
                restored.Outstanding[1].DueKey == "due-b" &&
                restored.Outstanding[2].DueKey == "due-z");
            Assert(restored.Outstanding[0].AmountKopecks == 700L);
            var dueDayOne = restored.CaptureDueStage(1);
            Assert(dueDayOne.Count == 2 && dueDayOne[0].DueKey == "due-a" &&
                dueDayOne[1].DueKey == "due-b");
            Assert(restored.CaptureDueStage(0).Count == 0);
            Assert(restored.CaptureDueStage(3).Count == 3);
            Throws<NotSupportedException>(() => ((IList<DurablePayable>)dueDayOne).Clear());
            Throws<ArgumentOutOfRangeException>(() => restored.CaptureDueStage(-1));
            Assert(restored.Outstanding.Count == 3);
            Throws<ArgumentException>(() => new DurablePayablesSnapshot(new[] {
                new DurablePayable("duplicate", 1, 100L),
                new DurablePayable("duplicate", 2, 200L)
            }));
            Throws<ArgumentOutOfRangeException>(() => new DurablePayable("zero", 1, 0L));
            Throws<NotSupportedException>(() =>
                ((IList<DurablePayable>)restored.Outstanding).Add(
                    new DurablePayable("extra", 4, 1L)));

            var header = new GameStateSnapshotDto(0, "campaign", 42UL, "normal", "dayone", 0, false,
                new AcademyMetricsDto(50, 50, 50, 50, 50), 10000L, 10000L,
                new RandomStreamSnapshotDto[0], new string[0], new string[0],
                new string[0], new string[0]);
            var composed = new DurableGameStateSnapshot(
                new DurableFinanceSnapshot(header, new LedgerSnapshot(10000L, new LedgerEntry[0]),
                    new DurableCommandReceipt[0]),
                new ScheduledEventQueueSnapshot(new ScheduledEventOccurrence[0], new string[0]),
                payables).Restore();
            Assert(composed.Payables.Outstanding.Count == 3 &&
                composed.Payables.Outstanding[2].DueKey == "due-z");
            Assert(composed.Finance.Ledger.CurrentBalanceKopecks == 10000L);
        }

        private static void QuietDayAdvanceGolden()
        {
            var header = new GameStateSnapshotDto(0, "campaign", 42UL, "normal", "dayone", 0, false,
                new AcademyMetricsDto(50, 50, 50, 50, 50), 10000L, 10000L,
                new RandomStreamSnapshotDto[0], new string[0], new string[0],
                new string[0], new string[0]);
            var finance = new DurableFinanceSnapshot(header,
                new LedgerSnapshot(10000L, new LedgerEntry[0]), new DurableCommandReceipt[0]);
            var quiet = new DurableGameStateSnapshot(finance,
                new ScheduledEventQueueSnapshot(new ScheduledEventOccurrence[0], new string[0]));
            var next = DurableDayAdvance.PrepareQuietDay(quiet, 0, 0);
            Assert(quiet.Header.DayIndex == 0 && quiet.Header.Revision == 0);
            Assert(next.Header.DayIndex == 1 && next.Header.Revision == 1);
            Assert(next.Header.ProcessedDayKeys.Count == 1 &&
                next.Header.ProcessedDayKeys[0] == "end-day:campaign:0");
            Assert(next.Restore().Finance.Ledger.CurrentBalanceKopecks == 10000L);
            Throws<InvalidOperationException>(() => DurableDayAdvance.PrepareQuietDay(quiet, 1, 0));
            Throws<InvalidOperationException>(() => DurableDayAdvance.PrepareQuietDay(next, 0, 0));
            var payable = new DurableGameStateSnapshot(finance,
                new ScheduledEventQueueSnapshot(new ScheduledEventOccurrence[0], new string[0]),
                new DurablePayablesSnapshot(new[] { new DurablePayable("rent", 0, 200L) }));
            Throws<InvalidOperationException>(() => DurableDayAdvance.PrepareQuietDay(payable, 0, 0));
            Assert(payable.Header.DayIndex == 0 && payable.Payables.Outstanding.Count == 1);
            var weeklyHeader = new GameStateSnapshotDto(0, "campaign", 42UL, "normal", "dayone", 4, false,
                header.Metrics, 10000L, 10000L, new RandomStreamSnapshotDto[0],
                new string[0], new string[0], new string[0], new string[0]);
            var weekly = new DurableGameStateSnapshot(
                new DurableFinanceSnapshot(weeklyHeader, finance.Ledger, finance.Receipts),
                new ScheduledEventQueueSnapshot(new ScheduledEventOccurrence[0], new string[0]));
            Throws<InvalidOperationException>(() => DurableDayAdvance.PrepareQuietDay(weekly, 0, 4));
        }

        private static void AffordablePayablesGolden()
        {
            var payables = new DurablePayablesSnapshot(new[] {
                new DurablePayable("a", 0, 800L),
                new DurablePayable("b", 0, 300L),
                new DurablePayable("c", 0, 200L),
                new DurablePayable("later", 1, 50L)
            });
            var selected = payables.CaptureAffordableDueStage(0, 500L);
            Assert(selected.Count == 2 && selected[0].DueKey == "b" &&
                selected[1].DueKey == "c");
            Assert(payables.Outstanding.Count == 4);
            Assert(payables.CaptureAffordableDueStage(0, 0L).Count == 0);
            Throws<NotSupportedException>(() => ((IList<DurablePayable>)selected).Clear());
            Throws<ArgumentOutOfRangeException>(() => payables.CaptureAffordableDueStage(0, -1L));
            Throws<ArgumentOutOfRangeException>(() => payables.CaptureAffordableDueStage(-1, 500L));
        }


        private static void PayablesDayAdvanceGolden()
        {
            var header = new GameStateSnapshotDto(1, "campaign", 42UL, "normal", "dayone", 0, false,
                new AcademyMetricsDto(50, 50, 50, 50, 50), 1000L, 500L,
                new RandomStreamSnapshotDto[0], new[] { "setup" }, new string[0],
                new string[0], new string[0]);
            var ledger = new LedgerSnapshot(1000L,
                new[] { new LedgerEntry("setup-post", "setup", -500L) });
            var finance = new DurableFinanceSnapshot(header, ledger,
                new[] { new DurableCommandReceipt("setup", "setup-fingerprint", 1,
                    new[] { "setup-post" }) });
            // Revision must be at least that of existing receipts.
            var state = new DurableGameStateSnapshot(finance,
                new ScheduledEventQueueSnapshot(new ScheduledEventOccurrence[0], new string[0]),
                new DurablePayablesSnapshot(new[] {
                    new DurablePayable("a", 0, 800L),
                    new DurablePayable("b", 0, 300L),
                    new DurablePayable("c", 0, 200L),
                    new DurablePayable("future", 2, 50L)
                }));
            var next = DurableDayAdvance.PreparePayablesDay(state, 1, 0);
            Assert(state.Header.DayIndex == 0 && state.Finance.Ledger.Entries.Count == 1);
            Assert(next.Header.DayIndex == 1 && next.Header.BalanceKopecks == 0L);
            Assert(next.Finance.Ledger.Entries.Count == 3);
            Assert(next.Finance.Receipts.Count == 2);
            Assert(next.Payables.Outstanding.Count == 2 &&
                next.Payables.Outstanding[0].DueKey == "a" &&
                next.Payables.Outstanding[1].DueKey == "future");
            Assert(next.Restore().Header.BalanceKopecks == 0L);
            Throws<InvalidOperationException>(() => DurableDayAdvance.PreparePayablesDay(next, 1, 0));
            Throws<InvalidOperationException>(() => DurableDayAdvance.PreparePayablesDay(state, 0, 0));
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
