"use strict";
const test = require("node:test");
const assert = require("node:assert/strict");
const { createSelectionGateway } = require("./day01-selection-gateway.cjs");
const readModel = require("./day01-read-model.cjs");

test("queue selects all three canonical documents without altering the fixture", () => {
  const gateway = createSelectionGateway(readModel);
  assert.deepEqual(gateway.list(), ["DOC-01", "DOC-02", "DOC-03"]);
  assert.equal(gateway.selected().id, "DOC-01");
  for (const id of gateway.list()) {
    assert.equal(gateway.select(id), true);
    assert.equal(gateway.selected().id, id);
  }
  assert.equal(readModel.model.openingBalanceKopecks, "200000000");
  assert.equal(readModel.model.documents[2].recognizedRevenueKopecks, "0");
  assert.equal(Object.isFrozen(readModel.model), true);
});

test("invalid queue selection preserves selected document", () => {
  const gateway = createSelectionGateway(readModel);
  assert.equal(gateway.select("DOC-02"), true);
  assert.equal(gateway.select("NOT-A-DOCUMENT"), false);
  assert.equal(gateway.selected().id, "DOC-02");
  assert.notEqual(gateway.list(), gateway.list());
});
