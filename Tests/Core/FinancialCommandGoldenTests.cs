using System;
using System.Collections.Generic;
using TAU.President.Core;

namespace TAU.President.Tests.Core
{
    public static class FinancialCommandGoldenTests
    {
        private static void Assert(bool value)
        {
            if (!value) throw new Exception("Financial command golden assertion failed.");
        }

        private static FinancialCommandState NewState(long cash = 1000, long revision = 0)
        {
            return new FinancialCommandState(new LedgerSnapshot(cash, new LedgerEntry[0]), revision, 0);
        }

        private static FinancialPostingCommand Command(string id, string fingerprint, long revision,
            params LedgerEntry[] entries)
        {
            return new FinancialPostingCommand(id, fingerprint, revision, 0, entries);
        }

        public static void RunAll()
        {
            var initial = NewState();
            var postings = new List<LedgerEntry> { new LedgerEntry("p1", "source", -300),
                new LedgerEntry("p2", "source", -200) };
            var cmd = new FinancialPostingCommand("c1", "sha256:payload-a", 0, 0, postings);
            postings.Clear();
            Assert(cmd.Postings.Count == 2);
            var applied = FinancialCommandProcessor.Execute(initial, cmd);
            Assert(applied.Status == FinancialCommandStatus.Committed);
            Assert(applied.State.Revision == 1 && applied.State.Ledger.CurrentBalanceKopecks == 500);
            Assert(applied.State.Ledger.Entries.Count == 2 && applied.State.ReceiptCount == 1);
            Assert(initial.Revision == 0 && initial.ReceiptCount == 0 && initial.Ledger.Entries.Count == 0);
            Assert(applied.Receipt.CommittedRevision == 1 && applied.Receipt.BalanceKopecks == 500);

            // Replay must return the same durable receipt even with stale expected revision.
            var replay = FinancialCommandProcessor.Execute(applied.State, cmd);
            Assert(replay.Status == FinancialCommandStatus.Replayed);
            Assert(Object.ReferenceEquals(replay.State, applied.State));
            Assert(Object.ReferenceEquals(replay.Receipt, applied.Receipt));
            var collision = FinancialCommandProcessor.Execute(applied.State,
                Command("c1", "sha256:payload-different", 1, new LedgerEntry("p3", "source", -10)));
            Assert(collision.Status == FinancialCommandStatus.CommandIdCollision && collision.Receipt == null);
            Assert(Object.ReferenceEquals(collision.State, applied.State));

            var stale = FinancialCommandProcessor.Execute(applied.State,
                Command("c2", "sha256:stale", 0, new LedgerEntry("p3", "source", -10)));
            Assert(stale.Status == FinancialCommandStatus.StaleState && stale.State.ReceiptCount == 1);
            var wrongDay = FinancialCommandProcessor.Execute(applied.State,
                new FinancialPostingCommand("c2", "sha256:day", 1, 1,
                    new LedgerEntry[] { new LedgerEntry("p3", "source", -10) }));
            Assert(wrongDay.Status == FinancialCommandStatus.StaleState);

            var duplicate = FinancialCommandProcessor.Execute(applied.State,
                Command("c2", "sha256:dup", 1, new LedgerEntry("p1", "source", -1)));
            Assert(duplicate.Status == FinancialCommandStatus.Rejected && duplicate.State.ReceiptCount == 1);
            var overdraft = FinancialCommandProcessor.Execute(applied.State,
                Command("c2", "sha256:overdraft", 1,
                    new LedgerEntry("p3", "source", -100),
                    new LedgerEntry("p4", "source", -600)));
            Assert(overdraft.Status == FinancialCommandStatus.Rejected);
            Assert(applied.State.Ledger.CurrentBalanceKopecks == 500 && applied.State.Ledger.Entries.Count == 2);
            var validAfterFailure = FinancialCommandProcessor.Execute(applied.State,
                Command("c2", "sha256:valid", 1, new LedgerEntry("p3", "source", -100)));
            Assert(validAfterFailure.Status == FinancialCommandStatus.Committed);
            Assert(validAfterFailure.State.Revision == 2 && validAfterFailure.State.ReceiptCount == 2);
            Assert(validAfterFailure.State.Ledger.CurrentBalanceKopecks == 400);


            // Persisted receipts survive reconstruction; replay never posts twice.
            var exported = validAfterFailure.State.ExportReceipts();
            Assert(exported.Count == 2 && exported[0].CommandId == "c1" &&
                exported[1].CommandId == "c2");
            var restored = FinancialCommandState.Restore(validAfterFailure.State.Ledger,
                validAfterFailure.State.Revision, 0, exported);
            var restoredReplay = FinancialCommandProcessor.Execute(restored, cmd);
            Assert(restoredReplay.Status == FinancialCommandStatus.Replayed);
            Assert(restoredReplay.State.Ledger.Entries.Count == 3 &&
                restoredReplay.State.Ledger.CurrentBalanceKopecks == 400);
            var restoredCollision = FinancialCommandProcessor.Execute(restored,
                Command("c1", "sha256:tampered", 2, new LedgerEntry("p7", "source", -1)));
            Assert(restoredCollision.Status == FinancialCommandStatus.CommandIdCollision);
            Assert(restoredCollision.State.ReceiptCount == 2);
            var shuffled = FinancialCommandState.Restore(validAfterFailure.State.Ledger, 2, 0,
                new FinancialCommandReceipt[] { exported[1], exported[0] });
            Assert(shuffled.ExportReceipts()[0].CommandId == "c1");
            bool invalidReceipt = false;
            try
            {
                FinancialCommandState.Restore(validAfterFailure.State.Ledger, 2, 0,
                    new FinancialCommandReceipt[] { exported[0], exported[0] });
            }
            catch (ArgumentException) { invalidReceipt = true; }
            Assert(invalidReceipt);
            invalidReceipt = false;
            try
            {
                FinancialCommandState.Restore(validAfterFailure.State.Ledger, 2, 0,
                    new FinancialCommandReceipt[] {
                        exported[0], new FinancialCommandReceipt("c2", "sha256:bad", 2, 999) });
            }
            catch (ArgumentException) { invalidReceipt = true; }
            Assert(invalidReceipt);
            invalidReceipt = false;
            try
            {
                FinancialCommandState.Restore(validAfterFailure.State.Ledger, 2, 0,
                    new FinancialCommandReceipt[] {
                        exported[0], new FinancialCommandReceipt("c3", "sha256:bad", 1, 500) });
            }
            catch (ArgumentException) { invalidReceipt = true; }
            Assert(invalidReceipt);

            var max = NewState(long.MaxValue);
            var overflow = FinancialCommandProcessor.Execute(max,
                Command("c3", "sha256:overflow", 0, new LedgerEntry("p5", "source", 1)));
            Assert(overflow.Status == FinancialCommandStatus.Rejected && max.Revision == 0);
            var revisionMax = NewState(1000, long.MaxValue);
            var revisionOverflow = FinancialCommandProcessor.Execute(revisionMax,
                Command("c4", "sha256:revision", long.MaxValue, new LedgerEntry("p6", "source", -1)));
            Assert(revisionOverflow.Status == FinancialCommandStatus.Rejected && revisionMax.ReceiptCount == 0);

            // Canonical fingerprint v1: exact ordered payload, not a caller assertion.
            var canonicalPostings = new LedgerEntry[] {
                new LedgerEntry("verified-p1", "s1", -50),
                new LedgerEntry("verified-p2", "s2", 10) };
            string canonical = FinancialCommandFingerprint.Compute("verified-c1", 0, 0, canonicalPostings);
            Assert(canonical.StartsWith("sha256:") && canonical.Length == 71);
            Assert(canonical == FinancialCommandFingerprint.Compute("verified-c1", 0, 0, canonicalPostings));
            Assert(canonical != FinancialCommandFingerprint.Compute("verified-c1", 0, 0,
                new LedgerEntry[] { canonicalPostings[1], canonicalPostings[0] }));
            Assert(canonical != FinancialCommandFingerprint.Compute("verified-c1", 0, 0,
                new LedgerEntry[] { new LedgerEntry("verified-p1", "s1", -51), canonicalPostings[1] }));
            Assert(canonical != FinancialCommandFingerprint.Compute("verified-c1", 1, 0, canonicalPostings));
            Assert(canonical != FinancialCommandFingerprint.Compute("verified-c2", 0, 0, canonicalPostings));
            // Length-prefixing must disambiguate adjacent field boundaries and preserve Unicode.
            string boundaryA = FinancialCommandFingerprint.Compute("a", 0, 0,
                new LedgerEntry[] { new LedgerEntry("bc", "d", 1) });
            string boundaryB = FinancialCommandFingerprint.Compute("ab", 0, 0,
                new LedgerEntry[] { new LedgerEntry("c", "d", 1) });
            Assert(boundaryA != boundaryB);
            Assert(FinancialCommandFingerprint.Compute("unicode-ё", 0, 0, canonicalPostings) ==
                FinancialCommandFingerprint.Compute("unicode-ё", 0, 0, canonicalPostings));
            Assert(FinancialCommandFingerprint.Compute("unicode-ё", 0, 0, canonicalPostings) !=
                FinancialCommandFingerprint.Compute("unicode-е", 0, 0, canonicalPostings));
            // A player's machine locale must not alter persisted command identity.
            var previousCulture = System.Globalization.CultureInfo.CurrentCulture;
            try
            {
                foreach (string cultureName in new[] { "fr-FR", "tr-TR", "ar-SA" })
                {
                    System.Globalization.CultureInfo.CurrentCulture =
                        System.Globalization.CultureInfo.GetCultureInfo(cultureName);
                    Assert(FinancialCommandFingerprint.Compute("verified-c1", 0, 0,
                        canonicalPostings) == canonical);
                    Assert(FinancialCommandFingerprint.Compute("locale-negative", 17, 3,
                        new LedgerEntry[] { new LedgerEntry("locale-p", "locale-s", -123456789) }) ==
                        FinancialCommandFingerprint.Compute("locale-negative", 17, 3,
                            new LedgerEntry[] { new LedgerEntry("locale-p", "locale-s", -123456789) }));
                }
            }
            finally { System.Globalization.CultureInfo.CurrentCulture = previousCulture; }
            var verified = new FinancialPostingCommand("verified-c1", canonical, 0, 0, canonicalPostings);
            var verifiedResult = FinancialCommandFingerprint.ExecuteVerified(initial, verified);
            Assert(verifiedResult.Status == FinancialCommandStatus.Committed);
            Assert(verifiedResult.State.Ledger.CurrentBalanceKopecks == 960);
            var verifiedReplay = FinancialCommandFingerprint.ExecuteVerified(verifiedResult.State, verified);
            Assert(verifiedReplay.Status == FinancialCommandStatus.Replayed);
            var forged = new FinancialPostingCommand("verified-c1", canonical, 0, 0,
                new LedgerEntry[] { new LedgerEntry("verified-p1", "s1", -51), canonicalPostings[1] });
            var forgedResult = FinancialCommandFingerprint.ExecuteVerified(verifiedResult.State, forged);
            Assert(forgedResult.Status == FinancialCommandStatus.Rejected &&
                forgedResult.Error == "InvalidCommandFingerprint" &&
                Object.ReferenceEquals(forgedResult.State, verifiedResult.State));
            Assert(verifiedResult.State.Ledger.CurrentBalanceKopecks == 960);
            bool nullRejected = false;
            try { FinancialCommandFingerprint.Compute("c", 0, 0, new LedgerEntry[] { null }); }
            catch (ArgumentException) { nullRejected = true; }
            Assert(nullRejected);

            // Invalid UTF-16 must not be silently normalized to U+FFFD in a command ID,
            // source ID or posting ID; otherwise distinct payloads can share a hash.
            string malformed = "bad" + (char)0xD800;
            bool encodingRejected = false;
            try { FinancialCommandFingerprint.Compute(malformed, 0, 0, canonicalPostings); }
            catch (System.Text.EncoderFallbackException) { encodingRejected = true; }
            Assert(encodingRejected);
            encodingRejected = false;
            try { FinancialCommandFingerprint.Compute("ok", 0, 0,
                new LedgerEntry[] { new LedgerEntry("p", malformed, 1) }); }
            catch (System.Text.EncoderFallbackException) { encodingRejected = true; }
            Assert(encodingRejected);
            // Posting IDs also participate in strict UTF-8 hashing. A malformed source
            // must fail closed at the verified execution boundary without any ledger write.
            encodingRejected = false;
            try { FinancialCommandFingerprint.Compute("ok", 0, 0,
                new LedgerEntry[] { new LedgerEntry(malformed, "source", 1) }); }
            catch (System.Text.EncoderFallbackException) { encodingRejected = true; }
            Assert(encodingRejected);
            // A malformed posting ID must also fail closed at the execution boundary.
            var malformedPostingCommand = new FinancialPostingCommand("verified-bad-posting", canonical,
                0, 0, new LedgerEntry[] { new LedgerEntry(malformed, "source", 1) });
            var malformedPostingResult = FinancialCommandFingerprint.ExecuteVerified(initial, malformedPostingCommand);
            Assert(malformedPostingResult.Status == FinancialCommandStatus.Rejected &&
                malformedPostingResult.Error == "InvalidCommandEncoding" &&
                Object.ReferenceEquals(malformedPostingResult.State, initial));
            Assert(initial.ReceiptCount == 0 && initial.Ledger.Entries.Count == 0);
            var malformedSourceCommand = new FinancialPostingCommand("verified-bad-source", canonical,
                0, 0, new LedgerEntry[] { new LedgerEntry("p", malformed, 1) });
            var malformedSourceResult = FinancialCommandFingerprint.ExecuteVerified(initial, malformedSourceCommand);
            Assert(malformedSourceResult.Status == FinancialCommandStatus.Rejected &&
                malformedSourceResult.Error == "InvalidCommandEncoding" &&
                Object.ReferenceEquals(malformedSourceResult.State, initial));
            Assert(initial.ReceiptCount == 0 && initial.Ledger.Entries.Count == 0);
            var malformedCommand = new FinancialPostingCommand(malformed, canonical, 0, 0, canonicalPostings);
            var malformedResult = FinancialCommandFingerprint.ExecuteVerified(initial, malformedCommand);
            Assert(malformedResult.Status == FinancialCommandStatus.Rejected &&
                malformedResult.Error == "InvalidCommandEncoding" &&
                Object.ReferenceEquals(malformedResult.State, initial));
            Assert(initial.ReceiptCount == 0 && initial.Ledger.Entries.Count == 0);

            var empty = FinancialCommandProcessor.Execute(initial,
                Command("empty", "sha256:empty", 0));
            Assert(empty.Status == FinancialCommandStatus.Committed && empty.State.Revision == 1);
            Assert(empty.State.Ledger.CurrentBalanceKopecks == 1000);
        }
    }
}
