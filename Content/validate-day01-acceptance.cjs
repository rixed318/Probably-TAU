"use strict";
const assert = require("node:assert/strict");
const fs = require("node:fs");
const path = require("node:path");
// Draft event manifest consistency (content only; no EventDirector/runtime behavior).
const contentSpec = JSON.parse(fs.readFileSync(path.join(__dirname, "CONTENT_SPEC_DRAFT.json"), "utf8"));
// Validate all nested draft monetary fields; Core owns actual ledger postings.
function validateDraftMoney(node, location = "content") {
  if (Array.isArray(node)) {
    node.forEach((item, i) => validateDraftMoney(item, location + "[" + i + "]"));
    return;
  }
  if (node === null || typeof node !== "object") return;
  for (const [key, value] of Object.entries(node)) {
    const field = location + "." + key;
    if (key.endsWith("Kopecks")) {
      assert.equal(typeof value, "string", "money must be a decimal string: " + field);
      assert.match(value, /^(0|-?[1-9][0-9]*)$/, "invalid kopecks: " + field);
      const amount = BigInt(value);
      assert.ok(amount >= -(1n << 63n) && amount <= (1n << 63n) - 1n,
        "kopecks exceed signed Int64: " + field);
    } else {
      validateDraftMoney(value, field);
    }
  }
}
validateDraftMoney(contentSpec);
// Deferred partner revenue is never cash at signing; Core owns earned ledger postings.
const partnerVisit = contentSpec.day1.visits.find(visit => visit.id === "D01_PARTNER");
assert.ok(partnerVisit, "missing Day01 partner visit");
for (const choice of partnerVisit.choices) {
  if (choice.id === "PARTNER-3") {
    assert.equal(choice.conditionalRevenueKopecks, undefined,
      "declined partner offer cannot promise revenue");
    continue;
  }
  assert.ok(["PARTNER-1", "PARTNER-2"].includes(choice.id),
    "unexpected Day01 partner choice: " + choice.id);
  assert.equal(choice.revenueEarliestDayIndex, 4,
    "partner revenue cannot be recognized before DayIndex 4: " + choice.id);
  assert.equal(choice.revenueEarliestDay, choice.revenueEarliestDayIndex + 1,
    "partner revenue display day/DayIndex mismatch: " + choice.id);
  assert.deepEqual([...choice.requirements].sort(), ["E03_mentor_paid", "E07_delivered"].sort(),
    "partner revenue requires both Core-confirmed facts: " + choice.id);
  assert.ok(BigInt(choice.conditionalRevenueKopecks) > 0n,
    "partner conditional revenue must be positive: " + choice.id);
  assert.ok(!choice.effects.some(effect => effect.metric === "budget" ||
    Object.hasOwn(effect, "deltaKopecks")),
    "partner signing cannot post cash: " + choice.id);
}
// Draft weekly closing days are zero-based: displayed Days 05 and 10 -> DayIndex 4 and 9.
// Guard content labels only; Core owns EndDay settlement and ledger idempotency.
assert.deepEqual(contentSpec.meta.weeklySettlementClosingDayIndexes, [4, 9],
  "weekly closing DayIndexes must be 4 and 9 (display Days 05/10)");
assert.equal(contentSpec.opening.weeklySettlementFirstClosingDayIndex, 4,
  "opening first weekly closing DayIndex must be 4");
// Day01 uses zero-based canonical DayIndex; document day labels are display-only.
// Keep the draft consistent before UI/Core adapters consume these values.
assert.equal(contentSpec.opening.dayIndex, 0, "opening Day01 must have DayIndex 0");
assert.equal(contentSpec.day1.dayIndex, contentSpec.opening.dayIndex,
  "Day01 scenario DayIndex must match opening");
for (const document of contentSpec.documentTypes) {
  assert.ok(Number.isSafeInteger(document.visible.dayIndex) &&
    document.visible.dayIndex === document.visible.day - 1,
    "draft document display day/DayIndex mismatch: " + document.id);
}
const eventIds = contentSpec.eventManifest.map(event => event.id);
const projectIds = contentSpec.projects.map(project => project.id);
// Guard complete authored rosters, not only valid ID syntax: a missing event/project
// must not silently pass content validation or remove a planned Day01 follow-up.
const expectedEventIds = Array.from({length: 13}, (_, index) => "E" + String(index + 2).padStart(2, "0"));
assert.deepEqual([...eventIds].sort(), expectedEventIds,
  "event manifest must contain exactly E02..E14 once each");
assert.deepEqual([...projectIds].sort(), ["P01", "P02"],
  "project manifest must contain exactly P01 and P02 once each");
// Stable event/project Definition IDs are exact authored tokens, never paths or display labels.
// These draft constraints do not define Core runtime types.
for (const id of eventIds) {
  assert.match(id, /^E(?:0[2-9]|1[0-4])$/, "invalid event Definition ID: " + id);
}
for (const id of projectIds) {
  assert.match(id, /^P0[12]$/, "invalid project Definition ID: " + id);
}
// Draft choice Definition IDs must not collide across different events.
const eventChoiceIds = contentSpec.eventManifest.flatMap(event => event.choices);
assert.equal(new Set(eventChoiceIds).size, eventChoiceIds.length,
  "duplicate event choice Definition ID across event manifest");
// Day01 and follow-up event choices share one authored Definition ID namespace.
// Core CommandId and runtime instanceId remain separately owned.
const day01ChoiceIds = contentSpec.day1.visits.flatMap(visit => visit.choices.map(choice => choice.id));
assert.equal(new Set([...day01ChoiceIds, ...eventChoiceIds]).size,
  day01ChoiceIds.length + eventChoiceIds.length,
  "duplicate choice Definition ID between Day01 visits and follow-up events");
for (const event of contentSpec.eventManifest) {
  assert.equal(event.required, true,
    "every E02-E14 event must remain mandatory in the authored manifest: " + event.id);
  assert.ok(Array.isArray(event.choices) && event.choices.length >= 2,
    "every E02-E14 event must offer at least two authored outcomes: " + event.id);
  for (const choiceId of event.choices) {
    assert.match(choiceId, new RegExp("^" + event.id + "-[A-Z]+$"),
      "event choice Definition ID must belong to its event: " + choiceId);
  }
}
assert.equal(new Set([...eventIds, ...projectIds]).size, eventIds.length + projectIds.length,
  "duplicate event/project Definition ID");
// Project launch choice references must resolve to authored Day01/event Definition IDs.
const authoredChoiceIds = new Set([
  ...contentSpec.day1.visits.flatMap(visit => visit.choices.map(choice => choice.id)),
  ...eventChoiceIds
]);
for (const project of contentSpec.projects) {
  if (project.launchChoices !== undefined) {
    assert.ok(Array.isArray(project.launchChoices) && project.launchChoices.length > 0,
      "project launchChoices must be a nonempty array: " + project.id);
    assert.equal(new Set(project.launchChoices).size, project.launchChoices.length,
      "duplicate project launch choice: " + project.id);
    for (const choiceId of project.launchChoices) {
      assert.ok(authoredChoiceIds.has(choiceId),
        "unknown project launch choice Definition ID: " + project.id + " -> " + choiceId);
    }
  }
}
// Project monetary proposals must use signed Int64 decimal-string kopecks, not JSON numbers.
// This is content validation only; Core owns actual project payments and ledger postings.
for (const project of contentSpec.projects) {
  for (const [key, value] of Object.entries(project)) {
    if (!key.endsWith("Kopecks")) continue;
    assert.equal(typeof value, "string", "project money must be a decimal string: " + project.id + "/" + key);
    assert.match(value, /^(0|-?[1-9][0-9]*)$/, "invalid project kopecks: " + project.id + "/" + key);
    const amount = BigInt(value);
    assert.ok(amount >= -(1n << 63n) && amount <= (1n << 63n) - 1n,
      "project kopecks exceed Int64: " + project.id + "/" + key);
  }
}
// Draft project progress must count positive full EndDay transitions after launch.
// Core remains responsible for runtime progress and scheduling.
for (const project of contentSpec.projects) {
  if (project.requiredFullEndDaysAfterLaunch !== undefined) {
    assert.ok(Number.isSafeInteger(project.requiredFullEndDaysAfterLaunch) &&
      project.requiredFullEndDaysAfterLaunch >= 1,
      "project requires a positive integer count of full EndDays: " + project.id);
  }
}
// P02's three *full* EndDay transitions are a fixed authored balance rule.
// The launch-day close is excluded; this does not advance a runtime clock.
const workshop = contentSpec.projects.find(project => project.id === "P02");
assert.ok(workshop, "missing P02 workshop");
assert.equal(workshop.requiredFullEndDaysAfterLaunch, 3,
  "P02 must require exactly three full EndDays after launch");
// Draft project unlocks must resolve to an authored event that actually feeds the project.
// This guards narrative references only; NPC/EventDirector owns runtime completion facts.
for (const project of contentSpec.projects) {
  if (project.unlockBy === undefined) continue;
  const match = /^(E(?:0[2-9]|1[0-4])) completed$/.exec(project.unlockBy);
  assert.ok(match, "invalid draft project unlockBy: " + project.id);
  const sourceEvent = contentSpec.eventManifest.find(event => event.id === match[1]);
  assert.ok(sourceEvent && sourceEvent.feeds.includes(project.id),
    "project unlockBy must match an authored event feed: " + project.id);
}
const validTargets = new Set([...eventIds, ...projectIds, "CHAPTER2_HOOK"]);
for (const event of contentSpec.eventManifest) {
  assert.ok(Number.isSafeInteger(event.dayIndex) && event.dayIndex >= 0,
    "invalid event DayIndex: " + event.id);
  assert.equal(event.dayIndex, event.day - 1, "event day label mismatch: " + event.id);
  // E09 is a report-only settlement view, not a second cash posting stage.
  // Keep all other authored events in scheduled-content; Core owns the ledger.
  const expectedPhase = event.id === "E09" ? "EndDay-settlement" : "scheduled-content";
  assert.equal(event.triggerPhase, expectedPhase,
    "event triggerPhase drift (E09 report-only): " + event.id);
  // Every authored event needs an explicit nonempty fallback for rejected/unavailable branches.
  // This is a narrative-data invariant, not an EventDirector implementation.
  assert.ok(typeof event.fallback === "string" && event.fallback.trim().length > 0,
    "missing event fallback: " + event.id);
  assert.ok(Array.isArray(event.feeds), "event feeds missing: " + event.id);
  assert.equal(new Set(event.feeds).size, event.feeds.length, "duplicate event feed: " + event.id);
  for (const target of event.feeds) {
    assert.ok(validTargets.has(target), "unknown event/project feed: " + event.id + " -> " + target);
    assert.notEqual(target, event.id, "event cannot feed itself: " + event.id);
    const nextEvent = contentSpec.eventManifest.find(candidate => candidate.id === target);
    if (nextEvent) {
      assert.ok(nextEvent.dayIndex > event.dayIndex,
        "event feed must point to a later DayIndex: " + event.id + " -> " + target);
    }
  }
}
console.log("PASS: draft event manifest Definition IDs, 0-based DayIndex and feed targets");

const fixture = JSON.parse(fs.readFileSync(path.join(__dirname, "DAY01_UI_FIXTURE.json"), "utf8"));
// Static partner deferred-revenue parity; Core alone recognizes earned ledger income.
const partnerFixtureVisit = fixture.visitors.find(visit => visit.visitId === "D01_PARTNER");
assert.ok(partnerFixtureVisit, "missing partner fixture visit");
for (const choice of partnerVisit.choices) {
  const fixtureChoice = partnerFixtureVisit.choices.find(item => item.choiceId === choice.id);
  assert.ok(fixtureChoice, "missing partner fixture choice: " + choice.id);
  if (choice.id === "PARTNER-3") {
    assert.equal(fixtureChoice.deferredPartnerRevenue, undefined,
      "declined partner fixture must not promise revenue");
    continue;
  }
  const deferred = fixtureChoice.deferredPartnerRevenue;
  assert.ok(deferred, "missing fixture deferred partner revenue: " + choice.id);
  assert.equal(deferred.amountKopecks, choice.conditionalRevenueKopecks,
    "partner deferred amount drift: " + choice.id);
  assert.equal(deferred.earliestDayIndex, choice.revenueEarliestDayIndex,
    "partner deferred DayIndex drift: " + choice.id);
  assert.deepEqual([...deferred.requiredFacts].sort(), [...choice.requirements].sort(),
    "partner deferred receipt prerequisites drift: " + choice.id);
  assert.equal(deferred.earnedOnlyAfterDelivery, true,
    "partner fixture must require delivered service: " + choice.id);
  assert.equal(deferred.signingCashDeltaKopecks, "0",
    "partner fixture must not credit signing cash: " + choice.id);
}
console.log("PASS: Day01 partner deferred revenue draft/fixture parity; no signing cash");
assert.equal(fixture.sourceSpecVersion, contentSpec.meta.specVersion,
  "Day01 UI fixture sourceSpecVersion drift from canonical content draft");
assert.equal(fixture.dayIndex, 0);
// Day labels are 1-based presentation only; canonical state and reports use 0-based DayIndex.
assert.equal(fixture.displayDay, fixture.dayIndex + 1, "Day01 display day mismatch");
assert.equal(fixture.roomConflict.displayDay, fixture.roomConflict.dayIndex + 1,
  "room conflict display day mismatch");
assert.equal(fixture.endDayReport.dayIndex, fixture.dayIndex,
  "end-day report must refer to the current DayIndex");
// Day01 room collision is a shared fictional request slot, never an automatic booking.
// Keep the canonical draft and platform-independent UI fixture aligned.
function validateRoomSlot(draft, ui) {
  const room = ui.roomConflict;
  assert.equal(room.displayRoom, draft.opening.fictionalRoom, "room conflict display room drift");
  assert.equal(draft.opening.collisionSlot,
    "day" + String(room.displayDay).padStart(2, "0") + "/" + room.slot,
    "opening collision slot drift");
  assert.equal(room.dayIndex, room.displayDay - 1, "room collision DayIndex drift");
  assert.equal(room.status, "requests-only", "room conflict must remain requests-only");
  assert.equal(room.noAutomaticReservation, true, "room requests cannot book automatically");
  for (const doc of draft.documentTypes) {
    assert.equal(doc.visible.room, room.displayRoom, "draft document room drift: " + doc.id);
    assert.equal(doc.visible.dayIndex, room.dayIndex, "draft document collision DayIndex drift: " + doc.id);
    assert.equal(doc.visible.slot, room.slot, "draft document collision slot drift: " + doc.id);
    const uiDoc = ui.documents.find(item => item.id === doc.id);
    assert.ok(uiDoc, "missing room conflict document: " + doc.id);
    assert.equal(uiDoc.visible.room, doc.visible.room, "UI document room drift: " + doc.id);
    assert.equal(uiDoc.visible.dayIndex, doc.visible.dayIndex, "UI document collision DayIndex drift: " + doc.id);
    assert.equal(uiDoc.visible.slot, doc.visible.slot, "UI document collision slot drift: " + doc.id);
  }
}
validateRoomSlot(contentSpec, fixture);
console.log("PASS: Day01 three-document shared room/slot/DayIndex draft-fixture parity; requests are not bookings");
assert.equal(fixture.documents.length, 3);
assert.equal(fixture.visitors.length, 3);
// Cross-file canonical identity guard: no drift between draft content and UI fixture.
const canonicalVisits = contentSpec.day1.visits.map(v => [v.id, v.npcId, v.documentId]);
const fixtureVisits = fixture.visitors.map(v => [v.visitId, v.visitorId, v.documentId]);
assert.deepEqual(fixtureVisits, canonicalVisits, "Day01 visit/NPC/document identity drift");
// Reject duplicate canonical NPC Definition IDs before roster comparison; one fictional NPC per Day01 visit.
assert.equal(new Set(contentSpec.fictionalNpcs.map(npc => npc.id)).size, contentSpec.fictionalNpcs.length,
  "duplicate canonical Day01 NPC Definition ID");
const canonicalNpcVisits = contentSpec.fictionalNpcs.map(npc => [npc.visitId, npc.id]).sort((a,b) => a[0].localeCompare(b[0]));
assert.deepEqual(fixture.visitors.map(v => [v.visitId, v.visitorId]).sort((a,b) => a[0].localeCompare(b[0])),
  canonicalNpcVisits, "Day01 NPC roster drift");
assert.equal(new Set(fixture.documents.map(d => d.id)).size, 3);
assert.equal(new Set(fixture.visitors.map(v => v.visitId)).size, 3);
assert.equal(new Set(fixture.visitors.map(v => v.visitorId)).size, 3);
// Stable fictional asset identities are authored in Content, never inferred from display names.
// UI/Android/Windows can resolve these IDs to original art without changing the Core schema.
const portraitIds = contentSpec.fictionalNpcs.map(npc => npc.portraitAssetId);
const standingIds = contentSpec.fictionalNpcs.map(npc => npc.standingAssetId);
assert.equal(new Set(standingIds).size, standingIds.length,
  "duplicate canonical Day01 standing sprite asset ID");
for (const assetId of standingIds) {
  assert.match(assetId, /^[a-z0-9]+(?:_[a-z0-9]+)*$/,
    "invalid platform-independent standing sprite asset ID");
}
assert.equal(new Set([...standingIds, ...contentSpec.fictionalNpcs.map(npc => npc.portraitAssetId)]).size,
  standingIds.length + contentSpec.fictionalNpcs.length,
  "standing sprite and portrait asset IDs must not collide");
const documentAssetIds = contentSpec.documentTypes.map(doc => doc.documentAssetId);
// All nine planned Day01 asset identities share one cross-platform namespace.
// A standing sprite must not reuse a document or portrait ID.
const assetIds = [...portraitIds, ...standingIds, ...documentAssetIds];
assert.equal(portraitIds.length, fixture.visitors.length, "Day01 portrait roster mismatch");
assert.equal(documentAssetIds.length, fixture.documents.length, "Day01 document art roster mismatch");
assert.equal(new Set(assetIds).size, assetIds.length, "duplicate Day01 asset ID");
for (const assetId of assetIds) {
  assert.match(assetId, /^[a-z0-9]+(?:_[a-z0-9]+)*$/, "invalid platform-independent asset ID");
}
for (const npc of contentSpec.fictionalNpcs) {
  assert.ok(typeof npc.visualDescription === "string" && npc.visualDescription.trim().length > 0,
    "missing original character visual description: " + npc.id);
}
// Original art prompts must remain visually distinguishable across the three fictional visitors.
const visualBriefs = contentSpec.fictionalNpcs.map(npc => npc.visualDescription.trim().replace(/\s+/g, " ").toLocaleLowerCase("ru-RU"));
assert.equal(new Set(visualBriefs).size, visualBriefs.length,
  "duplicate fictional Day01 NPC visual descriptions");
// Planned art links must match canonical fictional NPC/document definitions exactly.
// These are lookup keys for UI on Windows/Android, not generated PNGs or Core instance IDs.
const artLinks = JSON.parse(fs.readFileSync(path.join(__dirname, "DAY01_ART_LINKS.json"), "utf8"));
assert.equal(artLinks.dayIndex, contentSpec.day1.dayIndex, "Day01 art links DayIndex drift");
const canonicalArtLinks = contentSpec.day1.visits.map(visit => {
  const npc = contentSpec.fictionalNpcs.find(item => item.id === visit.npcId);
  const doc = contentSpec.documentTypes.find(item => item.id === visit.documentId);
  assert.ok(npc && doc, "unresolved canonical Day01 art identity: " + visit.id);
  return [visit.id, npc.id, doc.id, npc.portraitAssetId, npc.standingAssetId,
    doc.documentAssetId, npc.visualDescription];
});
assert.deepEqual(artLinks.visits.map(visit => [visit.visitId, visit.visitorId,
  visit.documentId, visit.portraitAssetId, visit.standingAssetId,
  visit.documentAssetId, visit.visualBrief]), canonicalArtLinks,
  "Day01 art-link NPC/document/asset/visual-brief drift");
console.log("PASS: Day01 planned art links match canonical fictional content");
console.log("PASS: stable fictional Day01 portrait/document asset IDs and descriptions");

// Guard canonical Day01 money against drift between authored content and UI mock.
// This is static data comparison only: Core remains the sole cash/ledger writer.
assert.equal(fixture.opening.budgetKopecks, contentSpec.opening.budgetKopecks,
  "Day01 opening budget drift");
const draftDocs = new Map(contentSpec.documentTypes.map(d => [d.id, d]));
assert.equal(draftDocs.size, contentSpec.documentTypes.length, "duplicate canonical document Definition ID");
assert.deepEqual([...draftDocs.keys()].sort(), fixture.documents.map(d => d.id).sort(),
  "Day01 draft/fixture document Definition ID roster drift");
for (const document of fixture.documents) {
  const draft = draftDocs.get(document.id);
  assert.ok(draft, "missing canonical document: " + document.id);
  for (const section of ["visible", "revealed"]) {
    const fixtureSection = section === "visible" ? document.visible : document.inspection.reveals;
    for (const [key, value] of Object.entries(draft[section])) {
      if (key.endsWith("Kopecks")) {
        assert.equal(fixtureSection[key], value, "document money drift: " + document.id + "/" + key);
      }
    }
  }
}
for (const visit of contentSpec.day1.visits) {
  const mock = fixture.visitors.find(v => v.visitId === visit.id);
  assert.ok(mock, "missing Day01 visitor: " + visit.id);
  assert.deepEqual(mock.choices.map(c => c.choiceId).sort(), visit.choices.map(c => c.id).sort(),
    "Day01 canonical/UI choice Definition ID roster drift: " + visit.id);
  for (const choice of visit.choices) {
    const mockChoice = mock.choices.find(c => c.choiceId === choice.id);
    assert.ok(mockChoice, "missing Day01 choice: " + choice.id);
    const draftCash = choice.effects.filter(e => e.metric === "budget").map(e => e.deltaKopecks);
    const mockCash = mockChoice.effects.filter(e => e.kind === "cashDelta").map(e => e.deltaKopecks);
    assert.deepEqual(mockCash, draftCash, "choice cash drift: " + choice.id);
    if (Object.hasOwn(choice, "conditionalRevenueKopecks")) {
      assert.equal(mockChoice.deferredPartnerRevenue?.amountKopecks, choice.conditionalRevenueKopecks,
        "deferred partner revenue drift: " + choice.id);
    } else {
      assert.equal(mockChoice.deferredPartnerRevenue, undefined,
        "unexpected deferred partner revenue: " + choice.id);
    }
  }
}
console.log("PASS: Day01 draft/fixture opening, document, immediate and deferred kopeck amounts agree");
// Guard choice flags, score effects and inspection prerequisites across canonical draft/UI.
// These are declarative content comparisons; no Core state mutation occurs here.
for (const visit of contentSpec.day1.visits) {
  const mock = fixture.visitors.find(v => v.visitId === visit.id);
  for (const choice of visit.choices) {
    const mockChoice = mock.choices.find(c => c.choiceId === choice.id);
    assert.deepEqual(mockChoice.flags, choice.flag, "choice flag drift: " + choice.id);
    const expectedEffects = choice.effects.map(effect => effect.metric === "budget"
      ? {kind: "cashDelta", deltaKopecks: effect.deltaKopecks}
      : {kind: "scoreDelta", score: effect.metric, delta: effect.delta});
    assert.deepEqual(mockChoice.effects, expectedEffects, "choice effect drift: " + choice.id);
    const requiredInspection = choice.condition?.startsWith("inspected:") ? visit.documentId : null;
    assert.equal(mockChoice.requiresInspection, requiredInspection,
      "choice inspection prerequisite drift: " + choice.id);
  }
}
console.log("PASS: Day01 flags, full choice effects and inspection prerequisites agree");


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
const documentIdList = fixture.documents.map(d => d.id);
const documentIds = new Set(documentIdList);
assert.equal(documentIds.size, documentIdList.length,
  "duplicate Day01 document Definition ID");
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
assert.equal(fixture.acceptanceExamples.length, 3, "review acceptance oracle count");
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
    if (example.roomResolutionChoiceId === "ROOM-4") {
      assert.equal(example.roomStatus, "unresolved-no-booking");
    } else {
      const priority = fixture.roomResolutionChoices.ifMultiple.find(c => c.id === example.roomResolutionChoiceId).priority;
      assert.equal(example.roomStatus, "priority-" + priority + "-no-booking",
        "explicit room priority must not silently become a booking");
    }
  } else {
    assert.equal(example.roomResolutionChoiceId, "ROOM-AUTO");
    assert.equal(example.roomStatus, "requests-only-no-booking");
  }
}
console.log("PASS: 3 Day01 acceptance oracles, 3 visits/docs, budget, scores, postings, inspections, room, no signing income");

// Cross-platform Day01 art-link contract: planned references, not proof of PNG delivery.
const art = JSON.parse(fs.readFileSync(path.join(__dirname, "DAY01_ART_LINKS.json"), "utf8"));
assert.equal(art.dayIndex, fixture.dayIndex);
assert.equal(art.schemaVersion, "day01-art-links-1", "Day01 art link schema version drift");
assert.equal(art.status, "planned-art-references-not-PNGs-or-runtime",
  "Day01 art links must not claim runtime integration or delivered PNGs");

// Keep art provenance pointers stable; these are planned references, not verified PNGs.
assert.equal(art.sourceFixture, "Content/DAY01_UI_FIXTURE.json", "Day01 art links must cite canonical UI fixture");
assert.equal(art.sourceManifest, "feature/ui-5/docs/art/ASSET_MANIFEST_D01.csv", "Day01 art links must cite UI-owned asset manifest");
assert.ok(Array.isArray(art.platforms), "Day01 art platforms must be an array");
assert.deepEqual([...art.platforms].sort(), ["Android", "Windows"]);
// Art handoff must use the same zero-based Day01 index as the content and UI fixture.
// This is metadata parity only; no runtime clock or event scheduling is changed.
assert.equal(art.dayIndex, contentSpec.day1.dayIndex,
  "Day01 art links DayIndex must match canonical content DayIndex 0");
assert.equal(art.worldLogicalResolution, "320x180");
assert.equal(art.artDirection, "A");
// Reject ambiguous or duplicate policy metadata; this does not inspect generated art.
assert.ok(Array.isArray(art.rules), "Day01 art rules must be an array");
assert.ok(art.rules.every(rule => typeof rule === "string" && rule.trim().length > 0), "Day01 art rules must be nonempty strings");
assert.equal(new Set(art.rules.map(rule => rule.trim().toLowerCase())).size, art.rules.length, "Day01 art rules must be unique");
// Fictional original art only: metadata guard, not an IP clearance or PNG inspection.
assert.ok(Array.isArray(art.rules) && art.rules.includes("Original fictional characters only; no real academy branding"),
  "Day01 art contract must require original fictional characters and exclude real academy branding");
// Documents must remain blank templates: UI renders localized text and kopeck amounts.
// This is an authored metadata guard, not a check of actual PNG pixel contents.
assert.ok(Array.isArray(art.rules) && art.rules.includes("All document text and money are rendered dynamically by UI"),
  "Day01 art contract must keep document text and money out of baked-in assets");
assert.ok(art.rules.includes("No change to Core, Day01 choices, money, events or ledger"),
  "Day01 art contract must forbid gameplay, economy and event changes");
assert.ok(Array.isArray(art.visits), "Day01 art visits must be an array");
assert.ok(art.visits.every(visit => visit !== null && typeof visit === "object" && !Array.isArray(visit)),
  "Day01 art visits must contain non-null objects");
assert.equal(art.visits.length, fixture.visitors.length);
const artByVisit = new Map(art.visits.map(v => [v.visitId, v]));
assert.equal(artByVisit.size, art.visits.length);
const artAssetIds = new Set();
for (const visit of fixture.visitors) {
  const link = artByVisit.get(visit.visitId);
  assert.ok(link, "missing art mapping: " + visit.visitId);
  assert.equal(link.visitorId, visit.visitorId);
  assert.equal(link.documentId, visit.documentId);
  assert.ok(typeof link.visualBrief === "string" && link.visualBrief.trim().length >= 40);
  for (const field of ["portraitAssetId", "standingAssetId", "documentAssetId"]) {
    const id = link[field];
    assert.match(id, /^[a-z][a-z0-9_]*$/);
    assert.ok(!artAssetIds.has(id), "duplicate asset ID: " + id);
    artAssetIds.add(id);
  }
}
assert.equal(artAssetIds.size, 9);
// Original Day01 NPCs need distinct production briefs, not interchangeable portraits.
// Authored text only: UI/owner must still review actual generated PNGs.
const normalizedVisualBriefs = art.visits.map(visit => visit.visualBrief.trim().replace(/\s+/g, " ").toLocaleLowerCase("ru"));
assert.equal(new Set(normalizedVisualBriefs).size, art.visits.length,
  "Day01 fictional NPC visual briefs must be distinct");
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
  assert.equal(npc.standingAssetId, link.standingAssetId, "draft standing ID mismatch: " + link.visitId);
  assert.equal(npc.visualDescription, link.visualBrief, "draft visual brief mismatch: " + link.visitId);
  assert.equal(doc.documentAssetId, link.documentAssetId, "draft document asset ID mismatch: " + link.visitId);
}
console.log("PASS: content draft and art links 3 NPC portraits/briefs and 3 document asset IDs");
console.log("PASS: Day01 art links 3/3 visits, 9 unique assets, cross-platform IDs");

// Issue #21: every planned Day01 art ID must have an explicit production brief.
// This checks source text only; it does not establish that PNGs exist.
const artPrompts = fs.readFileSync(path.join(__dirname, "DAY01_ART_PROMPTS.md"), "utf8");
const briefLines = artPrompts.split(/\r?\n/).filter(line => /^(?:## Candidate \d+ — |### |- )/.test(line));
for (const id of artAssetIds) {
  const matches = briefLines.filter(line => line.includes("`" + id + "`"));
  assert.equal(matches.length, 1, "expected exactly one production brief heading/bullet for " + id);
}
console.log("PASS: 9/9 Day01 art IDs have production briefs (PNG delivery not checked)");

// Exhaustive static Day01 choice-space guard (3 x 3 x 3), not a Core simulation.
// The fourth room-resolution choice is deliberately outside this matrix.
let combinationsChecked = 0;
let lowestClosingCash = money(fixture.opening.budgetKopecks);
let highestClosingCash = 0n;
for (const lab of fixture.visitors[0].choices) {
  for (const student of fixture.visitors[1].choices) {
    for (const partner of fixture.visitors[2].choices) {
      const selected = [lab, student, partner];
      const inspections = new Set(selected.map(choice => choice.requiresInspection).filter(Boolean));
      const inspectionCost = [...inspections].reduce((total, documentId) => {
        const document = fixture.documents.find(item => item.id === documentId);
        assert.ok(document, "unknown inspection document: " + documentId);
        assert.ok(Number.isSafeInteger(document.inspection.costTokens) && document.inspection.costTokens >= 0);
        return total + document.inspection.costTokens;
      }, 0);
      assert.ok(inspectionCost <= fixture.opening.inspectionTokens,
        "Day01 choice combination requires unavailable inspections");
      let cash = money(fixture.opening.budgetKopecks);
      const scores = {...fixture.opening.scores};
      for (const choice of selected) {
        for (const effect of choice.effects) {
          if (effect.kind === "cashDelta") {
            cash += money(effect.deltaKopecks);
            assert.ok(cash >= 0n, "Day01 combination overspends available cash");
            assert.ok(cash <= (1n << 63n) - 1n, "Day01 cash overflows Int64");
          } else if (effect.kind === "scoreDelta") {
            scores[effect.score] += effect.delta;
            assert.ok(scores[effect.score] >= 0 && scores[effect.score] <= 100,
              "Day01 combination score outside Core bounds: " + effect.score);
          } else assert.fail("unknown Day01 effect kind");
        }
      }
      // Every choice triple must expose only valid room outcomes; priority never books a room.
      const claimants = selected.flatMap(choice => {
        const [kind, value] = Object.entries(choice.flags)[0];
        return (kind === "LAB" && ["full", "phased"].includes(value)) ||
          (kind === "STUDENT" && value === "full") ||
          (kind === "PARTNER" && value === "onsite") ? [kind.toLowerCase()] : [];
      });
      const availableRoomChoices = claimants.length > 1
        ? fixture.roomResolutionChoices.ifMultiple
        : fixture.roomResolutionChoices.ifAtMostOne;
      const expectedRoomIds = claimants.length > 1
        ? ["ROOM-1", "ROOM-2", "ROOM-3", "ROOM-4"]
        : ["ROOM-AUTO"];
      assert.deepEqual(availableRoomChoices.map(choice => choice.id).sort(), expectedRoomIds.sort(),
        "room choice IDs must be stable for every Day01 route");
      for (const roomChoice of availableRoomChoices) {
        if (roomChoice.id === "ROOM-AUTO") {
          assert.equal(roomChoice.priority, "only-request-or-none");
        } else if (roomChoice.id === "ROOM-4") {
          assert.equal(roomChoice.priority, "unresolved");
        } else {
          const expectedPriority = {"ROOM-1": "lab", "ROOM-2": "student", "ROOM-3": "partner"}[roomChoice.id];
          assert.equal(roomChoice.priority, expectedPriority,
            "room choice ID must preserve its canonical priority: " + roomChoice.id);
        }
      }
      if (cash < lowestClosingCash) lowestClosingCash = cash;
      if (cash > highestClosingCash) highestClosingCash = cash;
      combinationsChecked++;
    }
  }
}
assert.equal(combinationsChecked, 27, "Day01 must enumerate all 27 document-choice combinations");
assert.equal(lowestClosingCash.toString(), "171500000", "Day01 minimum cash changed: rebalance explicitly");
assert.equal(highestClosingCash.toString(), "200000000", "Day01 maximum cash changed: rebalance explicitly");
console.log("PASS: 27/27 Day01 document-choice combinations, inspections, cash and Q/S/M/E/R bounds; closing cash 171500000..200000000 kopecks");
