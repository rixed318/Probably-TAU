using System;
using System.Collections.Generic;
using TAU.President.Npc;

namespace TAU.President.Tests.Npc
{
    /// <summary>Standalone assertions; invoke RunAll from a real C# runner when available.</summary>
    public static class EventConditionGoldenTests
    {
        public static void RunAll()
        {
            var flags = new Dictionary<string, string>(StringComparer.Ordinal) { { "PARTNER", "onsite" } };
            var facts = new HashSet<string>(StringComparer.Ordinal) { "E03_mentor_paid", "E07_delivered" };
            var onsite = EventCondition.All(
                EventCondition.FlagEquals("PARTNER", "onsite"),
                EventCondition.FactExists("E03_mentor_paid"),
                EventCondition.FactExists("E07_delivered"),
                EventCondition.DayIndexAtLeast(4));
            var remote = EventCondition.FlagEquals("PARTNER", "remote");
            Assert(EventConditionEvaluator.Evaluate(EventCondition.Always(), 0, flags, facts));
            Assert(!EventConditionEvaluator.Evaluate(onsite, 3, flags, facts));
            Assert(EventConditionEvaluator.Evaluate(onsite, 4, flags, facts));
            Assert(!EventConditionEvaluator.Evaluate(remote, 4, flags, facts));
            Assert(EventConditionEvaluator.Evaluate(EventCondition.Any(remote, onsite), 4, flags, facts));
            Assert(!EventConditionEvaluator.Evaluate(EventCondition.FactExists("missing"), 4, flags, facts));
            Assert(!EventConditionEvaluator.Evaluate(EventCondition.FlagEquals("partner", "onsite"), 4, flags, facts));
            Assert(!EventConditionEvaluator.Evaluate(EventCondition.FlagEquals("PARTNER", "ONSITE"), 4, flags, facts));
            facts.Remove("E07_delivered");
            Assert(!EventConditionEvaluator.Evaluate(onsite, 4, flags, facts));
            Assert(EventConditionEvaluator.Evaluate(EventCondition.Any(remote, EventCondition.Always()), 4, flags, facts));
            Throws<ArgumentException>(() => EventCondition.All());
            Throws<ArgumentException>(() => EventCondition.Any((EventCondition)null));
            Throws<ArgumentException>(() => EventCondition.FactExists(" "));
            Throws<ArgumentOutOfRangeException>(() => EventCondition.DayIndexAtLeast(-1));
            Throws<ArgumentOutOfRangeException>(() => EventConditionEvaluator.Evaluate(onsite, -1, flags, facts));
            Throws<ArgumentNullException>(() => EventConditionEvaluator.Evaluate(null, 0, flags, facts));

            // Regression: nested predicates must be stable at the exact DayIndex boundary.
            var nested = EventCondition.All(
                EventCondition.Any(
                    EventCondition.FlagEquals("PARTNER", "remote"),
                    EventCondition.FlagEquals("PARTNER", "onsite")),
                EventCondition.DayIndexAtLeast(4),
                EventCondition.FactExists("E03_mentor_paid"));
            Assert(!EventConditionEvaluator.Evaluate(nested, 3, flags, facts));
            Assert(EventConditionEvaluator.Evaluate(nested, 4, flags, facts));
            Assert(EventConditionEvaluator.Evaluate(nested, int.MaxValue, flags, facts));

            // The group owns a copy of its input array and exposes no mutable children.
            var children = new[] { EventCondition.Always() };
            var frozen = EventCondition.All(children);
            children[0] = EventCondition.FlagEquals("PARTNER", "remote");
            Assert(EventConditionEvaluator.Evaluate(frozen, 0, flags, facts));
            Throws<NotSupportedException>(() =>
                ((IList<EventCondition>)frozen.Items)[0] = EventCondition.FlagEquals("PARTNER", "remote"));

            // Evaluating never mutates caller-owned fact/flag collections.
            int factCount = facts.Count, flagCount = flags.Count;
            Assert(EventConditionEvaluator.Evaluate(nested, 4, flags, facts));
            Assert(facts.Count == factCount && flags.Count == flagCount);
            Assert(flags["PARTNER"] == "onsite" && facts.Contains("E03_mentor_paid"));

            // Caller-provided case-insensitive containers must not change wire-ID semantics.
            var insensitiveFlags = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
                { { "PARTNER", "onsite" } };
            var insensitiveFacts = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
                { "E03_mentor_paid" };
            Assert(EventConditionEvaluator.Evaluate(EventCondition.FlagEquals("PARTNER", "onsite"),
                4, insensitiveFlags, insensitiveFacts));
            Assert(!EventConditionEvaluator.Evaluate(EventCondition.FlagEquals("partner", "onsite"),
                4, insensitiveFlags, insensitiveFacts));
            Assert(EventConditionEvaluator.Evaluate(EventCondition.FactExists("E03_mentor_paid"),
                4, insensitiveFlags, insensitiveFacts));
            Assert(!EventConditionEvaluator.Evaluate(EventCondition.FactExists("e03_mentor_paid"),
                4, insensitiveFlags, insensitiveFacts));

            // Boolean groups must preserve false outcomes across missing facts and
            // mutually exclusive partner choices, including nested combinations.
            var absent = EventCondition.FactExists("NEVER_EARNED");
            var trueBranch = EventCondition.FlagEquals("PARTNER", "onsite");
            Assert(!EventConditionEvaluator.Evaluate(EventCondition.All(trueBranch, absent),
                4, flags, facts));
            Assert(!EventConditionEvaluator.Evaluate(EventCondition.Any(absent,
                EventCondition.FlagEquals("PARTNER", "remote")), 4, flags, facts));
            Assert(EventConditionEvaluator.Evaluate(EventCondition.Any(absent, trueBranch),
                4, flags, facts));
            Assert(!EventConditionEvaluator.Evaluate(EventCondition.All(
                EventCondition.Any(absent, trueBranch), absent), 4, flags, facts));

            // Direct AST construction must enforce the same ID rules as JSON parsing.
            Throws<ArgumentException>(() => EventCondition.FactExists("E03\nmentor"));
            Throws<ArgumentException>(() => EventCondition.FlagEquals("PARTNER\t", "onsite"));
            Throws<ArgumentException>(() => EventCondition.FlagEquals("PARTNER", "on\u0000site"));
            Assert(EventConditionEvaluator.Evaluate(EventCondition.FactExists("E03_mentor_paid"),
                4, insensitiveFlags, insensitiveFacts));

            // UTF-16 identifiers must not contain orphan surrogates; valid pairs remain legal.
            Throws<ArgumentException>(() => EventCondition.FactExists("bad\uD800"));
            Throws<ArgumentException>(() => EventCondition.FlagEquals("PARTNER", "bad\uDC00"));
            string paired = "fact_\uD83D\uDE00";
            Assert(EventConditionEvaluator.Evaluate(EventCondition.FactExists(paired), 4,
                flags, new HashSet<string>(StringComparer.Ordinal) { paired }));

            // Fail closed on absent context, malformed groups, and noncanonical identifiers.
            Throws<ArgumentNullException>(() => EventConditionEvaluator.Evaluate(onsite, 4, null, facts));
            Throws<ArgumentNullException>(() => EventConditionEvaluator.Evaluate(onsite, 4, flags, null));
            Throws<ArgumentException>(() => EventCondition.All((EventCondition[])null));
            Throws<ArgumentException>(() => EventCondition.FlagEquals(" PARTNER", "onsite"));
            Throws<ArgumentException>(() => EventCondition.FlagEquals("PARTNER", "onsite "));

        }
        private static void Assert(bool value) { if (!value) throw new Exception("NPC condition assertion failed."); }
        private static void Throws<T>(Action action) where T : Exception
        {
            try { action(); } catch (T) { return; }
            throw new Exception("Expected " + typeof(T).Name);
        }
    }
}
