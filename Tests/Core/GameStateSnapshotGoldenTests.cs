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
            RejectSettledOutstandingPayableGolden();
            QuietDayAdvanceGolden();
            AffordablePayablesGolden();
            PayablesDayAdvanceGolden();
            EndDayReportGolden();
            DocumentDecisionTransactionGolden();
            Day01FlowGolden();
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

        private static void RejectSettledOutstandingPayableGolden()
        {
            var header = new GameStateSnapshotDto(1, "campaign", 42UL, "normal", "dayone", 1, false,
                new AcademyMetricsDto(50, 50, 50, 50, 50), 1000L, 800L,
                new RandomStreamSnapshotDto[0], new[] { "end-day:campaign:0" },
                new[] { "end-day:campaign:0" }, new string[0], new string[0]);
            var ledger = new LedgerSnapshot(1000L,
                new[] { new LedgerEntry("payable:rent", "end-day:campaign:0", -200L) });
            var finance = new DurableFinanceSnapshot(header, ledger,
                new[] { new DurableCommandReceipt("end-day:campaign:0",
                    "end-day-payables-v1:4:rent:200;", 1, new[] { "payable:rent" }) });
            var events = new ScheduledEventQueueSnapshot(
                new ScheduledEventOccurrence[0], new string[0]);
            Throws<ArgumentException>(() => new DurableGameStateSnapshot(finance, events,
                new DurablePayablesSnapshot(new[] { new DurablePayable("rent", 0, 200L) })));
            var valid = new DurableGameStateSnapshot(finance, events,
                new DurablePayablesSnapshot(new[] { new DurablePayable("utilities", 0, 300L) }));
            Assert(valid.Restore().Payables.Outstanding.Count == 1);
            Assert(valid.Finance.Ledger.CurrentBalanceKopecks == 800L);
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
            var next = DurableDayAdvance.PrepareDay(quiet, 0, 0);
            Assert(quiet.Header.DayIndex == 0 && quiet.Header.Revision == 0);
            Assert(next.Header.DayIndex == 1 && next.Header.Revision == 1);
            Assert(next.Header.ProcessedDayKeys.Count == 1 &&
                next.Header.ProcessedDayKeys[0] == "end-day:campaign:0");
            Assert(next.Restore().Finance.Ledger.CurrentBalanceKopecks == 10000L);
            Assert(next.Header.ProcessedCommandIds.Count == 1 &&
                next.Header.ProcessedCommandIds[0] == "end-day:campaign:0");
            Assert(next.Finance.Receipts.Count == 1 &&
                next.Finance.Receipts[0].CommandId == "end-day:campaign:0" &&
                next.Finance.Receipts[0].PostingKeys.Count == 0 &&
                next.Finance.Receipts[0].Fingerprint == "end-day-quiet-v1");
            Assert(next.Restore().Finance.Receipts.Count == 1);
            Throws<InvalidOperationException>(() => DurableDayAdvance.PrepareQuietDay(quiet, 1, 0));
            Throws<InvalidOperationException>(() => DurableDayAdvance.PrepareQuietDay(next, 0, 0));
            var payable = new DurableGameStateSnapshot(finance,
                new ScheduledEventQueueSnapshot(new ScheduledEventOccurrence[0], new string[0]),
                new DurablePayablesSnapshot(new[] { new DurablePayable("rent", 0, 200L) }));
            Throws<InvalidOperationException>(() => DurableDayAdvance.PrepareQuietDay(payable, 0, 0));
            Assert(DurableDayAdvance.PrepareDay(payable, 0, 0).Finance.Ledger.CurrentBalanceKopecks == 9800L);
            Throws<InvalidOperationException>(() => DurableDayAdvance.PrepareDay(payable, 1, 0));
            Assert(payable.Header.DayIndex == 0 && payable.Payables.Outstanding.Count == 1);
            var weeklyHeader = new GameStateSnapshotDto(0, "campaign", 42UL, "normal", "dayone", 4, false,
                header.Metrics, 10000L, 10000L, new RandomStreamSnapshotDto[0],
                new string[0], new string[0], new string[0], new string[0]);
            var weekly = new DurableGameStateSnapshot(
                new DurableFinanceSnapshot(weeklyHeader, finance.Ledger, finance.Receipts),
                new ScheduledEventQueueSnapshot(new ScheduledEventOccurrence[0], new string[0]));
            Throws<InvalidOperationException>(() => DurableDayAdvance.PrepareQuietDay(weekly, 0, 4));
            Throws<InvalidOperationException>(() => DurableDayAdvance.PrepareDay(weekly, 0, 4));
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
            var next = DurableDayAdvance.PrepareDay(state, 1, 0);
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
            // No affordable payment must still leave a durable committed EndDay receipt.
            var noCashHeader = new GameStateSnapshotDto(1, "campaign", 42UL, "normal", "dayone", 0, false,
                header.Metrics, 1000L, 0L, new RandomStreamSnapshotDto[0],
                new[] { "setup" }, new string[0], new string[0], new string[0]);
            var noCashLedger = new LedgerSnapshot(1000L,
                new[] { new LedgerEntry("setup-post", "setup", -1000L) });
            var noCashFinance = new DurableFinanceSnapshot(noCashHeader, noCashLedger,
                new[] { new DurableCommandReceipt("setup", "setup-fingerprint", 1,
                    new[] { "setup-post" }) });
            var noCash = new DurableGameStateSnapshot(noCashFinance,
                new ScheduledEventQueueSnapshot(new ScheduledEventOccurrence[0], new string[0]),
                new DurablePayablesSnapshot(new[] { new DurablePayable("rent", 0, 100L) }));
            var noCashNext = DurableDayAdvance.PreparePayablesDay(noCash, 1, 0);
            Assert(noCashNext.Header.DayIndex == 1 && noCashNext.Header.BalanceKopecks == 0L);
            Assert(noCashNext.Finance.Receipts.Count == 2 &&
                noCashNext.Finance.Receipts[1].PostingKeys.Count == 0 &&
                noCashNext.Finance.Receipts[1].CommandId == "end-day:campaign:0");
            Assert(noCashNext.Payables.Outstanding.Count == 1);
            Assert(noCashNext.Restore().Finance.Receipts.Count == 2);
            Throws<InvalidOperationException>(() => DurableDayAdvance.PreparePayablesDay(noCashNext, 1, 0));
        }

        private static void EndDayReportGolden()
        {
            var header = new GameStateSnapshotDto(0, "campaign", 42UL, "normal", "dayone", 0, false,
                new AcademyMetricsDto(50, 50, 50, 50, 50), 1000L, 1000L,
                new RandomStreamSnapshotDto[0], new string[0], new string[0],
                new string[0], new string[0]);
            var before = new DurableGameStateSnapshot(
                new DurableFinanceSnapshot(header, new LedgerSnapshot(1000L, new LedgerEntry[0]),
                    new DurableCommandReceipt[0]),
                new ScheduledEventQueueSnapshot(new ScheduledEventOccurrence[0], new string[0]),
                new DurablePayablesSnapshot(new[] { new DurablePayable("rent", 0, 200L) }));
            var after = DurableDayAdvance.PrepareDay(before, 0, 0);
            var report = DurableDayReport.Capture(before, after.Restore());
            Assert(report.ClosedDayIndex == 0 && report.OpeningCashKopecks == 1000L &&
                report.ClosingCashKopecks == 800L && report.Postings.Count == 1 &&
                report.Postings[0].PostingId == "payable:rent" &&
                report.UnpaidPayables.Count == 0);
            Throws<InvalidOperationException>(() => DurableDayReport.Capture(before, before));
            var quietBefore = new DurableGameStateSnapshot(before.Finance, before.Events);
            var quietAfter = DurableDayAdvance.PrepareDay(quietBefore, 0, 0);
            var quietReport = DurableDayReport.Capture(quietBefore, quietAfter);
            Assert(quietReport.Postings.Count == 0 && quietReport.ClosingCashKopecks == 1000L);
        }


        private static void DocumentDecisionTransactionGolden()
        {
            var header = new GameStateSnapshotDto(0, "campaign", 42UL, "normal", "dayone", 0, false,
                new AcademyMetricsDto(50, 50, 50, 50, 50), 1000L, 1000L,
                new RandomStreamSnapshotDto[0], new string[0], new string[0],
                new string[0], new string[0]);
            var before = new DurableGameStateSnapshot(
                new DurableFinanceSnapshot(header, new LedgerSnapshot(1000L, new LedgerEntry[0]),
                    new DurableCommandReceipt[0]),
                new ScheduledEventQueueSnapshot(new ScheduledEventOccurrence[0], new string[0]));
            var effects = new DocumentDecisionEffects("day01.document-a.accept", -250L, 2, -1, 0, 1, 3);
            var after = DurableDocumentDecision.Prepare(before, "choice-001", 0, 0, effects);
            Assert(before.Header.Revision == 0 && before.Header.BalanceKopecks == 1000L);
            Assert(after.Header.Revision == 1 && after.Header.DayIndex == 0 &&
                after.Header.BalanceKopecks == 750L &&
                after.Header.Metrics.EducationQuality == 52 &&
                after.Header.Metrics.StudentSatisfaction == 49 &&
                after.Header.Metrics.PublicReputation == 53);
            Assert(after.Finance.Ledger.Entries.Count == 1 &&
                after.Finance.Ledger.Entries[0].PostingId == "decision:choice-001" &&
                after.Finance.Receipts[0].PostingKeys[0] == "decision:choice-001");
            Assert(after.Restore().Finance.Receipts[0].Fingerprint ==
                after.Finance.Receipts[0].Fingerprint);
            Throws<InvalidOperationException>(() =>
                DurableDocumentDecision.Prepare(after, "choice-001", 1, 0, effects));
            Throws<InvalidOperationException>(() =>
                DurableDocumentDecision.Prepare(before, "choice-002", 1, 0, effects));
            Throws<InvalidOperationException>(() =>
                DurableDocumentDecision.Prepare(before, "choice-003", 0, 0,
                    new DocumentDecisionEffects("too-expensive", -1001L, 0, 0, 0, 0, 0)));
            Throws<ArgumentOutOfRangeException>(() =>
                DurableDocumentDecision.Prepare(before, "choice-004", 0, 0,
                    new DocumentDecisionEffects("out-of-range", 0L, 51, 0, 0, 0, 0)));
            Assert(before.Finance.Ledger.Entries.Count == 0 &&
                before.Finance.Receipts.Count == 0);
            // Zero-cash decisions still advance revision and persist an idempotency receipt.
            // They must not create a zero-value ledger posting or alter the opening cash.
            var noCashEffects = new DocumentDecisionEffects("day01.document-b.inspect",
                0L, 0, 1, 0, 0, 0);
            var noCash = DurableDocumentDecision.Prepare(before, "choice-zero", 0, 0, noCashEffects);
            Assert(noCash.Header.Revision == 1 &&
                noCash.Header.BalanceKopecks == 1000L &&
                noCash.Header.Metrics.StudentSatisfaction == 51 &&
                noCash.Finance.Ledger.Entries.Count == 0 &&
                noCash.Finance.Receipts.Count == 1 &&
                noCash.Finance.Receipts[0].PostingKeys.Count == 0);
            var noCashRestored = noCash.Restore();
            Assert(noCashRestored.Finance.Receipts.Count == 1 &&
                noCashRestored.Header.ProcessedCommandIds.Count == 1);
            Throws<InvalidOperationException>(() =>
                DurableDocumentDecision.Prepare(noCashRestored, "choice-zero", 1, 0, noCashEffects));
            Assert(before.Header.Revision == 0 && before.Finance.Receipts.Count == 0);

            var next = DurableDayAdvance.PrepareDay(after, 1, 0);
            Assert(next.Header.DayIndex == 1 && next.Header.Revision == 2 &&
                next.Header.BalanceKopecks == 750L &&
                next.Finance.Receipts.Count == 2);
            Assert(DurableDayReport.Capture(after, next).Postings.Count == 0);
        }


        private static void Day01FlowGolden()
        {
            var header = new GameStateSnapshotDto(0, "campaign", 42UL, "normal", "dayone", 0, false,
                new AcademyMetricsDto(50, 50, 50, 50, 50), 1000L, 1000L,
                new RandomStreamSnapshotDto[0], new string[0], new string[0],
                new string[0], new string[0]);
            var before = new DurableGameStateSnapshot(
                new DurableFinanceSnapshot(header, new LedgerSnapshot(1000L, new LedgerEntry[0]),
                    new DurableCommandReceipt[0]),
                new ScheduledEventQueueSnapshot(new ScheduledEventOccurrence[0], new string[0]));
            var choices = new[] {
                new Day01DocumentChoice("doc-1", new DocumentDecisionEffects("a", -100L, 1, 0, 0, 0, 0)),
                new Day01DocumentChoice("doc-2", new DocumentDecisionEffects("b", -200L, 0, 2, 0, 0, 0)),
                new Day01DocumentChoice("doc-3", new DocumentDecisionEffects("c", 50L, 0, 0, 3, 0, 0))
            };
            var result = DurableDay01Flow.Prepare(before, 0, 0, choices);
            Assert(before.Header.Revision == 0 && before.Finance.Ledger.Entries.Count == 0);
            Assert(result.Next.Header.DayIndex == 1 && result.Next.Header.Revision == 4 &&
                result.Next.Header.BalanceKopecks == 750L &&
                result.Next.Header.Metrics.EducationQuality == 51 &&
                result.Next.Header.Metrics.StudentSatisfaction == 52 &&
                result.Next.Header.Metrics.StaffMorale == 53);
            Assert(result.Next.Finance.Ledger.Entries.Count == 3 &&
                result.Next.Finance.Receipts.Count == 4 &&
                result.Next.Finance.Ledger.Entries[0].PostingId == "decision:doc-1" &&
                result.Next.Finance.Ledger.Entries[2].PostingId == "decision:doc-3" &&
                result.Report.Postings.Count == 0);
            Assert(result.Next.Restore().Header.BalanceKopecks == 750L);
            Throws<InvalidOperationException>(() => DurableDay01Flow.Prepare(result.Next, 0, 0, choices));

            var unaffordable = new[] {
                choices[0], choices[1],
                new Day01DocumentChoice("doc-3", new DocumentDecisionEffects("c", -1000L, 0, 0, 0, 0, 0))
            };
            Throws<InvalidOperationException>(() => DurableDay01Flow.Prepare(before, 0, 0, unaffordable));
            Assert(before.Header.BalanceKopecks == 1000L &&
                before.Finance.Receipts.Count == 0 &&
                before.Finance.Ledger.Entries.Count == 0);
            var duplicate = new[] { choices[0], choices[1], choices[0] };
            Throws<ArgumentException>(() => DurableDay01Flow.Prepare(before, 0, 0, duplicate));
            // A different command ID cannot authorize a second choice for the same decision.
            var duplicateDecision = new[] {
                choices[0], choices[1],
                new Day01DocumentChoice("doc-other",
                    new DocumentDecisionEffects("a", 0L, 0, 0, 0, 0, 0))
            };
            Throws<ArgumentException>(() => DurableDay01Flow.Prepare(before, 0, 0, duplicateDecision));
            // No partial publication after a rejected third decision or duplicate ID.
            Assert(before.Header.Revision == 0 && before.Header.DayIndex == 0 &&
                before.Header.Metrics.EducationQuality == 50 &&
                before.Header.Metrics.StudentSatisfaction == 50 &&
                before.Header.Metrics.StaffMorale == 50 &&
                before.Finance.Ledger.Entries.Count == 0 &&
                before.Finance.Receipts.Count == 0);
            // Third-choice metric overflow must not publish the first two valid choices.
            var excessiveMetric = new[] {
                choices[0], choices[1],
                new Day01DocumentChoice("doc-3",
                    new DocumentDecisionEffects("c", 0L, 0, 0, 100, 0, 0))
            };
            Throws<ArgumentOutOfRangeException>(() =>
                DurableDay01Flow.Prepare(before, 0, 0, excessiveMetric));
            // EndDay command namespace is reserved, even for a document choice.
            var reservedCommand = new[] {
                choices[0], choices[1],
                new Day01DocumentChoice("end-day:campaign:0",
                    new DocumentDecisionEffects("c", 0L, 0, 0, 0, 0, 0))
            };
            Throws<ArgumentException>(() =>
                DurableDay01Flow.Prepare(before, 0, 0, reservedCommand));
            Assert(before.Header.Revision == 0 && before.Header.DayIndex == 0 &&
                before.Header.BalanceKopecks == 1000L &&
                before.Header.Metrics.StaffMorale == 50 &&
                before.Finance.Ledger.Entries.Count == 0 &&
                before.Finance.Receipts.Count == 0);
            // A due event at EndDay must fail closed after all three document choices.
            // The original durable snapshot remains untouched despite staged postings.
            var dueHeader = new GameStateSnapshotDto(0, "campaign", 42UL, "normal", "dayone", 0, false,
                new AcademyMetricsDto(50, 50, 50, 50, 50), 1000L, 1000L,
                new RandomStreamSnapshotDto[0], new string[0], new string[0],
                new[] { "event-due" }, new string[0]);
            var dueBefore = new DurableGameStateSnapshot(
                new DurableFinanceSnapshot(dueHeader, new LedgerSnapshot(1000L, new LedgerEntry[0]),
                    new DurableCommandReceipt[0]),
                new ScheduledEventQueueSnapshot(
                    new[] { new ScheduledEventOccurrence("event-due", "event.day01", 0, 1) },
                    new string[0]));
            Throws<InvalidOperationException>(() => DurableDay01Flow.Prepare(dueBefore, 0, 0, choices));
            Assert(dueBefore.Header.Revision == 0 && dueBefore.Header.DayIndex == 0 &&
                dueBefore.Header.BalanceKopecks == 1000L &&
                dueBefore.Finance.Ledger.Entries.Count == 0 &&
                dueBefore.Finance.Receipts.Count == 0 &&
                dueBefore.Events.Pending.Count == 1);
            // A due event must also prevent settlement of an otherwise affordable payable.
            // Neither staged document postings nor the payable may escape a failed EndDay.
            var dueWithPayable = new DurableGameStateSnapshot(
                dueBefore.Finance, dueBefore.Events,
                new DurablePayablesSnapshot(new[] {
                    new DurablePayable("day01-blocked-rent", 0, 200L)
                }));
            Throws<InvalidOperationException>(() =>
                DurableDay01Flow.Prepare(dueWithPayable, 0, 0, choices));
            Assert(dueWithPayable.Header.Revision == 0 &&
                dueWithPayable.Header.DayIndex == 0 &&
                dueWithPayable.Header.BalanceKopecks == 1000L &&
                dueWithPayable.Finance.Ledger.Entries.Count == 0 &&
                dueWithPayable.Finance.Receipts.Count == 0 &&
                dueWithPayable.Payables.Outstanding.Count == 1 &&
                dueWithPayable.Payables.Outstanding[0].DueKey == "day01-blocked-rent" &&
                dueWithPayable.Events.Pending.Count == 1 &&
                dueWithPayable.Events.CompletedIds.Count == 0);
            var dueWithPayableRestored = dueWithPayable.Restore();
            Assert(dueWithPayableRestored.Payables.Outstanding.Count == 1 &&
                dueWithPayableRestored.Events.Pending.Count == 1 &&
                dueWithPayableRestored.Finance.Ledger.Entries.Count == 0);
            // A future event must not block Day01: it stays queued through EndDay and Restore.
            // This complements the due-event fail-closed case without invoking NPC logic.
            var futureEventHeader = new GameStateSnapshotDto(0, "campaign", 42UL, "normal", "dayone", 0, false,
                new AcademyMetricsDto(50, 50, 50, 50, 50), 1000L, 1000L,
                new RandomStreamSnapshotDto[0], new string[0], new string[0],
                new[] { "event-tomorrow" }, new string[0]);
            var futureEventBefore = new DurableGameStateSnapshot(
                new DurableFinanceSnapshot(futureEventHeader, new LedgerSnapshot(1000L, new LedgerEntry[0]),
                    new DurableCommandReceipt[0]),
                new ScheduledEventQueueSnapshot(
                    new[] { new ScheduledEventOccurrence("event-tomorrow", "event.day02", 1, 1) },
                    new string[0]));
            var futureEventFlow = DurableDay01Flow.Prepare(futureEventBefore, 0, 0, choices);
            Assert(futureEventFlow.Next.Header.DayIndex == 1 &&
                futureEventFlow.Next.Header.BalanceKopecks == 750L &&
                futureEventFlow.Next.Events.Pending.Count == 1 &&
                futureEventFlow.Next.Events.Pending[0].InstanceId == "event-tomorrow" &&
                futureEventFlow.Next.Events.CompletedIds.Count == 0);
            var futureEventRestored = futureEventFlow.Next.Restore();
            Assert(futureEventRestored.Events.Pending.Count == 1 &&
                futureEventRestored.Events.Pending[0].DueDayIndex == 1 &&
                futureEventRestored.Events.CompletedIds.Count == 0);
            // The deferred event becomes due on DayIndex 1. A second EndDay must
            // fail closed until NPC/event handling consumes it; Restore cannot bypass
            // the due-event guard or publish a partial financial/day transition.
            Throws<InvalidOperationException>(() => DurableDayAdvance.PrepareDay(
                futureEventRestored, futureEventRestored.Header.Revision, 1));
            Assert(futureEventRestored.Header.DayIndex == 1 &&
                futureEventRestored.Header.Revision == 4 &&
                futureEventRestored.Header.BalanceKopecks == 750L &&
                futureEventRestored.Finance.Ledger.Entries.Count == 3 &&
                futureEventRestored.Finance.Receipts.Count == 4 &&
                futureEventRestored.Events.Pending.Count == 1 &&
                futureEventRestored.Events.CompletedIds.Count == 0);
            Assert(futureEventBefore.Header.DayIndex == 0 &&
                futureEventBefore.Finance.Ledger.Entries.Count == 0 &&
                futureEventBefore.Events.Pending.Count == 1);
            // A real payable due on Day01 must settle through the canonical EndDay,
            // after all three document choices, without mixing its posting into a choice receipt.
            var withPayable = new DurableGameStateSnapshot(before.Finance, before.Events,
                new DurablePayablesSnapshot(new[] { new DurablePayable("day01-rent", 0, 200L) }));
            var paidFlow = DurableDay01Flow.Prepare(withPayable, 0, 0, choices);
            Assert(paidFlow.Next.Header.DayIndex == 1 &&
                paidFlow.Next.Header.Revision == 4 &&
                paidFlow.Next.Header.BalanceKopecks == 550L &&
                paidFlow.Next.Finance.Ledger.Entries.Count == 4 &&
                paidFlow.Next.Finance.Ledger.Entries[3].PostingId == "payable:day01-rent" &&
                paidFlow.Next.Finance.Receipts.Count == 4 &&
                paidFlow.Next.Finance.Receipts[3].PostingKeys.Count == 1 &&
                paidFlow.Next.Finance.Receipts[3].PostingKeys[0] == "payable:day01-rent" &&
                paidFlow.Report.Postings.Count == 1 &&
                paidFlow.Report.Postings[0].PostingId == "payable:day01-rent" &&
                paidFlow.Report.UnpaidPayables.Count == 0);
            var paidRestored = paidFlow.Next.Restore();
            Assert(paidRestored.Header.BalanceKopecks == 550L &&
                paidRestored.Payables.Outstanding.Count == 0 &&
                paidRestored.Finance.Ledger.Entries.Count == 4);
            Assert(withPayable.Header.Revision == 0 &&
                withPayable.Payables.Outstanding.Count == 1 &&
                withPayable.Finance.Ledger.Entries.Count == 0);
            Throws<InvalidOperationException>(() => DurableDay01Flow.Prepare(paidRestored,
                paidRestored.Header.Revision, 0, choices));
            // A payable larger than the post-decision cash must remain outstanding.
            // EndDay still commits exactly once with an empty posting receipt.
            var withUnpaid = new DurableGameStateSnapshot(before.Finance, before.Events,
                new DurablePayablesSnapshot(new[] { new DurablePayable("day01-large-rent", 0, 800L) }));
            var unpaidFlow = DurableDay01Flow.Prepare(withUnpaid, 0, 0, choices);
            Assert(unpaidFlow.Next.Header.DayIndex == 1 &&
                unpaidFlow.Next.Header.Revision == 4 &&
                unpaidFlow.Next.Header.BalanceKopecks == 750L &&
                unpaidFlow.Next.Finance.Ledger.Entries.Count == 3 &&
                unpaidFlow.Next.Finance.Receipts.Count == 4 &&
                unpaidFlow.Next.Finance.Receipts[3].PostingKeys.Count == 0 &&
                unpaidFlow.Report.Postings.Count == 0 &&
                unpaidFlow.Report.UnpaidPayables.Count == 1 &&
                unpaidFlow.Report.UnpaidPayables[0].DueKey == "day01-large-rent");
            var unpaidRestored = unpaidFlow.Next.Restore();
            Assert(unpaidRestored.Payables.Outstanding.Count == 1 &&
                unpaidRestored.Payables.Outstanding[0].DueKey == "day01-large-rent" &&
                unpaidRestored.Header.BalanceKopecks == 750L);
            Throws<InvalidOperationException>(() => DurableDay01Flow.Prepare(
                unpaidRestored, unpaidRestored.Header.Revision, 0, choices));
            Assert(withUnpaid.Header.Revision == 0 &&
                withUnpaid.Payables.Outstanding.Count == 1 &&
                withUnpaid.Finance.Ledger.Entries.Count == 0);

            // Mixed due liabilities: pay only the affordable one, keep the other intact.
            // The selected posting and remaining debt must survive Restore without duplication.
            var mixedBefore = new DurableGameStateSnapshot(before.Finance, before.Events,
                new DurablePayablesSnapshot(new[] {
                    new DurablePayable("day01-small-rent", 0, 200L),
                    new DurablePayable("day01-large-fee", 0, 800L)
                }));
            var mixedFlow = DurableDay01Flow.Prepare(mixedBefore, 0, 0, choices);
            Assert(mixedFlow.Next.Header.DayIndex == 1 &&
                mixedFlow.Next.Header.Revision == 4 &&
                mixedFlow.Next.Header.BalanceKopecks == 550L &&
                mixedFlow.Next.Finance.Ledger.Entries.Count == 4 &&
                mixedFlow.Next.Finance.Ledger.Entries[3].PostingId == "payable:day01-small-rent" &&
                mixedFlow.Next.Finance.Receipts.Count == 4 &&
                mixedFlow.Next.Finance.Receipts[3].PostingKeys.Count == 1 &&
                mixedFlow.Next.Finance.Receipts[3].PostingKeys[0] == "payable:day01-small-rent" &&
                mixedFlow.Report.Postings.Count == 1 &&
                mixedFlow.Report.UnpaidPayables.Count == 1 &&
                mixedFlow.Report.UnpaidPayables[0].DueKey == "day01-large-fee");
            var mixedRestored = mixedFlow.Next.Restore();
            Assert(mixedRestored.Header.BalanceKopecks == 550L &&
                mixedRestored.Payables.Outstanding.Count == 1 &&
                mixedRestored.Payables.Outstanding[0].DueKey == "day01-large-fee" &&
                mixedRestored.Finance.Ledger.Entries.Count == 4);
            Throws<InvalidOperationException>(() => DurableDay01Flow.Prepare(
                mixedRestored, mixedRestored.Header.Revision, 0, choices));
            Assert(mixedBefore.Header.Revision == 0 &&
                mixedBefore.Header.BalanceKopecks == 1000L &&
                mixedBefore.Payables.Outstanding.Count == 2 &&
                mixedBefore.Finance.Ledger.Entries.Count == 0);

            // Permuting the input payable list must not change EndDay settlement.
            // Stable DueKey ordering, not insertion order, determines the ledger and report.
            var reversedBefore = new DurableGameStateSnapshot(before.Finance, before.Events,
                new DurablePayablesSnapshot(new[] {
                    new DurablePayable("day01-large-fee", 0, 800L),
                    new DurablePayable("day01-small-rent", 0, 200L)
                }));
            var reversedFlow = DurableDay01Flow.Prepare(reversedBefore, 0, 0, choices);
            Assert(reversedFlow.Next.Header.BalanceKopecks == mixedFlow.Next.Header.BalanceKopecks &&
                reversedFlow.Next.Finance.Ledger.Entries.Count == mixedFlow.Next.Finance.Ledger.Entries.Count &&
                reversedFlow.Next.Finance.Ledger.Entries[3].PostingId ==
                    mixedFlow.Next.Finance.Ledger.Entries[3].PostingId &&
                reversedFlow.Next.Finance.Receipts[3].Fingerprint ==
                    mixedFlow.Next.Finance.Receipts[3].Fingerprint &&
                reversedFlow.Report.UnpaidPayables.Count == 1 &&
                reversedFlow.Report.UnpaidPayables[0].DueKey == "day01-large-fee");
            Assert(reversedBefore.Payables.Outstanding.Count == 2 &&
                reversedBefore.Finance.Ledger.Entries.Count == 0);

            // A liability due tomorrow must not be settled during Day01's EndDay.
            // The day advances once, and the future obligation survives Restore.
            var futureBefore = new DurableGameStateSnapshot(before.Finance, before.Events,
                new DurablePayablesSnapshot(new[] {
                    new DurablePayable("day02-maintenance", 1, 200L)
                }));
            var futureFlow = DurableDay01Flow.Prepare(futureBefore, 0, 0, choices);
            Assert(futureFlow.Next.Header.DayIndex == 1 &&
                futureFlow.Next.Header.Revision == 4 &&
                futureFlow.Next.Header.BalanceKopecks == 750L &&
                futureFlow.Next.Finance.Ledger.Entries.Count == 3 &&
                futureFlow.Next.Finance.Receipts[3].PostingKeys.Count == 0 &&
                futureFlow.Report.Postings.Count == 0 &&
                futureFlow.Next.Payables.Outstanding.Count == 1 &&
                futureFlow.Next.Payables.Outstanding[0].DueKey == "day02-maintenance" &&
                futureFlow.Next.Payables.Outstanding[0].DueDayIndex == 1);
            var futureRestored = futureFlow.Next.Restore();
            Assert(futureRestored.Payables.Outstanding.Count == 1 &&
                futureRestored.Payables.Outstanding[0].DueKey == "day02-maintenance" &&
                futureRestored.Header.BalanceKopecks == 750L);
            Assert(futureBefore.Header.DayIndex == 0 &&
                futureBefore.Payables.Outstanding.Count == 1 &&
                futureBefore.Finance.Ledger.Entries.Count == 0);

            // Due-now and future liabilities coexist: settle only today's payable.
            // The future one must not appear in the EndDay posting or receipt.
            var staggeredBefore = new DurableGameStateSnapshot(before.Finance, before.Events,
                new DurablePayablesSnapshot(new[] {
                    new DurablePayable("day02-future", 1, 100L),
                    new DurablePayable("day01-current", 0, 200L)
                }));
            var staggeredFlow = DurableDay01Flow.Prepare(staggeredBefore, 0, 0, choices);
            AssertNamed(staggeredFlow.Next.Header.DayIndex == 1, "staggered day index");
            AssertNamed(staggeredFlow.Next.Header.BalanceKopecks == 550L, "staggered cash");
            AssertNamed(staggeredFlow.Next.Finance.Ledger.Entries.Count == 4, "staggered ledger count");
            AssertNamed(staggeredFlow.Next.Finance.Ledger.Entries[3].PostingId == "payable:day01-current",
                "staggered ledger posting ID");
            AssertNamed(staggeredFlow.Next.Finance.Receipts[3].PostingKeys.Count == 1,
                "staggered EndDay receipt posting count");
            AssertNamed(staggeredFlow.Next.Finance.Receipts[3].PostingKeys[0] == "payable:day01-current",
                "staggered EndDay receipt posting key");
            AssertNamed(staggeredFlow.Report.Postings.Count == 1, "staggered report posting count");
            AssertNamed(staggeredFlow.Report.UnpaidPayables.Count == 1, "staggered report unpaid count");
            AssertNamed(staggeredFlow.Report.UnpaidPayables[0].DueKey == "day02-future",
                "staggered report future payable key");
            AssertNamed(staggeredFlow.Next.Payables.Outstanding.Count == 1, "staggered outstanding count");
            AssertNamed(staggeredFlow.Next.Payables.Outstanding[0].DueKey == "day02-future",
                "staggered future payable key");
            var staggeredRestored = staggeredFlow.Next.Restore();
            Assert(staggeredRestored.Header.BalanceKopecks == 550L &&
                staggeredRestored.Payables.Outstanding.Count == 1 &&
                staggeredRestored.Payables.Outstanding[0].DueDayIndex == 1);
            Assert(staggeredBefore.Header.Revision == 0 &&
                staggeredBefore.Payables.Outstanding.Count == 2 &&
                staggeredBefore.Finance.Ledger.Entries.Count == 0);

            // Three non-monetary choices still commit three durable decision receipts
            // and one EndDay receipt, without fabricating zero-value ledger postings.
            var nonMonetaryChoices = new[] {
                new Day01DocumentChoice("doc-free-1",
                    new DocumentDecisionEffects("free-a", 0L, 1, 0, 0, 0, 0)),
                new Day01DocumentChoice("doc-free-2",
                    new DocumentDecisionEffects("free-b", 0L, 0, 1, 0, 0, 0)),
                new Day01DocumentChoice("doc-free-3",
                    new DocumentDecisionEffects("free-c", 0L, 0, 0, 1, 0, 0))
            };
            var nonMonetary = DurableDay01Flow.Prepare(before, 0, 0, nonMonetaryChoices);
            Assert(nonMonetary.Next.Header.DayIndex == 1 &&
                nonMonetary.Next.Header.Revision == 4 &&
                nonMonetary.Next.Header.BalanceKopecks == 1000L &&
                nonMonetary.Next.Header.Metrics.EducationQuality == 51 &&
                nonMonetary.Next.Header.Metrics.StudentSatisfaction == 51 &&
                nonMonetary.Next.Header.Metrics.StaffMorale == 51 &&
                nonMonetary.Next.Finance.Ledger.Entries.Count == 0 &&
                nonMonetary.Next.Finance.Receipts.Count == 4 &&
                nonMonetary.Next.Finance.Receipts[0].PostingKeys.Count == 0 &&
                nonMonetary.Next.Finance.Receipts[1].PostingKeys.Count == 0 &&
                nonMonetary.Next.Finance.Receipts[2].PostingKeys.Count == 0 &&
                nonMonetary.Next.Finance.Receipts[3].PostingKeys.Count == 0 &&
                nonMonetary.Report.Postings.Count == 0);
            var nonMonetaryRestored = nonMonetary.Next.Restore();
            Assert(nonMonetaryRestored.Header.BalanceKopecks == 1000L &&
                nonMonetaryRestored.Finance.Ledger.Entries.Count == 0 &&
                nonMonetaryRestored.Finance.Receipts.Count == 4);
            Throws<InvalidOperationException>(() => DurableDay01Flow.Prepare(
                nonMonetaryRestored, nonMonetaryRestored.Header.Revision, 0, nonMonetaryChoices));
            Assert(before.Header.Revision == 0 &&
                before.Header.BalanceKopecks == 1000L &&
                before.Finance.Receipts.Count == 0 &&
                before.Finance.Ledger.Entries.Count == 0);

            // Replaying the same immutable input with the same ordered choices must
            // produce byte-for-byte stable financial identifiers and receipt fingerprints.
            // This checks deterministic Prepare without requiring SaveService or Unity.
            var deterministicA = DurableDay01Flow.Prepare(withPayable, 0, 0, choices);
            var deterministicB = DurableDay01Flow.Prepare(withPayable, 0, 0, choices);
            Assert(deterministicA.Next.Header.Revision == deterministicB.Next.Header.Revision &&
                deterministicA.Next.Header.BalanceKopecks == deterministicB.Next.Header.BalanceKopecks &&
                deterministicA.Next.Finance.Ledger.Entries.Count == deterministicB.Next.Finance.Ledger.Entries.Count &&
                deterministicA.Next.Finance.Receipts.Count == deterministicB.Next.Finance.Receipts.Count);
            for (int i = 0; i < deterministicA.Next.Finance.Ledger.Entries.Count; i++)
            {
                Assert(deterministicA.Next.Finance.Ledger.Entries[i].PostingId ==
                    deterministicB.Next.Finance.Ledger.Entries[i].PostingId);
            }
            for (int i = 0; i < deterministicA.Next.Finance.Receipts.Count; i++)
                Assert(deterministicA.Next.Finance.Receipts[i].Fingerprint ==
                    deterministicB.Next.Finance.Receipts[i].Fingerprint);
            Assert(withPayable.Header.Revision == 0 &&
                withPayable.Finance.Receipts.Count == 0 &&
                withPayable.Finance.Ledger.Entries.Count == 0);

            // A future event must remain pending even when today's payable is unaffordable.
            // The debt and event are independent durable obligations across EndDay/Restore.
            var deferredUnpaidBefore = new DurableGameStateSnapshot(
                futureEventBefore.Finance,
                futureEventBefore.Events,
                new DurablePayablesSnapshot(new[] {
                    new DurablePayable("day01-event-unpaid", 0, 800L)
                }));
            var deferredUnpaidFlow = DurableDay01Flow.Prepare(deferredUnpaidBefore, 0, 0, choices);
            Assert(deferredUnpaidFlow.Next.Header.DayIndex == 1 &&
                deferredUnpaidFlow.Next.Header.Revision == 4 &&
                deferredUnpaidFlow.Next.Header.BalanceKopecks == 750L &&
                deferredUnpaidFlow.Next.Finance.Ledger.Entries.Count == 3 &&
                deferredUnpaidFlow.Next.Finance.Receipts.Count == 4 &&
                deferredUnpaidFlow.Next.Finance.Receipts[3].PostingKeys.Count == 0 &&
                deferredUnpaidFlow.Report.Postings.Count == 0 &&
                deferredUnpaidFlow.Report.UnpaidPayables.Count == 1 &&
                deferredUnpaidFlow.Report.UnpaidPayables[0].DueKey == "day01-event-unpaid" &&
                deferredUnpaidFlow.Next.Events.Pending.Count == 1 &&
                deferredUnpaidFlow.Next.Events.Pending[0].InstanceId == "event-tomorrow" &&
                deferredUnpaidFlow.Next.Events.CompletedIds.Count == 0);
            var deferredUnpaidRestored = deferredUnpaidFlow.Next.Restore();
            Assert(deferredUnpaidRestored.Payables.Outstanding.Count == 1 &&
                deferredUnpaidRestored.Payables.Outstanding[0].DueKey == "day01-event-unpaid" &&
                deferredUnpaidRestored.Events.Pending.Count == 1 &&
                deferredUnpaidRestored.Events.Pending[0].DueDayIndex == 1 &&
                deferredUnpaidRestored.Events.CompletedIds.Count == 0);
            Throws<InvalidOperationException>(() => DurableDay01Flow.Prepare(
                deferredUnpaidRestored, deferredUnpaidRestored.Header.Revision, 0, choices));
            Assert(deferredUnpaidBefore.Header.DayIndex == 0 &&
                deferredUnpaidBefore.Header.BalanceKopecks == 1000L &&
                deferredUnpaidBefore.Payables.Outstanding.Count == 1 &&
                deferredUnpaidBefore.Events.Pending.Count == 1 &&
                deferredUnpaidBefore.Finance.Ledger.Entries.Count == 0);

            // A future scheduled event and a due-now payable must coexist: EndDay
            // settles only the payable and leaves tomorrow's event unconsumed.
            var combinedBefore = new DurableGameStateSnapshot(
                futureEventBefore.Finance,
                futureEventBefore.Events,
                new DurablePayablesSnapshot(new[] {
                    new DurablePayable("day01-event-rent", 0, 200L)
                }));
            var combinedFlow = DurableDay01Flow.Prepare(combinedBefore, 0, 0, choices);
            Assert(combinedFlow.Next.Header.DayIndex == 1 &&
                combinedFlow.Next.Header.Revision == 4 &&
                combinedFlow.Next.Header.BalanceKopecks == 550L &&
                combinedFlow.Next.Finance.Ledger.Entries.Count == 4 &&
                combinedFlow.Next.Finance.Ledger.Entries[3].PostingId == "payable:day01-event-rent" &&
                combinedFlow.Next.Finance.Receipts.Count == 4 &&
                combinedFlow.Next.Finance.Receipts[3].PostingKeys.Count == 1 &&
                combinedFlow.Report.Postings.Count == 1 &&
                combinedFlow.Report.UnpaidPayables.Count == 0 &&
                combinedFlow.Next.Payables.Outstanding.Count == 0 &&
                combinedFlow.Next.Events.Pending.Count == 1 &&
                combinedFlow.Next.Events.Pending[0].InstanceId == "event-tomorrow" &&
                combinedFlow.Next.Events.CompletedIds.Count == 0);
            var combinedRestored = combinedFlow.Next.Restore();
            Assert(combinedRestored.Header.BalanceKopecks == 550L &&
                combinedRestored.Finance.Ledger.Entries.Count == 4 &&
                combinedRestored.Payables.Outstanding.Count == 0 &&
                combinedRestored.Events.Pending.Count == 1 &&
                combinedRestored.Events.Pending[0].DueDayIndex == 1 &&
                combinedRestored.Events.CompletedIds.Count == 0);
            Assert(combinedBefore.Header.DayIndex == 0 &&
                combinedBefore.Header.BalanceKopecks == 1000L &&
                combinedBefore.Payables.Outstanding.Count == 1 &&
                combinedBefore.Events.Pending.Count == 1 &&
                combinedBefore.Finance.Ledger.Entries.Count == 0);

            // Both a future payable and a future NPC event must survive Day01 together.
            // Neither becomes due during DayIndex 0, and the report still projects debt.
            var futureWorkBefore = new DurableGameStateSnapshot(
                futureEventBefore.Finance, futureEventBefore.Events,
                new DurablePayablesSnapshot(new[] {
                    new DurablePayable("day02-shared-future", 1, 200L)
                }));
            var futureWork = DurableDay01Flow.Prepare(futureWorkBefore, 0, 0, choices);
            AssertNamed(futureWork.Next.Header.DayIndex == 1 &&
                futureWork.Next.Header.BalanceKopecks == 750L &&
                futureWork.Next.Finance.Ledger.Entries.Count == 3 &&
                futureWork.Report.Postings.Count == 0 &&
                futureWork.Report.UnpaidPayables.Count == 1 &&
                futureWork.Report.UnpaidPayables[0].DueKey == "day02-shared-future" &&
                futureWork.Next.Events.Pending.Count == 1 &&
                futureWork.Next.Events.Pending[0].InstanceId == "event-tomorrow",
                "Day01 future payable and event coexist");
            var futureWorkRestored = futureWork.Next.Restore();
            AssertNamed(futureWorkRestored.Payables.Outstanding.Count == 1 &&
                futureWorkRestored.Payables.Outstanding[0].DueDayIndex == 1 &&
                futureWorkRestored.Events.Pending.Count == 1 &&
                futureWorkRestored.Events.Pending[0].DueDayIndex == 1 &&
                futureWorkRestored.Events.CompletedIds.Count == 0 &&
                futureWorkBefore.Header.Revision == 0 &&
                futureWorkBefore.Finance.Ledger.Entries.Count == 0,
                "Day01 future work restore and input immutability");

            // A due NPC event on Day02 must fail closed before the payable can settle.
            // The same snapshot contains both due work items; Core must not silently
            // skip the event, charge money, or append an EndDay receipt.
            Throws<InvalidOperationException>(() => DurableDayAdvance.PrepareDay(
                futureWorkRestored, futureWorkRestored.Header.Revision, 1));
            AssertNamed(futureWorkRestored.Header.DayIndex == 1 &&
                futureWorkRestored.Header.BalanceKopecks == 750L &&
                futureWorkRestored.Payables.Outstanding.Count == 1 &&
                futureWorkRestored.Payables.Outstanding[0].DueKey == "day02-shared-future" &&
                futureWorkRestored.Events.Pending.Count == 1 &&
                futureWorkRestored.Events.CompletedIds.Count == 0 &&
                futureWorkRestored.Finance.Ledger.Entries.Count == 3 &&
                futureWorkRestored.Finance.Receipts.Count == 4,
                "Day02 due NPC event blocks payable EndDay atomically");

            // A due NPC event must also block EndDay when there is NO payable.
            // This isolates event handling from the financial settlement guard.
            var eventOnlyDay02 = new DurableGameStateSnapshot(
                futureWorkRestored.Finance, futureWorkRestored.Events,
                new DurablePayablesSnapshot(new DurablePayable[0]));
            Throws<InvalidOperationException>(() => DurableDayAdvance.PrepareDay(
                eventOnlyDay02, eventOnlyDay02.Header.Revision, 1));
            AssertNamed(eventOnlyDay02.Header.DayIndex == 1 &&
                eventOnlyDay02.Header.BalanceKopecks == 750L &&
                eventOnlyDay02.Payables.Outstanding.Count == 0 &&
                eventOnlyDay02.Events.Pending.Count == 1 &&
                eventOnlyDay02.Events.Pending[0].InstanceId == "event-tomorrow" &&
                eventOnlyDay02.Events.CompletedIds.Count == 0 &&
                eventOnlyDay02.Finance.Ledger.Entries.Count == 3 &&
                eventOnlyDay02.Finance.Receipts.Count == 4,
                "Day02 due NPC event without payable blocks EndDay atomically");

            // Restoring an event-only Day02 snapshot must not bypass the due-event
            // guard or produce a phantom receipt. This is a save/load boundary check.
            var restoredEventOnlyDay02 = eventOnlyDay02.Restore();
            Throws<InvalidOperationException>(() => DurableDayAdvance.PrepareDay(
                restoredEventOnlyDay02, restoredEventOnlyDay02.Header.Revision, 1));
            AssertNamed(restoredEventOnlyDay02.Header.DayIndex == 1 &&
                restoredEventOnlyDay02.Header.BalanceKopecks == 750L &&
                restoredEventOnlyDay02.Events.Pending.Count == 1 &&
                restoredEventOnlyDay02.Events.CompletedIds.Count == 0 &&
                restoredEventOnlyDay02.Payables.Outstanding.Count == 0 &&
                restoredEventOnlyDay02.Finance.Ledger.Entries.Count == 3 &&
                restoredEventOnlyDay02.Finance.Receipts.Count == 4,
                "Day02 event-only Restore preserves fail-closed EndDay");

            // A future payable must settle exactly once on Day02 after a clean Day01.
            // This is the Core-only path: no NPC due-event is present in this fixture.
            var tomorrowPayable = new DurableGameStateSnapshot(before.Finance, before.Events,
                new DurablePayablesSnapshot(new[] {
                    new DurablePayable("day02-settlement", 1, 200L)
                }));
            var day01Prepared = DurableDay01Flow.Prepare(tomorrowPayable, 0, 0, choices);
            AssertNamed(day01Prepared.Next.Header.BalanceKopecks == 750L &&
                day01Prepared.Next.Payables.Outstanding.Count == 1 &&
                day01Prepared.Next.Finance.Ledger.Entries.Count == 3,
                "Day02 payable deferred through Day01");
            var day02Input = day01Prepared.Next.Restore();
            // A Day03 payable is not due at Day02 close. EndDay must use the
            // quiet path, preserve the future obligation and append no ledger posting.
            var futureDay03Input = new DurableGameStateSnapshot(
                day02Input.Finance, day02Input.Events,
                new DurablePayablesSnapshot(new[] {
                    new DurablePayable("day03-future", 2, 200L)
                }));
            var futureDay03Deferred = DurableDayAdvance.PrepareDay(
                futureDay03Input, futureDay03Input.Header.Revision, 1);
            AssertNamed(futureDay03Deferred.Header.DayIndex == 2 &&
                futureDay03Deferred.Header.BalanceKopecks == 750L &&
                futureDay03Deferred.Payables.Outstanding.Count == 1 &&
                futureDay03Deferred.Payables.Outstanding[0].DueKey == "day03-future" &&
                futureDay03Deferred.Finance.Ledger.Entries.Count == 3 &&
                futureDay03Deferred.Finance.Receipts.Count == 5 &&
                futureDay03Input.Header.DayIndex == 1 &&
                futureDay03Input.Finance.Receipts.Count == 4,
                "Day03 future payable remains deferred at Day02 EndDay");

            // Once Day03 actually closes, the previously deferred obligation is due.
            // Settlement must happen exactly once and must not mutate the Day02 snapshot.
            var futureDay03Closed = DurableDayAdvance.PrepareDay(
                futureDay03Deferred, futureDay03Deferred.Header.Revision, 2);
            AssertNamed(futureDay03Closed.Header.DayIndex == 3 &&
                futureDay03Closed.Header.BalanceKopecks == 550L &&
                futureDay03Closed.Payables.Outstanding.Count == 0 &&
                futureDay03Closed.Finance.Ledger.Entries.Count == 4 &&
                futureDay03Closed.Finance.Ledger.Entries[3].PostingId == "payable:day03-future" &&
                futureDay03Closed.Finance.Receipts.Count == 6 &&
                futureDay03Closed.Finance.Receipts[5].PostingKeys.Count == 1 &&
                futureDay03Closed.Finance.Receipts[5].PostingKeys[0] == "payable:day03-future" &&
                futureDay03Deferred.Header.DayIndex == 2 &&
                futureDay03Deferred.Header.BalanceKopecks == 750L &&
                futureDay03Deferred.Payables.Outstanding.Count == 1 &&
                futureDay03Deferred.Finance.Ledger.Entries.Count == 3 &&
                futureDay03Deferred.Finance.Receipts.Count == 5,
                "Day03 deferred payable settles only on its due day");
            var futureDay03Restored = futureDay03Closed.Restore();
            Throws<InvalidOperationException>(() => DurableDayAdvance.PrepareDay(
                futureDay03Restored, futureDay03Restored.Header.Revision, 2));
            AssertNamed(futureDay03Restored.Header.BalanceKopecks == 550L &&
                futureDay03Restored.Payables.Outstanding.Count == 0 &&
                futureDay03Restored.Finance.Ledger.Entries.Count == 4 &&
                futureDay03Restored.Finance.Receipts.Count == 6,
                "Day03 payable restore and stale replay guard");


            var day02Closed = DurableDayAdvance.PrepareDay(day02Input,
                day02Input.Header.Revision, 1);
            AssertNamed(day02Closed.Header.DayIndex == 2 &&
                day02Closed.Header.Revision == 5 &&
                day02Closed.Header.BalanceKopecks == 550L &&
                day02Closed.Payables.Outstanding.Count == 0 &&
                day02Closed.Finance.Ledger.Entries.Count == 4 &&
                day02Closed.Finance.Ledger.Entries[3].PostingId == "payable:day02-settlement" &&
                day02Closed.Finance.Receipts.Count == 5 &&
                day02Closed.Finance.Receipts[4].PostingKeys.Count == 1 &&
                day02Closed.Finance.Receipts[4].PostingKeys[0] == "payable:day02-settlement",
                "Day02 payable settled once");
            var day02Restored = day02Closed.Restore();
            AssertNamed(day02Restored.Header.BalanceKopecks == 550L &&
                day02Restored.Payables.Outstanding.Count == 0 &&
                day02Restored.Finance.Ledger.Entries.Count == 4 &&
                day02Restored.Finance.Receipts.Count == 5,
                "Day02 payable restore");
            Throws<InvalidOperationException>(() => DurableDayAdvance.PrepareDay(
                day02Restored, day02Restored.Header.Revision, 1));
            AssertNamed(day02Input.Header.DayIndex == 1 &&
                day02Input.Header.BalanceKopecks == 750L &&
                day02Input.Payables.Outstanding.Count == 1 &&
                day02Input.Finance.Ledger.Entries.Count == 3 &&
                tomorrowPayable.Header.DayIndex == 0 &&
                tomorrowPayable.Finance.Ledger.Entries.Count == 0,
                "Day02 settlement input immutability");

            // An unaffordable Day02 liability stays payable, never creating negative cash.
            // Day01 must not charge the future liability, and Restore must not replay it.
            var tomorrowShortfall = new DurableGameStateSnapshot(before.Finance, before.Events,
                new DurablePayablesSnapshot(new[] {
                    new DurablePayable("day02-shortfall", 1, 800L)
                }));
            var shortfallDay01 = DurableDay01Flow.Prepare(tomorrowShortfall, 0, 0, choices);
            var shortfallDay02Input = shortfallDay01.Next.Restore();
            var shortfallDay02 = DurableDayAdvance.PrepareDay(shortfallDay02Input,
                shortfallDay02Input.Header.Revision, 1);
            AssertNamed(shortfallDay02.Header.DayIndex == 2 &&
                shortfallDay02.Header.BalanceKopecks == 750L &&
                shortfallDay02.Payables.Outstanding.Count == 1 &&
                shortfallDay02.Payables.Outstanding[0].DueKey == "day02-shortfall" &&
                shortfallDay02.Finance.Ledger.Entries.Count == 3 &&
                shortfallDay02.Finance.Receipts.Count == 5,
                "Day02 insufficient mandatory payable remains outstanding");
            var shortfallRestored = shortfallDay02.Restore();
            AssertNamed(shortfallRestored.Header.BalanceKopecks == 750L &&
                shortfallRestored.Payables.Outstanding.Count == 1 &&
                shortfallRestored.Finance.Ledger.Entries.Count == 3 &&
                shortfallDay02Input.Header.DayIndex == 1 &&
                shortfallDay02Input.Payables.Outstanding.Count == 1 &&
                tomorrowShortfall.Header.DayIndex == 0,
                "Day02 shortfall restore and source immutability");
            // A committed Day02 with insufficient cash cannot be applied twice.
            // The outstanding debt remains, but a stale EndDay must not create
            // a second receipt, mutate cash, or advance the day again.
            Throws<InvalidOperationException>(() => DurableDayAdvance.PrepareDay(
                shortfallRestored, shortfallRestored.Header.Revision, 1));
            AssertNamed(shortfallRestored.Header.DayIndex == 2 &&
                shortfallRestored.Header.BalanceKopecks == 750L &&
                shortfallRestored.Payables.Outstanding.Count == 1 &&
                shortfallRestored.Finance.Ledger.Entries.Count == 3 &&
                shortfallRestored.Finance.Receipts.Count == 5,
                "Day02 insufficient payable stale replay leaves snapshot unchanged");

            // A stale revision on the correct Day02 must fail before settlement.
            // This protects against concurrent UI submissions using an old read model.
            Throws<InvalidOperationException>(() => DurableDayAdvance.PrepareDay(
                day02Input, day02Input.Header.Revision - 1, 1));
            AssertNamed(day02Input.Header.DayIndex == 1 &&
                day02Input.Header.BalanceKopecks == 750L &&
                day02Input.Payables.Outstanding.Count == 1 &&
                day02Input.Finance.Ledger.Entries.Count == 3 &&
                day02Input.Finance.Receipts.Count == 4,
                "Day02 stale revision rejected without financial mutation");

            // A future-day EndDay request must not settle a Day02 payable early.
            // This is distinct from a stale revision: the revision is current,
            // but the expected day is ahead of the snapshot's DayIndex.
            Throws<InvalidOperationException>(() => DurableDayAdvance.PrepareDay(
                day02Input, day02Input.Header.Revision, 2));
            AssertNamed(day02Input.Header.DayIndex == 1 &&
                day02Input.Header.BalanceKopecks == 750L &&
                day02Input.Payables.Outstanding.Count == 1 &&
                day02Input.Finance.Ledger.Entries.Count == 3 &&
                day02Input.Finance.Receipts.Count == 4,
                "Day02 future-day precondition rejected without settlement");

            // A past-day EndDay request with the current revision must also fail.
            // A stale UI day token must not settle today's payable or append receipts.
            Throws<InvalidOperationException>(() => DurableDayAdvance.PrepareDay(
                day02Input, day02Input.Header.Revision, 0));
            AssertNamed(day02Input.Header.DayIndex == 1 &&
                day02Input.Header.BalanceKopecks == 750L &&
                day02Input.Payables.Outstanding.Count == 1 &&
                day02Input.Finance.Ledger.Entries.Count == 3 &&
                day02Input.Finance.Receipts.Count == 4,
                "Day02 past-day precondition rejected without settlement");

            // A negative day token is invalid even with a current revision.
            // No settlement or receipt may be applied to the source snapshot.
            Throws<InvalidOperationException>(() => DurableDayAdvance.PrepareDay(
                day02Input, day02Input.Header.Revision, -1));
            AssertNamed(day02Input.Header.DayIndex == 1 &&
                day02Input.Header.BalanceKopecks == 750L &&
                day02Input.Payables.Outstanding.Count == 1 &&
                day02Input.Finance.Ledger.Entries.Count == 3 &&
                day02Input.Finance.Receipts.Count == 4,
                "Day02 negative expected-day rejected without settlement");

            // A Day02 liability just one kopeck above available cash must remain
            // outstanding; the equality case below must still settle to zero.
            var oneKopeckShortInput = new DurableGameStateSnapshot(
                day02Input.Finance, day02Input.Events,
                new DurablePayablesSnapshot(new[] {
                    new DurablePayable("day02-one-kopeck-short", 1, 751L)
                }));
            var oneKopeckShortClosed = DurableDayAdvance.PrepareDay(
                oneKopeckShortInput, oneKopeckShortInput.Header.Revision, 1);
            AssertNamed(oneKopeckShortClosed.Header.DayIndex == 2 &&
                oneKopeckShortClosed.Header.BalanceKopecks == 750L &&
                oneKopeckShortClosed.Payables.Outstanding.Count == 1 &&
                oneKopeckShortClosed.Payables.Outstanding[0].DueKey == "day02-one-kopeck-short" &&
                oneKopeckShortClosed.Finance.Ledger.Entries.Count == 3 &&
                oneKopeckShortClosed.Finance.Receipts.Count == 5 &&
                oneKopeckShortInput.Header.DayIndex == 1 &&
                oneKopeckShortInput.Header.BalanceKopecks == 750L &&
                oneKopeckShortInput.Finance.Receipts.Count == 4,
                "Day02 one-kopeck shortfall does not spend or mutate input");

            // An exactly affordable Day02 liability must settle completely, leaving zero
            // cash rather than treating equality as insufficient funds. Restore and replay
            // must preserve the single posting/receipt without mutating the source.
            var exactFundsInput = new DurableGameStateSnapshot(
                day02Input.Finance, day02Input.Events,
                new DurablePayablesSnapshot(new[] {
                    new DurablePayable("day02-exact-funds", 1, 750L)
                }));
            var exactFundsClosed = DurableDayAdvance.PrepareDay(exactFundsInput,
                exactFundsInput.Header.Revision, 1);
            AssertNamed(exactFundsClosed.Header.DayIndex == 2 &&
                exactFundsClosed.Header.BalanceKopecks == 0L &&
                exactFundsClosed.Payables.Outstanding.Count == 0 &&
                exactFundsClosed.Finance.Ledger.Entries.Count == 4 &&
                exactFundsClosed.Finance.Ledger.Entries[3].PostingId == "payable:day02-exact-funds" &&
                exactFundsClosed.Finance.Receipts.Count == 5 &&
                exactFundsClosed.Finance.Receipts[4].PostingKeys.Count == 1 &&
                exactFundsClosed.Finance.Receipts[4].PostingKeys[0] == "payable:day02-exact-funds",
                "Day02 exact-funds payable settles to zero");
            var exactFundsRestored = exactFundsClosed.Restore();
            Throws<InvalidOperationException>(() => DurableDayAdvance.PrepareDay(
                exactFundsRestored, exactFundsRestored.Header.Revision, 1));
            AssertNamed(exactFundsRestored.Header.BalanceKopecks == 0L &&
                exactFundsRestored.Payables.Outstanding.Count == 0 &&
                exactFundsRestored.Finance.Ledger.Entries.Count == 4 &&
                exactFundsRestored.Finance.Receipts.Count == 5 &&
                exactFundsInput.Header.DayIndex == 1 &&
                exactFundsInput.Header.BalanceKopecks == 750L &&
                exactFundsInput.Payables.Outstanding.Count == 1 &&
                exactFundsInput.Finance.Ledger.Entries.Count == 3 &&
                exactFundsInput.Finance.Receipts.Count == 4,
                "Day02 exact-funds restore, replay guard and source immutability");

            // A replay against a newer snapshot must reject the stale Day01 precondition.
            Throws<InvalidOperationException>(() => DurableDay01Flow.Prepare(result.Next,
                result.Next.Header.Revision, 0, choices));
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

        private static void AssertNamed(bool condition, string invariant)
        {
            if (!condition) throw new Exception("GameState snapshot assertion failed: " + invariant);
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
