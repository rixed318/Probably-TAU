using System;
using System.Collections.Generic;

namespace TAU.President.Npc
{
    /// <summary>Immutable proposed condition AST; no Core, Unity or JSON dependencies.</summary>
    public sealed class EventCondition
    {
        public string Op { get; private set; }
        public string Key { get; private set; }
        public string Value { get; private set; }
        public int? DayIndex { get; private set; }
        public IReadOnlyList<EventCondition> Items { get; private set; }

        private EventCondition(string op, string key, string value, int? dayIndex,
            IEnumerable<EventCondition> items)
        {
            Op = op;
            Key = key;
            Value = value;
            DayIndex = dayIndex;
            Items = new List<EventCondition>(items).AsReadOnly();
        }

        public static EventCondition Always() { return new EventCondition("always", null, null, null, new EventCondition[0]); }
        public static EventCondition FlagEquals(string key, string value)
        {
            return new EventCondition("flagEquals", Required(key), Required(value), null, new EventCondition[0]);
        }
        public static EventCondition FactExists(string factId)
        {
            return new EventCondition("factExists", Required(factId), null, null, new EventCondition[0]);
        }
        public static EventCondition DayIndexAtLeast(int dayIndex)
        {
            if (dayIndex < 0) throw new ArgumentOutOfRangeException(nameof(dayIndex));
            return new EventCondition("dayIndexAtLeast", null, null, dayIndex, new EventCondition[0]);
        }
        public static EventCondition All(params EventCondition[] items) { return Group("all", items); }
        public static EventCondition Any(params EventCondition[] items) { return Group("any", items); }
        private static EventCondition Group(string op, EventCondition[] items)
        {
            if (items == null || items.Length == 0) throw new ArgumentException("Group must be nonempty.", nameof(items));
            foreach (var item in items)
                if (item == null) throw new ArgumentException("Null child.", nameof(items));
            return new EventCondition(op, null, null, null, items);
        }
        private static string Required(string value)
        {
            if (string.IsNullOrWhiteSpace(value) || value != value.Trim() || HasControl(value))
                throw new ArgumentException("Nonempty canonical value required.");
            return value;
        }
        private static bool HasControl(string value)
        {
            foreach (char ch in value)
                if (char.IsControl(ch)) return true;
            return false;
        }
    }

    /// <summary>Pure read-only evaluator for Content EVENT_HOOKS_DRAFT operators.</summary>
    public static class EventConditionEvaluator
    {
        public static bool Evaluate(EventCondition condition, int currentDayIndex,
            IReadOnlyDictionary<string, string> flags, ISet<string> facts)
        {
            if (condition == null) throw new ArgumentNullException(nameof(condition));
            if (currentDayIndex < 0) throw new ArgumentOutOfRangeException(nameof(currentDayIndex));
            if (flags == null) throw new ArgumentNullException(nameof(flags));
            if (facts == null) throw new ArgumentNullException(nameof(facts));
            return EvaluateNode(condition, currentDayIndex, flags, facts);
        }

        private static bool EvaluateNode(EventCondition node, int day,
            IReadOnlyDictionary<string, string> flags, ISet<string> facts)
        {
            switch (node.Op)
            {
                case "always": return true;
                case "flagEquals":
                    foreach (var entry in flags)
                        if (string.Equals(entry.Key, node.Key, StringComparison.Ordinal))
                            return string.Equals(entry.Value, node.Value, StringComparison.Ordinal);
                    return false;
                case "factExists":
                    foreach (string fact in facts)
                        if (string.Equals(fact, node.Key, StringComparison.Ordinal)) return true;
                    return false;
                case "dayIndexAtLeast": return day >= node.DayIndex.Value;
                case "all":
                    foreach (var child in node.Items)
                        if (!EvaluateNode(child, day, flags, facts)) return false;
                    return true;
                case "any":
                    foreach (var child in node.Items)
                        if (EvaluateNode(child, day, flags, facts)) return true;
                    return false;
                default: throw new InvalidOperationException("Unknown condition operator.");
            }
        }
    }
}
