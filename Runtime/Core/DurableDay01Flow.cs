using System;
using System.Collections.Generic;

namespace TAU.President.Core
{
    /// <summary>One Day01 document choice with externally approved effects.</summary>
    public sealed class Day01DocumentChoice
    {
        public string CommandId { get; private set; }
        public DocumentDecisionEffects Effects { get; private set; }

        public Day01DocumentChoice(string commandId, DocumentDecisionEffects effects)
        {
            CommandId = GameStateSnapshotDto.Required(commandId, nameof(commandId));
            Effects = effects ?? throw new ArgumentNullException(nameof(effects));
        }
    }

    /// <summary>Detached Day01 result. Caller publishes Next once, after successful preparation.</summary>
    public sealed class DurableDay01Result
    {
        public DurableGameStateSnapshot Next { get; private set; }
        public DurableDayReport Report { get; private set; }

        public DurableDay01Result(DurableGameStateSnapshot next, DurableDayReport report)
        {
            Next = next ?? throw new ArgumentNullException(nameof(next));
            Report = report ?? throw new ArgumentNullException(nameof(report));
        }
    }

    /// <summary>
    /// Pure orchestration of exactly three approved document choices and an EndDay.
    /// No intermediate snapshot is published. Events and weekly settlements fail closed
    /// through the canonical Core EndDay entrypoint.
    /// </summary>
    public static class DurableDay01Flow
    {
        public static DurableDay01Result Prepare(
            DurableGameStateSnapshot current, long expectedRevision, int expectedDayIndex,
            IReadOnlyList<Day01DocumentChoice> choices)
        {
            if (current == null) throw new ArgumentNullException(nameof(current));
            if (choices == null) throw new ArgumentNullException(nameof(choices));
            if (choices.Count != 3)
                throw new ArgumentException("Day01 requires exactly three document choices.", nameof(choices));
            if (expectedDayIndex != 0 || current.Header.DayIndex != 0)
                throw new InvalidOperationException("Day01 flow requires DayIndex 0.");
            if (current.Header.Revision != expectedRevision || current.Header.IsFinished)
                throw new InvalidOperationException("Stale or finished Day01.");
            // EndDay owns this command ID. Reject collisions before staging any choice.
            // Otherwise a document can reserve the ID needed for the atomic day close.
            string endDayCommandId = "end-day:" + current.Header.CampaignId + ":0";
            // A previously persisted command must not own the EndDay ID either.
            // Reject before staging any of the three document decisions.
            foreach (var processedId in current.Header.ProcessedCommandIds)
                if (StringComparer.Ordinal.Equals(processedId, endDayCommandId))
                    throw new InvalidOperationException("EndDay command ID already processed.");
            var previouslyProcessed = new HashSet<string>(current.Header.ProcessedCommandIds, StringComparer.Ordinal);
            var ids = new HashSet<string>(StringComparer.Ordinal);
            var decisionIds = new HashSet<string>(StringComparer.Ordinal);
            foreach (var choice in choices)
            {
                if (choice == null) throw new ArgumentException("Null document choice.", nameof(choices));
                if (StringComparer.Ordinal.Equals(choice.CommandId, endDayCommandId))
                    throw new ArgumentException("Document command ID is reserved for EndDay.", nameof(choices));
                if (previouslyProcessed.Contains(choice.CommandId))
                    throw new InvalidOperationException("Document command ID already processed.");
                if (!ids.Add(choice.CommandId) || !decisionIds.Add(choice.Effects.DecisionId))
                    throw new ArgumentException("Duplicate document command or decision.", nameof(choices));
            }

            var staged = current;
            foreach (var choice in choices)
                staged = DurableDocumentDecision.Prepare(staged, choice.CommandId,
                    staged.Header.Revision, expectedDayIndex, choice.Effects);
            var closed = DurableDayAdvance.PrepareDay(staged, staged.Header.Revision, expectedDayIndex);
            var report = DurableDayReport.Capture(staged, closed);
            return new DurableDay01Result(closed, report);
        }
    }
}
