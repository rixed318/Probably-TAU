using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace TAU.President.Core
{
    /// <summary>Content-supplied, already-approved effects; Core does not choose balance values.</summary>
    public sealed class DocumentDecisionEffects
    {
        public string DecisionId { get; private set; }
        public long ImmediateCashDeltaKopecks { get; private set; }
        public int QualityDelta { get; private set; }
        public int StudentsDelta { get; private set; }
        public int StaffDelta { get; private set; }
        public int EmployersDelta { get; private set; }
        public int ReputationDelta { get; private set; }

        public DocumentDecisionEffects(string decisionId, long immediateCashDeltaKopecks,
            int qualityDelta, int studentsDelta, int staffDelta, int employersDelta, int reputationDelta)
        {
            DecisionId = GameStateSnapshotDto.Required(decisionId, nameof(decisionId));
            ImmediateCashDeltaKopecks = immediateCashDeltaKopecks;
            QualityDelta = qualityDelta;
            StudentsDelta = studentsDelta;
            StaffDelta = staffDelta;
            EmployersDelta = employersDelta;
            ReputationDelta = reputationDelta;
        }
    }

    /// <summary>Detached, atomic Core command preparation. The caller commits the returned snapshot.</summary>
    public static class DurableDocumentDecision
    {
        public static DurableGameStateSnapshot Prepare(
            DurableGameStateSnapshot current, string commandId,
            long expectedRevision, int expectedDayIndex, DocumentDecisionEffects effects)
        {
            if (current == null) throw new ArgumentNullException(nameof(current));
            if (effects == null) throw new ArgumentNullException(nameof(effects));
            commandId = GameStateSnapshotDto.Required(commandId, nameof(commandId));
            if (commandId.StartsWith("end-day:", StringComparison.Ordinal))
                throw new ArgumentException("Reserved EndDay command prefix.", nameof(commandId));
            var h = current.Header;
            // A repeated command cannot apply twice, even if its expected revision is stale.
            foreach (var processed in h.ProcessedCommandIds)
                if (StringComparer.Ordinal.Equals(processed, commandId))
                    throw new InvalidOperationException("Document command already committed.");
            if (h.IsFinished || h.Revision != expectedRevision || h.DayIndex != expectedDayIndex)
                throw new InvalidOperationException("Stale or finished document command.");

            var old = h.Metrics;
            var nextMetrics = new AcademyMetricsDto(
                checked(old.EducationQuality + effects.QualityDelta),
                checked(old.StudentSatisfaction + effects.StudentsDelta),
                checked(old.StaffMorale + effects.StaffDelta),
                checked(old.EmployerTrust + effects.EmployersDelta),
                checked(old.PublicReputation + effects.ReputationDelta));

            var postings = new List<LedgerEntry>();
            var postingKeys = new List<string>();
            if (effects.ImmediateCashDeltaKopecks != 0)
            {
                string postingId = "decision:" + commandId;
                postings.Add(new LedgerEntry(postingId, commandId, effects.ImmediateCashDeltaKopecks));
                postingKeys.Add(postingId);
            }
            // Checked arithmetic and insufficient-cash rejection happen before any publish.
            var nextLedger = current.Finance.Ledger.WithPostings(postings);
            var commandIds = new List<string>(h.ProcessedCommandIds);
            commandIds.Add(commandId);
            var receipts = new List<DurableCommandReceipt>(current.Finance.Receipts);
            var fingerprint = new StringBuilder("document-decision-v1:");
            fingerprint.Append(effects.DecisionId.Length.ToString(CultureInfo.InvariantCulture))
                .Append(':').Append(effects.DecisionId);
            foreach (long value in new long[] { effects.ImmediateCashDeltaKopecks,
                effects.QualityDelta, effects.StudentsDelta, effects.StaffDelta,
                effects.EmployersDelta, effects.ReputationDelta })
                fingerprint.Append(':').Append(value.ToString(CultureInfo.InvariantCulture));
            receipts.Add(new DurableCommandReceipt(commandId, fingerprint.ToString(),
                checked(h.Revision + 1), postingKeys));
            var nextHeader = new GameStateSnapshotDto(checked(h.Revision + 1),
                h.CampaignId, h.CampaignSeed, h.DifficultyId, h.ContentVersion,
                h.DayIndex, false, nextMetrics, h.OpeningBalanceKopecks,
                nextLedger.CurrentBalanceKopecks, h.RandomStreams, commandIds,
                h.ProcessedDayKeys, h.PendingEventIds, h.CompletedEventIds);
            return new DurableGameStateSnapshot(
                new DurableFinanceSnapshot(nextHeader, nextLedger, receipts),
                current.Events, current.Payables);
        }
    }
}
