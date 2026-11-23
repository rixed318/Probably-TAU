using System;
using System.Collections.Generic;
using System.Text.Json;

namespace TAU.President.Npc
{
    /// <summary>
    /// Strict draft Content JSON condition adapter for offline .NET tests.
    /// Not a Unity/runtime serialization contract; no Core state or effects.
    /// </summary>
    public static class EventConditionJsonAdapter
    {
        private const int MaxDepth = 32;

        public static EventCondition Parse(string json)
        {
            if (json == null) throw new ArgumentNullException(nameof(json));
            try
            {
                using (var document = JsonDocument.Parse(json))
                    return ReadNode(document.RootElement, 0);
            }
            catch (JsonException exception)
            {
                // System.Text.Json may reject excessive depth before ReadNode's own
                // depth guard runs. Keep all invalid JSON shapes on the same API contract.
                throw new FormatException("Invalid condition JSON.", exception);
            }
            catch (ArgumentException exception)
            {
                // The immutable AST factories validate canonical identifiers.
                // Invalid content JSON must not leak factory-specific exceptions.
                throw new FormatException("Invalid condition JSON value.", exception);
            }
        }

        private static EventCondition ReadNode(JsonElement node, int depth)
        {
            if (depth >= MaxDepth) throw new FormatException("Condition nesting exceeds 32 levels.");
            if (node.ValueKind != JsonValueKind.Object)
                throw new FormatException("Condition must be a JSON object.");
            string op = ReadString(node, "op");
            switch (op)
            {
                case "always":
                    RequireFields(node, "op");
                    return EventCondition.Always();
                case "flagEquals":
                    RequireFields(node, "op", "key", "value");
                    return EventCondition.FlagEquals(ReadString(node, "key"), ReadString(node, "value"));
                case "factExists":
                    RequireFields(node, "op", "factId");
                    return EventCondition.FactExists(ReadString(node, "factId"));
                case "dayIndexAtLeast":
                    RequireFields(node, "op", "value");
                    JsonElement day = node.GetProperty("value");
                    int dayIndex;
                    if (day.ValueKind != JsonValueKind.Number || !day.TryGetInt32(out dayIndex) || dayIndex < 0)
                        throw new FormatException("DayIndex must be a nonnegative Int32.");
                    return EventCondition.DayIndexAtLeast(dayIndex);
                case "all":
                case "any":
                    RequireFields(node, "op", "items");
                    JsonElement items = node.GetProperty("items");
                    if (items.ValueKind != JsonValueKind.Array || items.GetArrayLength() == 0)
                        throw new FormatException("Condition group must have nonempty items.");
                    var children = new List<EventCondition>();
                    foreach (var item in items.EnumerateArray())
                        children.Add(ReadNode(item, depth + 1));
                    return op == "all" ? EventCondition.All(children.ToArray()) : EventCondition.Any(children.ToArray());
                default:
                    throw new FormatException("Unknown condition op: " + op);
            }
        }

        private static string ReadString(JsonElement node, string name)
        {
            JsonElement value;
            if (!node.TryGetProperty(name, out value))
                throw new FormatException("Missing required condition field: " + name);
            if (value.ValueKind != JsonValueKind.String)
                throw new FormatException(name + " must be a string.");
            string result = value.GetString();
            if (string.IsNullOrWhiteSpace(result) || result != result.Trim() || HasControlCharacters(result))
                throw new FormatException(name + " must be a nonempty canonical string.");
            return result;
        }

        private static bool HasControlCharacters(string value)
        {
            foreach (char ch in value)
                if (char.IsControl(ch)) return true;
            return false;
        }

        private static void RequireFields(JsonElement node, params string[] expected)
        {
            var allowed = new HashSet<string>(expected, StringComparer.Ordinal);
            var seen = new HashSet<string>(StringComparer.Ordinal);
            foreach (var field in node.EnumerateObject())
            {
                if (!allowed.Contains(field.Name) || !seen.Add(field.Name))
                    throw new FormatException("Unexpected or duplicate condition field: " + field.Name);
            }
            if (seen.Count != allowed.Count)
                throw new FormatException("Missing required condition field.");
        }
    }
}
