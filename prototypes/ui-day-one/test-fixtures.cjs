"use strict";
const test = require("node:test");
const assert = require("node:assert/strict");
const fs = require("node:fs");
const path = require("node:path");
const vm = require("node:vm");

const source = fs.readFileSync(path.join(__dirname, "fixtures.js"), "utf8");
const sandbox = { window: {} };
vm.runInNewContext(source, sandbox, { filename: "fixtures.js" });
const newGateway = () => sandbox.window.TAU_UI_MOCK.createGateway();
const cmd = (type, documentId = "TAU-001") => ({ type, documentId });

test("the first-day read model has an outstanding visitor and static reserve", async () => {
  const gateway = newGateway();
  const view = await gateway.queryDashboard();
  assert.equal(view.budget, "1 500 000 ₽");
  assert.equal(view.resolved, false);
  assert.equal(view.visitorName, "Алина Демина");
});

test("invalid entity and premature AdvanceDay return explicit errors", async () => {
  const gateway = newGateway();
  assert.equal((await gateway.execute(cmd("InspectDocument", "missing"))).ok, false);
  const early = await gateway.execute(cmd("AdvanceDay"));
  assert.equal(early.ok, false);
  assert.match(early.error, /Сначала/);
});

test("inspect and clarify reveal fixed facts, without financial mutation", async () => {
  const gateway = newGateway();
  const inspected = await gateway.execute(cmd("InspectDocument"));
  assert.equal(inspected.ok, true);
  assert.equal(inspected.view.allowInspect, false);
  assert.equal(inspected.view.budget, "1 500 000 ₽");
  const clarified = await gateway.execute(cmd("RequestClarification"));
  assert.equal(clarified.view.amount, "210 000 ₽");
  assert.equal(clarified.view.budget, "1 500 000 ₽");
  assert.equal((await gateway.execute(cmd("InspectDocument"))).ok, false);
});

test("approve clarified quote then show canned ledger and day 2", async () => {
  const gateway = newGateway();
  await gateway.execute(cmd("RequestClarification"));
  const approved = await gateway.execute(cmd("ApproveProposal"));
  assert.equal(approved.view.budget, "1 290 000 ₽");
  assert.equal(approved.view.staffTrust, "63 / 100");
  assert.equal((await gateway.execute(cmd("ApproveProposal"))).ok, false);
  const end = await gateway.execute(cmd("AdvanceDay"));
  assert.equal(end.ok, true);
  assert.equal(end.report.balance, "1 255 000 ₽");
  assert.equal(end.report.ledger.length, 2);
  const again = await gateway.execute(cmd("AdvanceDay"));
  assert.equal(again.ok, false);
  assert.match(again.error, /уже завершён/);
  assert.equal((await gateway.queryDashboard()).budget, "1 290 000 ₽");
  const next = await gateway.execute(cmd("ShowDayTwo"));
  assert.equal(next.view.dayTwo, true);
  assert.equal(next.view.budget, "1 255 000 ₽");
});

test("reject retains initial reserve until end-of-day expenses", async () => {
  const gateway = newGateway();
  const rejected = await gateway.execute(cmd("RejectProposal"));
  assert.equal(rejected.view.budget, "1 500 000 ₽");
  assert.equal(rejected.view.staffTrust, "58 / 100");
  const end = await gateway.execute(cmd("AdvanceDay"));
  assert.equal(end.report.balance, "1 465 000 ₽");
  const next = await gateway.execute(cmd("ShowDayTwo"));
  assert.equal(next.view.staffTrust, "58 / 100");
});

test("defer retains a pending deadline in the mock read model", async () => {
  const gateway = newGateway();
  const postponed = await gateway.execute(cmd("DeferProposal"));
  assert.equal(postponed.view.resolved, true);
  assert.equal(postponed.view.deadline, "1 день");
  assert.equal((await gateway.execute(cmd("InspectDocument"))).ok, false);
  const end = await gateway.execute(cmd("AdvanceDay"));
  assert.equal(end.report.balance, "1 465 000 ₽");
});


test("ShowDayTwo requires a successful AdvanceDay, not merely a decision", async () => {
  const gateway = newGateway();
  assert.equal((await gateway.execute(cmd("ShowDayTwo"))).ok, false);
  await gateway.execute(cmd("ApproveProposal"));
  const premature = await gateway.execute(cmd("ShowDayTwo"));
  assert.equal(premature.ok, false);
  assert.match(premature.error, /Сначала завершите день/);
  assert.equal((await gateway.queryDashboard()).dayTwo, false);
  const end = await gateway.execute(cmd("AdvanceDay"));
  assert.equal(end.ok, true);
  assert.equal(end.report.balance, "1 225 000 ₽");
  assert.equal((await gateway.execute(cmd("AdvanceDay"))).ok, false);
  assert.equal((await gateway.execute(cmd("RejectProposal"))).ok, false);
  const next = await gateway.execute(cmd("ShowDayTwo"));
  assert.equal(next.ok, true);
  assert.equal(next.view.budget, end.report.balance);
  assert.equal((await gateway.execute(cmd("ShowDayTwo"))).ok, false);
});

test("each resolved branch advances exactly once", async () => {
  for (const decision of ["ApproveProposal", "RejectProposal", "DeferProposal"]) {
    const gateway = newGateway();
    assert.equal((await gateway.execute(cmd(decision))).ok, true);
    assert.equal((await gateway.execute(cmd("ShowDayTwo"))).ok, false);
    const first = await gateway.execute(cmd("AdvanceDay"));
    assert.equal(first.ok, true);
    assert.equal((await gateway.execute(cmd("AdvanceDay"))).ok, false);
    assert.equal((await gateway.execute(cmd("InspectDocument"))).ok, false);
    const next = await gateway.execute(cmd("ShowDayTwo"));
    assert.equal(next.ok, true);
    assert.equal(next.view.budget, first.report.balance);
  }
});
