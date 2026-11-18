"use strict";
const assert = require("node:assert/strict");
const fs = require("node:fs");
const path = require("node:path");
// Draft event manifest consistency (content only; no EventDirector/runtime behavior).
const contentSpec = JSON.parse(fs.readFileSync(path.join(__dirname, "CONTENT_SPEC_DRAFT.json"), "utf8"));
const eventIds = contentSpec.eventManifest.map(event => event.id);
const projectIds = contentSpec.projects.map(project => project.id);
assert.equal(new Set([...eventIds, ...projectIds]).size, eventIds.length + projectIds.length,
  "duplicate event/project Definition ID");
const validTargets = new Set([...eventIds, ...projectIds, "CHAPTER2_HOOK"]);
for (const event of contentSpec.eventManifest) {
  assert.ok(Number.isSafeInteger(event.dayIndex) && event.dayIndex >= 0,
    "invalid event DayIndex: " + event.id);
  assert.equal(event.dayIndex, event.day - 1, "event day label mismatch: " + event.id);
  assert.ok(Array.isArray(event.feeds), "event feeds missing: " + event.id);
  assert.equal(new Set(event.feeds).size, event.feeds.length, "duplicate event feed: " + event.id);
  for (const target of event.feeds) {
    assert.ok(validTargets.has(target), "unknown event/project feed: " + event.id + " -> " + target);
  }
}
console.log("PASS: draft event manifest Definition IDs, 0-based DayIndex and feed targets");

const fixture = JSON.parse(fs.readFileSync(path.join(__dirname, "DAY01_UI_FIXTURE.json"), "utf8"));
assert.equal(fixture.dayIndex, 0);
// Day labels are 1-based presentation only; canonical state and reports use 0-based DayIndex.
assert.equal(fixture.displayDay, fixture.dayIndex + 1, "Day01 display day mismatch");
assert.equal(fixture.roomConflict.displayDay, fixture.roomConflict.dayIndex + 1,
  "room conflict display day mismatch");
assert.equal(fixture.endDayReport.dayIndex, fixture.dayIndex,
  "end-day report must refer to the current DayIndex");
assert.equal(fixture.documents.length, 3);
assert.equal(fixture.visitors.length, 3);
// Cross-file canonical identity guard: no drift between draft content and UI fixture.
const canonicalVisits = contentSpec.day1.visits.map(v => [v.id, v.npcId, v.documentId]);
const fixtureVisits = fixture.visitors.map(v => [v.visitId, v.visitorId, v.documentId]);
assert.deepEqual(fixtureVisits, canonicalVisits, "Day01 visit/NPC/document identity drift");
const canonicalNpcVisits = contentSpec.fictionalNpcs.map(npc => [npc.visitId, npc.id]).sort((a,b) => a[0].localeCompare(b[0]));
assert.deepEqual(fixture.visitors.map(v => [v.visitId, v.visitorId]).sort((a,b) => a[0].localeCompare(b[0])),
  canonicalNpcVisits, "Day01 NPC roster drift");
assert.equal(new Set(fixture.documents.map(d => d.id)).size, 3);
assert.equal(new Set(fixture.visitors.map(v => v.visitId)).size, 3);
assert.equal(new Set(fixture.visitors.map(v => v.visitorId)).size, 3);
// Cross-document IDs must be globally unique; inspections may only reference their own document.
const allChoiceIds = fixture.visitors.flatMap(visitor => visitor.choices.map(choice => choice.choiceId));
assert.equal(new Set(allChoiceIds).size, allChoiceIds.length, "duplicate Day01 choiceId across visitors");
const inspectionIds = fixture.documents.map(document => document.inspection.id);
assert.equal(new Set(inspectionIds).size, inspectionIds.length, "duplicate inspection definition ID");
for (const visitor of fixture.visitors) {
  const document = fixture.documents.find(item => item.id === visitor.documentId);
  assert.ok(document, "visitor document must exist");
  for (const choice of visitor.choices) {
    if (choice.requiresInspection !== null) {
      assert.equal(choice.requiresInspection, document.id, "choice must inspect its own document");
    }
  }
}
assert.equal(fixture.roomConflict.roomId, "L-204");
assert.equal(fixture.roomConflict.noAutomaticReservation, true);
assert.equal(fixture.endDayReport.partnerEarnedIncomeKopecks, "0");
const money = s => { assert.match(s, /^(0|-?[1-9][0-9]*)$/); const n = BigInt(s); assert.ok(n >= -(1n << 63n) && n <= (1n << 63n)-1n); return n; };
// Every authored document monetary field and the opening balance use canonical Int64 kopecks.
money(fixture.opening.budgetKopecks);
for (const document of fixture.documents) {
  assert.equal(document.visible.dayIndex, document.visible.day - 1, "document day label must be 1-based");
  for (const section of [document.visible, document.inspection.reveals]) {
    for (const [key, value] of Object.entries(section)) {
      if (key.endsWith("Kopecks")) money(value);
    }
  }
}
// Score invariants: the authored opening and every golden Day01 outcome stay within Core Q/S/M/E/R bounds.
assert.deepEqual(Object.keys(fixture.opening.scores).sort(), ["E", "M", "Q", "R", "S"]);
for (const [score, value] of Object.entries(fixture.opening.scores)) {
  assert.ok(Number.isSafeInteger(value) && value >= 0 && value <= 100, "invalid opening score: " + score);
}
const documentIds = new Set(fixture.documents.map(d => d.id));
// One-to-one visit/document mapping: no shared or orphaned Day01 documents.
const assignedDocumentIds = fixture.visitors.map(visitor => visitor.documentId);
assert.equal(new Set(assignedDocumentIds).size, assignedDocumentIds.length,
  "multiple Day01 visitors share a document");
assert.deepEqual([...assignedDocumentIds].sort(), [...documentIds].sort(),
  "Day01 visits must cover every document exactly once");
for (const visitor of fixture.visitors) {
  assert.ok(documentIds.has(visitor.documentId));
  assert.equal(visitor.choices.length, 3);
  assert.equal(new Set(visitor.choices.map(c => c.choiceId)).size, 3);
}
// Validate every authored choice, not only choices exercised by the two golden examples.
for (const visitor of fixture.visitors) {
  for (const choice of visitor.choices) {
    assert.ok(Array.isArray(choice.effects), "choice effects must be an array: " + choice.choiceId);
    for (const effect of choice.effects) {
      if (effect.kind === "cashDelta") {
        money(effect.deltaKopecks); // signed Int64 decimal-string kopecks
      } else if (effect.kind === "scoreDelta") {
        assert.ok(Object.hasOwn(fixture.opening.scores, effect.score), "unknown score: " + effect.score);
        assert.ok(Number.isSafeInteger(effect.delta), "non-integer score delta: " + choice.choiceId);
      } else {
        assert.fail("unsupported immediate effect: " + choice.choiceId);
      }
    }
    if (choice.deferredPartnerRevenue) {
      assert.ok(!choice.effects.some(effect => effect.kind === "cashDelta"),
        "partner signing cannot post immediate cash: " + choice.choiceId);
      money(choice.deferredPartnerRevenue.amountKopecks);
      assert.equal(choice.deferredPartnerRevenue.signingCashDeltaKopecks, "0");
      assert.equal(choice.deferredPartnerRevenue.earnedOnlyAfterDelivery, true);
      assert.ok(Number.isSafeInteger(choice.deferredPartnerRevenue.earliestDayIndex) &&
        choice.deferredPartnerRevenue.earliestDayIndex >= 4,
        "partner income cannot be earned before DayIndex 4: " + choice.choiceId);
      assert.deepEqual([...choice.deferredPartnerRevenue.requiredFacts].sort(),
        ["E03_mentor_paid", "E07_delivered"].sort(),
        "partner income requires both Core-confirmed facts: " + choice.choiceId);
    }
  }
}
assert.equal(fixture.deferredFactProvenance.earnedIdTemplate,
  "partner/{sourceInstanceId}/delivery",
  "partner earned receipt must use original Core agreement instance");
console.log("PASS: all Day01 choice effects have valid types, Int64 kopecks and deferred signing rules");
assert.equal(fixture.acceptanceExamples.length, 2, "review acceptance oracle count");
for (const example of fixture.acceptanceExamples) {
  assert.equal(example.dayIndex, 0);
  assert.equal(example.selectedChoiceIds.length, 3);
  assert.equal(new Set(example.selectedChoiceIds).size, 3);
  assert.ok(Number.isSafeInteger(fixture.opening.inspectionTokens) && fixture.opening.inspectionTokens >= 0, "invalid opening inspection tokens");
  const inspected = new Set(example.inspectionDocumentIds);
  assert.equal(inspected.size, example.inspectionDocumentIds.length);
  const inspectionTokensUsed = [...inspected].reduce((sum, id) => {
    const document = fixture.documents.find(d => d.id === id);
    assert.ok(document, "inspection references unknown document: " + id);
    assert.ok(Number.isSafeInteger(document.inspection.costTokens) && document.inspection.costTokens >= 0, "invalid inspection cost: " + id);
    assert.equal(document.inspection.repeatCostTokens, 0, "repeated inspection must be free: " + id);
    return sum + document.inspection.costTokens;
  }, 0);
  assert.ok(inspectionTokensUsed <= fixture.opening.inspectionTokens, "inspection exceeds token budget: " + example.id);
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
  for (const [score, value] of Object.entries(scores)) {
    assert.ok(Number.isSafeInteger(value) && value >= 0 && value <= 100,
      "out-of-range closing score " + score + ": " + example.id);
  }
  assert.deepEqual(scores, expected.closingScores, example.id);
  assert.deepEqual(postings, expected.immediateLedgerPostings, example.id);
  assert.equal(expected.inspectionTokensUsed, inspectionTokensUsed);
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
// Draft content and planned art links must agree; this does not verify PNG delivery.
const draftNpcsByVisit = new Map(contentSpec.fictionalNpcs.map(npc => [npc.visitId, npc]));
const draftDocumentsById = new Map(contentSpec.documentTypes.map(doc => [doc.id, doc]));
assert.equal(draftNpcsByVisit.size, art.visits.length, "draft NPC visit count mismatch");
assert.equal(draftDocumentsById.size, art.visits.length, "draft document count mismatch");
for (const link of art.visits) {
  const npc = draftNpcsByVisit.get(link.visitId);
  const doc = draftDocumentsById.get(link.documentId);
  assert.ok(npc && doc, "art link missing draft NPC/document: " + link.visitId);
  assert.equal(npc.id, link.visitorId, "draft NPC identity mismatch: " + link.visitId);
  assert.equal(npc.portraitAssetId, link.portraitAssetId, "draft portrait ID mismatch: " + link.visitId);
  assert.equal(npc.visualDescription, link.visualBrief, "draft visual brief mismatch: " + link.visitId);
  assert.equal(doc.documentAssetId, link.documentAssetId, "draft document asset ID mismatch: " + link.visitId);
}
console.log("PASS: content draft and art links 3 NPC portraits/briefs and 3 document asset IDs");
console.log("PASS: Day01 art links 3/3 visits, 9 unique assets, cross-platform IDs");

// Issue #21: every planned Day01 art ID must have an explicit production brief.
// This checks source text only; it does not establish that PNGs exist.
const artPrompts = fs.readFileSync(path.join(__dirname, "DAY01_ART_PROMPTS.md"), "utf8");
const briefLines = artPrompts.split(/\r?\n/).filter(line => /^(?:## Candidate \d+ — |### |- )/.test(line));
for (const id of assetIds) {
  const matches = briefLines.filter(line => line.includes("`" + id + "`"));
  assert.equal(matches.length, 1, "expected exactly one production brief heading/bullet for " + id);
}
console.log("PASS: 9/9 Day01 art IDs have production briefs (PNG delivery not checked)");
