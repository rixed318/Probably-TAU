using System;
using System.Collections.Generic;
using TAU.President.Npc;

namespace TAU.President.Tests.Npc
{
    public static class EventConditionJsonGoldenTests
    {
        public static void RunAll()
        {
            var flags = new Dictionary<string, string>(StringComparer.Ordinal) { { "PARTNER", "onsite" } };
            var facts = new HashSet<string>(StringComparer.Ordinal) { "E03_mentor_paid", "E07_delivered" };
            string onsite = "{\"op\":\"all\",\"items\":[{\"op\":\"flagEquals\",\"key\":\"PARTNER\",\"value\":\"onsite\"},{\"op\":\"factExists\",\"factId\":\"E03_mentor_paid\"},{\"op\":\"factExists\",\"factId\":\"E07_delivered\"},{\"op\":\"dayIndexAtLeast\",\"value\":4}]}";
            var condition = EventConditionJsonAdapter.Parse(onsite);
            Assert(!EventConditionEvaluator.Evaluate(condition, 3, flags, facts));
            Assert(EventConditionEvaluator.Evaluate(condition, 4, flags, facts));
            facts.Remove("E07_delivered");
            Assert(!EventConditionEvaluator.Evaluate(condition, 4, flags, facts));
            Assert(EventConditionEvaluator.Evaluate(EventConditionJsonAdapter.Parse("{\"op\":\"any\",\"items\":[{\"op\":\"always\"}]}"), 0, flags, facts));
            Throws<FormatException>(() => EventConditionJsonAdapter.Parse("{\"op\":\"unknown\"}"));
            Throws<FormatException>(() => EventConditionJsonAdapter.Parse("{\"op\":\"always\",\"value\":1}"));
            Throws<FormatException>(() => EventConditionJsonAdapter.Parse("{\"op\":\"always\",\"op\":\"always\"}"));
            Throws<FormatException>(() => EventConditionJsonAdapter.Parse("{\"op\":\"all\",\"items\":[]}"));
            Throws<FormatException>(() => EventConditionJsonAdapter.Parse("{\"op\":\"dayIndexAtLeast\",\"value\":-1}"));
            Throws<FormatException>(() => EventConditionJsonAdapter.Parse("{\"op\":\"dayIndexAtLeast\",\"value\":2147483648}"));
            Throws<FormatException>(() => EventConditionJsonAdapter.Parse("{\"op\":\"dayIndexAtLeast\",\"value\":4.5}"));
            Throws<FormatException>(() => EventConditionJsonAdapter.Parse("{\"op\":\"factExists\",\"factId\":null}"));
            Throws<FormatException>(() => EventConditionJsonAdapter.Parse("{\"op\":\"flagEquals\",\"key\":\" PARTNER\",\"value\":\"onsite\"}"));
            Throws<FormatException>(() => EventConditionJsonAdapter.Parse("[]"));
            Throws<ArgumentNullException>(() => EventConditionJsonAdapter.Parse(null));
            // A deeply nested untrusted tree must fail before stack exhaustion.
            string deep = "{\"op\":\"always\"}";
            for (int i = 0; i < 33; i++) deep = "{\"op\":\"all\",\"items\":[" + deep + "]}";
            Throws<FormatException>(() => EventConditionJsonAdapter.Parse(deep));
        }
        private static void Assert(bool ok) { if (!ok) throw new Exception("NPC JSON condition assertion failed."); }
        private static void Throws<T>(Action action) where T : Exception
        {
            try { action(); } catch (T) { return; }
            throw new Exception("Expected " + typeof(T).Name);
        }
    }
}
