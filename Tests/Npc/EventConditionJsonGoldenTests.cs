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
            // Regression for CI #43659607524: parser-level errors must honor the
            // public FormatException contract and preserve the JSON root cause.
            ThrowsWithJsonCause("{");
            ThrowsWithJsonCause("{\"op\":\"always\"} trailing");
            ThrowsWithJsonCause("{\"op\":");

            // Draft Content PR #11 has mutually exclusive partner revenue paths.
            // The adapter must not silently accept the wrong branch or casing.
            string remote = "{\"op\":\"all\",\"items\":[{\"op\":\"flagEquals\",\"key\":\"PARTNER\",\"value\":\"remote\"},{\"op\":\"factExists\",\"factId\":\"E03_mentor_paid\"},{\"op\":\"factExists\",\"factId\":\"E07_delivered\"},{\"op\":\"dayIndexAtLeast\",\"value\":4}]}";
            var remoteCondition = EventConditionJsonAdapter.Parse(remote);
            facts.Add("E07_delivered");
            Assert(!EventConditionEvaluator.Evaluate(remoteCondition, 4, flags, facts));
            flags["PARTNER"] = "remote";
            Assert(EventConditionEvaluator.Evaluate(remoteCondition, 4, flags, facts));
            Assert(!EventConditionEvaluator.Evaluate(condition, 4, flags, facts));
            flags["PARTNER"] = "Remote";
            Assert(!EventConditionEvaluator.Evaluate(remoteCondition, 4, flags, facts));
            flags["PARTNER"] = "remote";
            Assert(!EventConditionEvaluator.Evaluate(remoteCondition, 3, flags, facts));
            Assert(EventConditionEvaluator.Evaluate(
                EventConditionJsonAdapter.Parse("{\"op\":\"dayIndexAtLeast\",\"value\":2147483647}"),
                int.MaxValue, flags, facts));
            Throws<FormatException>(() => EventConditionJsonAdapter.Parse(
                "{\"op\":\"all\",\"items\":[{\"op\":\"always\",\"op\":\"always\"}]}"));

            // Escaped JSON field aliases are decoded before duplicate/allowlist checks.
            Throws<FormatException>(() => EventConditionJsonAdapter.Parse(
                "{\"op\":\"always\",\"\\u006fp\":\"always\"}"));
            Throws<FormatException>(() => EventConditionJsonAdapter.Parse(
                "{\"op\":\"always\",\"Op\":\"always\"}"));
            Throws<FormatException>(() => EventConditionJsonAdapter.Parse(
                "{\"op\":\"factExists\",\"factId\":\"E07_delivered\",\"\\u0066actId\":\"E07_delivered\"}"));
            Assert(EventConditionEvaluator.Evaluate(
                EventConditionJsonAdapter.Parse("{\"\\u006fp\":\"always\"}"),
                0, flags, facts));

            // Missing required fields must fail with FormatException, not KeyNotFoundException.
            Throws<FormatException>(() => EventConditionJsonAdapter.Parse("{}"));
            Throws<FormatException>(() => EventConditionJsonAdapter.Parse("{\"key\":\"PARTNER\"}"));
            Throws<FormatException>(() => EventConditionJsonAdapter.Parse("{\"op\":\"flagEquals\",\"value\":\"onsite\"}"));
            Throws<FormatException>(() => EventConditionJsonAdapter.Parse("{\"op\":\"flagEquals\",\"key\":\"PARTNER\"}"));
            Throws<FormatException>(() => EventConditionJsonAdapter.Parse("{\"op\":\"factExists\"}"));
            Throws<FormatException>(() => EventConditionJsonAdapter.Parse("{\"op\":\"dayIndexAtLeast\"}"));
            Throws<FormatException>(() => EventConditionJsonAdapter.Parse("{\"op\":\"all\"}"));

            // JSON control characters must not enter persistent condition IDs.
            Throws<FormatException>(() => EventConditionJsonAdapter.Parse("{\"op\":\"factExists\",\"factId\":\"bad\\nkey\"}"));
            Throws<FormatException>(() => EventConditionJsonAdapter.Parse("{\"op\":\"flagEquals\",\"key\":\"PARTNER\",\"value\":\"onsite\\tvalue\"}"));

            // Escaped control characters must be rejected after JSON unescaping,
            // including NUL and a control character in the operator name.
            Throws<FormatException>(() => EventConditionJsonAdapter.Parse(
                "{\\\"op\\\":\\\"factExists\\\",\\\"factId\\\":\\\"E03\\\\u0000mentor\\\"}"));
            Throws<FormatException>(() => EventConditionJsonAdapter.Parse(
                "{\\\"op\\\":\\\"flag\\\\u000aEquals\\\",\\\"key\\\":\\\"PARTNER\\\",\\\"value\\\":\\\"onsite\\\"}"));
            Throws<FormatException>(() => EventConditionJsonAdapter.Parse(
                "{\\\"op\\\":\\\"flagEquals\\\",\\\"key\\\":\\\"PARTNER\\\",\\\"value\\\":\\\"on\\\\u007fsite\\\"}"));

            // A deeply nested untrusted tree must fail before stack exhaustion.
            string deep = "{\"op\":\"always\"}";
            for (int i = 0; i < 33; i++) deep = "{\"op\":\"all\",\"items\":[" + deep + "]}";
            Throws<FormatException>(() => EventConditionJsonAdapter.Parse(deep));
        }
        private static void ThrowsWithJsonCause(string json)
        {
            try { EventConditionJsonAdapter.Parse(json); }
            catch (FormatException ex)
            {
                Assert(ex.InnerException is System.Text.Json.JsonException);
                return;
            }
            throw new Exception("Expected FormatException with JsonException cause.");
        }
        private static void Assert(bool ok) { if (!ok) throw new Exception("NPC JSON condition assertion failed."); }
        private static void Throws<T>(Action action) where T : Exception
        {
            try { action(); } catch (T) { return; }
            throw new Exception("Expected " + typeof(T).Name);
        }
    }
}
