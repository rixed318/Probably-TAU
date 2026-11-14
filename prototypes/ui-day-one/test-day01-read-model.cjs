"use strict";
const test = require("node:test");
const assert = require("node:assert/strict");
const { model, formatRub, getDocument, getSummary } = require("./day01-read-model.cjs");

test("canonical DayIndex and five C0 metrics", () => {
  assert.equal(model.dayIndex, 0);
  assert.equal(model.dayNumber, 1);
  assert.deepEqual(model.metrics, { educationQuality: 52, studentSatisfaction: 48, staffMorale: 50, employerTrust: 45, publicReputation: 47 });
  assert.equal(model.openingBalanceKopecks, "200000000");
  assert.equal(getSummary().openingBalance, "2 000 000 ₽");
});

test("three distinct documents and visits, same requested room slot", () => {
  assert.deepEqual(model.documents.map(x => x.id), ["DOC-01", "DOC-02", "DOC-03"]);
  assert.equal(new Set(model.documents.map(x => x.visitId)).size, 3);
  assert.equal(new Set(model.documents.map(x => x.npcId)).size, 3);
  for (const doc of model.documents) {
    assert.equal(doc.room, "Л-204");
    assert.equal(doc.slotDayIndex, 3);
    assert.equal(doc.slot, "14:00-17:00");
  }
  assert.equal(getDocument("unknown"), null);
  assert.equal(getDocument("DOC-02").roomStatus, "request-not-booking");
});

test("exact content amounts and inspection alternatives", () => {
  assert.equal(getDocument("DOC-01").amountKopecks, "22000000");
  assert.equal(getDocument("DOC-01").revealed.realSeats, 20);
  assert.equal(getDocument("DOC-01").revealed.phasedPriceKopecks, "16000000");
  assert.equal(getDocument("DOC-02").amountKopecks, "6500000");
  assert.equal(getDocument("DOC-02").revealed.mobilePriceKopecks, "3500000");
  assert.equal(getDocument("DOC-03").conditionalRevenueKopecks, "12000000");
  assert.equal(getDocument("DOC-03").revealed.remoteConditionalRevenueKopecks, "9000000");
});

test("partner offer does not credit revenue on signing", () => {
  const partner = getDocument("DOC-03");
  assert.equal(partner.recognizedRevenueKopecks, "0");
  assert.equal(partner.revenueEarliestDayIndex, 4);
  assert.equal(getSummary().openingBalance, "2 000 000 ₽");
});

test("money display uses BigInt and validates canonical Int64 strings", () => {
  assert.equal(formatRub("22000000"), "220 000 ₽");
  assert.equal(formatRub("123456789"), "1 234 567,89 ₽");
  assert.equal(formatRub("0"), "0 ₽");
  assert.equal(formatRub("9223372036854775807"), "92 233 720 368 547 758,07 ₽");
  for (const invalid of [22000000, "-1", "01", "1.5", "1e3", "9223372036854775808"]) {
    assert.throws(() => formatRub(invalid));
  }
});

test("read-model is deeply immutable and separate from legacy gateway", () => {
  assert.equal(Object.isFrozen(model), true);
  assert.equal(Object.isFrozen(model.documents[0].revealed), true);
  assert.throws(() => { model.documents[0].amountKopecks = "0"; }, TypeError);
  assert.throws(() => { model.documents.push({}); }, TypeError);
  assert.equal(model.documents.length, 3);
  const fs = require("node:fs");
  const path = require("node:path");
  const legacySource = fs.readFileSync(path.join(__dirname, "fixtures.js"), "utf8");
  assert.match(legacySource, /TAU_UI_MOCK/);
  assert.doesNotMatch(legacySource, /day01-read-model/); // not wired into legacy gateway
});
