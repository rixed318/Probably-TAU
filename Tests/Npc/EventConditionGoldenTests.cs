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
        }
        private static void Assert(bool value) { if (!value) throw new Exception("NPC condition assertion failed."); }
        private static void Throws<T>(Action action) where T : Exception
        {
            try { action(); } catch (T) { return; }
            throw new Exception("Expected " + typeof(T).Name);
        }
    }
}
