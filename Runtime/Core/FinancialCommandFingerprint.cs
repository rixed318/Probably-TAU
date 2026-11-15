using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace TAU.President.Core
{
    /// <summary>
    /// Deterministic SHA-256 of the financial command's full ordered payload.
    /// Binary format v1: UTF-8 length-prefixed strings and decimal invariant integers,
    /// including command ID, expected revision/day and every ordered posting.
    /// No caller-provided fingerprint is trusted by this helper.
    /// </summary>
    public static class FinancialCommandFingerprint
    {
        public const string Version = "tau-financial-command-v1";

        private static void WriteString(BinaryWriter writer, string value)
        {
            byte[] bytes = Encoding.UTF8.GetBytes(value);
            writer.Write(bytes.Length);
            writer.Write(bytes);
        }

        public static string Compute(string commandId, long expectedRevision, int expectedDayIndex,
            IEnumerable<LedgerEntry> postings)
        {
            GameStateSnapshotDto.Required(commandId, nameof(commandId));
            if (expectedRevision < 0) throw new ArgumentOutOfRangeException(nameof(expectedRevision));
            if (expectedDayIndex < 0) throw new ArgumentOutOfRangeException(nameof(expectedDayIndex));
            if (postings == null) throw new ArgumentNullException(nameof(postings));

            using (var buffer = new MemoryStream())
            {
                using (var writer = new BinaryWriter(buffer, Encoding.UTF8, true))
                {
                    WriteString(writer, Version);
                    WriteString(writer, commandId);
                    WriteString(writer, expectedRevision.ToString(CultureInfo.InvariantCulture));
                    WriteString(writer, expectedDayIndex.ToString(CultureInfo.InvariantCulture));
                    // Snapshot and count first; avoids relying on enumeration stability.
                    var ordered = new List<LedgerEntry>();
                    foreach (var posting in postings)
                    {
                        if (posting == null) throw new ArgumentException("Null posting.", nameof(postings));
                        ordered.Add(posting);
                    }
                    writer.Write(ordered.Count);
                    foreach (var posting in ordered)
                    {
                        WriteString(writer, posting.PostingId);
                        WriteString(writer, posting.SourceId);
                        WriteString(writer, posting.DeltaKopecks.ToString(CultureInfo.InvariantCulture));
                    }
                    writer.Flush();
                }
                using (var sha = SHA256.Create())
                {
                    byte[] digest = sha.ComputeHash(buffer.ToArray());
                    var hex = new StringBuilder(7 + digest.Length * 2);
                    hex.Append("sha256:");
                    foreach (byte b in digest)
                        hex.Append(b.ToString("x2", CultureInfo.InvariantCulture));
                    return hex.ToString();
                }
            }
        }

        public static string Compute(FinancialPostingCommand command)
        {
            if (command == null) throw new ArgumentNullException(nameof(command));
            return Compute(command.CommandId, command.ExpectedRevision,
                command.ExpectedDayIndex, command.Postings);
        }

        /// <summary>Opt-in verified boundary; legacy Execute remains provisional.</summary>
        public static FinancialCommandResult ExecuteVerified(FinancialCommandState state,
            FinancialPostingCommand command)
        {
            if (state == null) throw new ArgumentNullException(nameof(state));
            if (command == null) throw new ArgumentNullException(nameof(command));
            if (!StringComparer.Ordinal.Equals(Compute(command), command.Fingerprint))
                return new FinancialCommandResult(state, null, FinancialCommandStatus.Rejected,
                    "InvalidCommandFingerprint");
            return FinancialCommandProcessor.Execute(state, command);
        }
    }
}
