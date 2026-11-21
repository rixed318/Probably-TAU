using System;
using System.Collections.Generic;

namespace TAU.President.Core
{
    /// <summary>Pure C# v1 domain snapshot. No IO, Unity types, or mutable collections.</summary>
    public sealed class GameStateSnapshotDto
    {
        public const int CurrentSchemaVersion = 1;
        public int SchemaVersion { get { return CurrentSchemaVersion; } }
        public long Revision { get; private set; }
        public string CampaignId { get; private set; }
        public ulong CampaignSeed { get; private set; }
        public string DifficultyId { get; private set; }
        public string ContentVersion { get; private set; }
        public int DayIndex { get; private set; }
        public bool IsFinished { get; private set; }
        public AcademyMetricsDto Metrics { get; private set; }
        public long OpeningBalanceKopecks { get; private set; }
        public long BalanceKopecks { get; private set; }
        private readonly List<RandomStreamSnapshotDto> randomStreams;
        private readonly List<string> processedCommandIds;
        private readonly List<string> processedDayKeys;
        private readonly List<string> pendingEventIds;
        private readonly List<string> completedEventIds;
        public IReadOnlyList<RandomStreamSnapshotDto> RandomStreams { get { return randomStreams.AsReadOnly(); } }
        public IReadOnlyList<string> ProcessedCommandIds { get { return processedCommandIds.AsReadOnly(); } }
        public IReadOnlyList<string> ProcessedDayKeys { get { return processedDayKeys.AsReadOnly(); } }
        public IReadOnlyList<string> PendingEventIds { get { return pendingEventIds.AsReadOnly(); } }
        public IReadOnlyList<string> CompletedEventIds { get { return completedEventIds.AsReadOnly(); } }

        // This is a minimal snapshot slice, NOT the final persistence wire format:
        // receipts with fingerprints, ledger entries, debts and typed NPC state come later.
        public GameStateSnapshotDto(long revision, string campaignId, ulong campaignSeed,
            string difficultyId, string contentVersion, int dayIndex, bool isFinished,
            AcademyMetricsDto metrics, long openingBalanceKopecks, long balanceKopecks,
            IEnumerable<RandomStreamSnapshotDto> randomStreams,
            IEnumerable<string> processedCommandIds, IEnumerable<string> processedDayKeys,
            IEnumerable<string> pendingEventIds, IEnumerable<string> completedEventIds)
        {
            if (revision < 0) throw new ArgumentOutOfRangeException(nameof(revision));
            if (dayIndex < 0 || dayIndex == int.MaxValue)
                throw new ArgumentOutOfRangeException(nameof(dayIndex));
            if (openingBalanceKopecks < 0 || balanceKopecks < 0)
                throw new ArgumentOutOfRangeException(nameof(balanceKopecks), "Cash cannot be negative.");
            CampaignId = Required(campaignId, nameof(campaignId));
            DifficultyId = Required(difficultyId, nameof(difficultyId));
            ContentVersion = Required(contentVersion, nameof(contentVersion));
            if (metrics == null) throw new ArgumentNullException(nameof(metrics));
            Revision = revision;
            CampaignSeed = campaignSeed;
            DayIndex = dayIndex;
            IsFinished = isFinished;
            Metrics = metrics;
            OpeningBalanceKopecks = openingBalanceKopecks;
            BalanceKopecks = balanceKopecks;
            this.randomStreams = CopyUnique(randomStreams, nameof(randomStreams), x => x.StreamId);
            this.processedCommandIds = CopyIds(processedCommandIds, nameof(processedCommandIds));
            this.processedDayKeys = CopyIds(processedDayKeys, nameof(processedDayKeys));
            this.pendingEventIds = CopyIds(pendingEventIds, nameof(pendingEventIds));
            this.completedEventIds = CopyIds(completedEventIds, nameof(completedEventIds));
            var completed = new HashSet<string>(this.completedEventIds, StringComparer.Ordinal);
            foreach (var id in this.pendingEventIds)
                if (completed.Contains(id))
                    throw new ArgumentException("Event cannot be both pending and completed.");
        }

        internal static string Required(string value, string name)
        {
            if (string.IsNullOrWhiteSpace(value) || value != value.Trim())
                throw new ArgumentException("A nonempty canonical ID is required.", name);
            return value;
        }

        private static List<string> CopyIds(IEnumerable<string> values, string name)
        {
            return CopyUnique(values, name, x => Required(x, name));
        }

        private static List<T> CopyUnique<T>(IEnumerable<T> values, string name, Func<T, string> id)
        {
            if (values == null) throw new ArgumentNullException(name);
            var result = new List<T>();
            var seen = new HashSet<string>(StringComparer.Ordinal);
            foreach (var item in values)
            {
                if (object.Equals(item, null)) throw new ArgumentException("Null entry.", name);
                string key = Required(id(item), name);
                if (!seen.Add(key)) throw new ArgumentException("Duplicate ID: " + key, name);
                result.Add(item);
            }
            return result;
        }
    }

    public sealed class AcademyMetricsDto
    {
        public int EducationQuality { get; private set; }
        public int StudentSatisfaction { get; private set; }
        public int StaffMorale { get; private set; }
        public int EmployerTrust { get; private set; }
        public int PublicReputation { get; private set; }

        public AcademyMetricsDto(int educationQuality, int studentSatisfaction,
            int staffMorale, int employerTrust, int publicReputation)
        {
            EducationQuality = Bound(educationQuality);
            StudentSatisfaction = Bound(studentSatisfaction);
            StaffMorale = Bound(staffMorale);
            EmployerTrust = Bound(employerTrust);
            PublicReputation = Bound(publicReputation);
        }

        private static int Bound(int value)
        {
            if (value < 0 || value > 100)
                throw new ArgumentOutOfRangeException(nameof(value), "Metric must be 0..100.");
            return value;
        }
    }

    public sealed class RandomStreamSnapshotDto
    {
        public string StreamId { get; private set; }
        public string AlgorithmId { get { return Pcg32.AlgorithmId; } }
        public ulong State { get; private set; }
        public ulong Increment { get; private set; }

        public RandomStreamSnapshotDto(string streamId, ulong state, ulong increment)
        {
            StreamId = GameStateSnapshotDto.Required(streamId, nameof(streamId));
            if ((increment & 1UL) == 0UL)
                throw new ArgumentException("PCG increment must be odd.", nameof(increment));
            State = state;
            Increment = increment;
        }

        public Pcg32 Restore()
        {
            return Pcg32.Restore(State, Increment);
        }
    }
}
