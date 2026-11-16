"use strict";
const assert = require("node:assert/strict");
const fs = require("node:fs");
const path = require("node:path");
const fixture = JSON.parse(fs.readFileSync(path.join(__dirname, "DAY01_UI_FIXTURE.json"), "utf8"));
assert.equal(fixture.dayIndex, 0);
assert.equal(fixture.documents.length, 3);
assert.equal(fixture.visitors.length, 3);
assert.equal(new Set(fixture.documents.map(d => d.id)).size, 3);
assert.equal(new Set(fixture.visitors.map(v => v.visitId)).size, 3);
assert.equal(new Set(fixture.visitors.map(v => v.visitorId)).size, 3);
assert.equal(fixture.roomConflict.roomId, "L-204");
assert.equal(fixture.roomConflict.noAutomaticReservation, true);
assert.equal(fixture.endDayReport.partnerEarnedIncomeKopecks, "0");
const money = s => { assert.match(s, /^(0|-?[1-9][0-9]*)$/); const n = BigInt(s); assert.ok(n >= -(1n << 63n) && n <= (1n << 63n)-1n); return n; };
const documentIds = new Set(fixture.documents.map(d => d.id));
for (const visitor of fixture.visitors) {
  assert.ok(documentIds.has(visitor.documentId));
  assert.equal(visitor.choices.length, 3);
  assert.equal(new Set(visitor.choices.map(c => c.choiceId)).size, 3);
}
assert.equal(fixture.acceptanceExamples.length, 2, "review acceptance oracle count");
for (const example of fixture.acceptanceExamples) {
  assert.equal(example.dayIndex, 0);
  assert.equal(example.selectedChoiceIds.length, 3);
  assert.equal(new Set(example.selectedChoiceIds).size, 3);
  assert.ok(example.inspectionDocumentIds.length <= fixture.opening.inspectionTokens);
  const inspected = new Set(example.inspectionDocumentIds);
  assert.equal(inspected.size, example.inspectionDocumentIds.length);
  for (const id of inspected) assert.ok(documentIds.has(id));
  let budget = money(fixture.opening.budgetKopecks);
  const scores = {...fixture.opening.scores};
  const postings = [];
  const claims = [];
  for (const visitor of fixture.visitors) {
    const selected = visitor.choices.filter(c => example.selectedChoiceIds.includes(c.choiceId));
    assert.equal(selected.length, 1, "exactly one choice per visitor");
    const choice = selected[0];
    if (choice.requiresInspection) assert.ok(inspected.has(choice.requiresInspection));
    for (const effect of choice.effects) {
      if (effect.kind === "cashDelta") {
        const delta = money(effect.deltaKopecks);
        budget += delta;
        postings.push({choiceId: choice.choiceId, deltaKopecks: effect.deltaKopecks});
      } else if (effect.kind === "scoreDelta") {
        assert.ok(Object.hasOwn(scores, effect.score));
        scores[effect.score] += effect.delta;
      } else assert.fail("unexpected immediate effect kind");
    }
    if (choice.deferredPartnerRevenue) {
      money(choice.deferredPartnerRevenue.amountKopecks);
      assert.equal(choice.deferredPartnerRevenue.signingCashDeltaKopecks, "0");
      assert.equal(choice.deferredPartnerRevenue.earnedOnlyAfterDelivery, true);
    }
    const flag = Object.entries(choice.flags)[0];
    if ((flag[0] === "LAB" && ["full","phased"].includes(flag[1])) ||
        (flag[0] === "STUDENT" && flag[1] === "full") ||
        (flag[0] === "PARTNER" && flag[1] === "onsite")) claims.push(flag[0]);
  }
  const expected = example.expected;
  assert.equal(budget.toString(), expected.closingBudgetKopecks, example.id);
  assert.deepEqual(scores, expected.closingScores, example.id);
  assert.deepEqual(postings, expected.immediateLedgerPostings, example.id);
  assert.equal(expected.inspectionTokensUsed, inspected.size);
  assert.equal(expected.openingBudgetKopecks, fixture.opening.budgetKopecks);
  assert.equal(expected.partnerEarnedIncomeKopecks, "0");
  assert.equal(expected.weeklySettlementApplied, false);
  assert.equal(expected.nextDefinitionId, fixture.endDayReport.nextDefinitionId);
  if (claims.length > 1) {
    assert.ok(fixture.roomResolutionChoices.ifMultiple.some(c => c.id === example.roomResolutionChoiceId));
    if (example.roomStatus === "unresolved-no-booking") {
      assert.equal(example.roomResolutionChoiceId, "ROOM-4", "unresolved room must use explicit ROOM-4");
    } else {
      assert.notEqual(example.roomResolutionChoiceId, "ROOM-4", "ROOM-4 cannot represent a resolved priority");
    }
    assert.equal(example.roomStatus, "unresolved-no-booking");
  } else {
    assert.equal(example.roomResolutionChoiceId, "ROOM-AUTO");
    assert.equal(example.roomStatus, "requests-only-no-booking");
  }
}
console.log("PASS: 2 Day01 acceptance oracles, 3 visits/docs, budget, scores, postings, inspections, room, no signing income");

// Cross-platform Day01 art-link contract: planned references, not proof of PNG delivery.
const art = JSON.parse(fs.readFileSync(path.join(__dirname, "DAY01_ART_LINKS.json"), "utf8"));
assert.equal(art.dayIndex, fixture.dayIndex);
assert.deepEqual([...art.platforms].sort(), ["Android", "Windows"]);
assert.equal(art.worldLogicalResolution, "320x180");
assert.equal(art.artDirection, "A");
assert.equal(art.visits.length, fixture.visitors.length);
const artByVisit = new Map(art.visits.map(v => [v.visitId, v]));
assert.equal(artByVisit.size, art.visits.length);
const assetIds = new Set();
for (const visit of fixture.visitors) {
  const link = artByVisit.get(visit.visitId);
  assert.ok(link, "missing art mapping: " + visit.visitId);
  assert.equal(link.visitorId, visit.visitorId);
  assert.equal(link.documentId, visit.documentId);
  assert.ok(typeof link.visualBrief === "string" && link.visualBrief.trim().length >= 40);
  for (const field of ["portraitAssetId", "standingAssetId", "documentAssetId"]) {
    const id = link[field];
    assert.match(id, /^[a-z][a-z0-9_]*$/);
    assert.ok(!assetIds.has(id), "duplicate asset ID: " + id);
    assetIds.add(id);
  }
}
assert.equal(assetIds.size, 9);
console.log("PASS: Day01 art links 3/3 visits, 9 unique assets, cross-platform IDs");
