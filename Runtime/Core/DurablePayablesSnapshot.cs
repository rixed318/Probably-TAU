using System;
using System.Collections.Generic;

namespace TAU.President.Core
{
    /// <summary>Unpaid mandatory expense, in positive Int64 kopecks.</summary>
    public sealed class DurablePayable
    {
        public string DueKey { get; private set; }
        public int DueDayIndex { get; private set; }
        public long AmountKopecks { get; private set; }

        public DurablePayable(string dueKey, int dueDayIndex, long amountKopecks)
        {
            DueKey = GameStateSnapshotDto.Required(dueKey, nameof(dueKey));
            if (dueDayIndex < 0) throw new ArgumentOutOfRangeException(nameof(dueDayIndex));
            if (amountKopecks <= 0) throw new ArgumentOutOfRangeException(nameof(amountKopecks));
            DueDayIndex = dueDayIndex;
            AmountKopecks = amountKopecks;
        }
    }

    /// <summary>Deterministic outstanding liabilities; no payment or ledger side effects.</summary>
    public sealed class DurablePayablesSnapshot
    {
        private readonly List<DurablePayable> outstanding;
        public IReadOnlyList<DurablePayable> Outstanding { get { return outstanding.AsReadOnly(); } }

        public DurablePayablesSnapshot(IEnumerable<DurablePayable> payables)
        {
            if (payables == null) throw new ArgumentNullException(nameof(payables));
            outstanding = new List<DurablePayable>();
            var keys = new HashSet<string>(StringComparer.Ordinal);
            foreach (var payable in payables)
            {
                if (payable == null) throw new ArgumentException("Null payable.", nameof(payables));
                if (!keys.Add(payable.DueKey))
                    throw new ArgumentException("Duplicate payable due key.", nameof(payables));
                outstanding.Add(payable);
            }
            outstanding.Sort((a, b) =>
            {
                int order = a.DueDayIndex.CompareTo(b.DueDayIndex);
                return order != 0 ? order : StringComparer.Ordinal.Compare(a.DueKey, b.DueKey);
            });
        }

        public DurablePayablesSnapshot Restore()
        {
            return new DurablePayablesSnapshot(outstanding);
        }
    }
}
