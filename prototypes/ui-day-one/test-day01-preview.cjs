"use strict";
const test = require("node:test");
const assert = require("node:assert/strict");
const fs = require("node:fs");
const path = require("node:path");
const vm = require("node:vm");
const { model, getSummary, getRoomConflictNotice, formatRub } = require("./day01-read-model.cjs");
const source = fs.readFileSync(path.join(__dirname, "day01-preview.js"), "utf8");
const html = fs.readFileSync(path.join(__dirname, "day01-preview.html"), "utf8");

function element(tag) {
  return {
    tag, children: [], attributes: {}, textContent: "", className: "",
    appendChild(child) { this.children.push(child); return child; },
    replaceChildren() { this.children = []; },
    setAttribute(key, value) { this.attributes[key] = value; }
  };
}
function run() {
  const nodes = Object.fromEntries(["day01-summary", "day01-conflicts", "day01-documents"].map(id => [id, element("div")]));
  const document = { getElementById: id => nodes[id], createElement: element };
  const window = { document, TAU_DAY01_READ_MODEL: { model, getSummary, getRoomConflictNotice, formatRub } };
  vm.runInNewContext(source, { window, document }, { filename: "day01-preview.js" });
  return nodes;
}
test("standalone page loads canonical fixture, not legacy mock gateway", () => {
  assert.match(html, /src="\.\/day01-read-model\.cjs"/);
  assert.match(html, /src="\.\/day01-preview\.js"/);
  assert.doesNotMatch(html, /src="\.\/fixtures\.js"/);
  assert.doesNotMatch(html, /src="\.\/app\.js"/);
});
test("renders one accessible conflict and three unconfirmed document requests", () => {
  const nodes = run();
  assert.match(nodes["day01-summary"].textContent, /2 000 000 ₽/);
  assert.equal(nodes["day01-conflicts"].children.length, 1);
  const alert = nodes["day01-conflicts"].children[0];
  assert.equal(alert.attributes.role, "status");
  assert.match(alert.children[1].textContent, /DOC-01, DOC-02, DOC-03/);
  assert.match(alert.children[2].textContent, /Ни одна не является подтверждённой бронью/);
  assert.equal(nodes["day01-documents"].children.length, 3);
  assert.equal(nodes["day01-documents"].children.every(x => x.children.some(p => p.textContent.includes("Не подтверждено"))), true);
});
test("conditional partner income is labelled future and not credited", () => {
  const nodes = run();
  const partner = nodes["day01-documents"].children[2];
  assert.match(partner.children.at(-1).textContent, /120 000 ₽.*не начислен/);
  assert.equal(model.documents[2].recognizedRevenueKopecks, "0");
});
test("renderer does not use HTML injection or mutate gateway", () => {
  assert.doesNotMatch(source, /innerHTML|outerHTML|eval\s*\(/);
  assert.doesNotMatch(source, /\.execute\s*\(|createGateway|GameState|LedgerService/);
});

test("keyboard skip link reaches a focusable document heading with visible focus", () => {
  assert.match(html, /<a class="skip-link" href="#day01-documents-heading">Перейти к документам<\/a>/);
  assert.match(html, /<h2 id="day01-documents-heading" tabindex="-1">/);
  assert.match(html, /\.skip-link:focus-visible\s*\{[^}]*top:\s*\.5rem/);
  assert.match(html, /:focus-visible\s*\{[^}]*outline:\s*3px/);
});

test("preview has narrow-screen layout, readable wrapping and reduced-motion guard", () => {
  assert.match(html, /@media \(max-width: 480px\)/);
  assert.match(html, /overflow-wrap:\s*anywhere/);
  assert.match(html, /@media \(prefers-reduced-motion: reduce\)/);
});

test("missing fixture shows an accessible error instead of an infinite loading message", () => {
  const nodes = Object.fromEntries(["day01-summary", "day01-conflicts", "day01-documents"].map(id => [id, element("div")]));
  nodes["day01-conflicts"].appendChild(element("p"));
  nodes["day01-documents"].appendChild(element("article"));
  const document = { getElementById: id => nodes[id], createElement: element };
  vm.runInNewContext(source, { window: { document }, document }, { filename: "day01-preview.js" });
  assert.equal(nodes["day01-summary"].attributes.role, "alert");
  assert.match(nodes["day01-summary"].textContent, /Не удалось загрузить данные/);
  assert.equal(nodes["day01-conflicts"].children.length, 0);
  assert.equal(nodes["day01-documents"].children.length, 0);
});

test("broken read-model functions fail closed with an alert and no partial cards", () => {
  const nodes = Object.fromEntries(["day01-summary", "day01-conflicts", "day01-documents"].map(id => [id, element("div")]));
  const document = { getElementById: id => nodes[id], createElement: element };
  const broken = {
    getSummary: () => ({ dayNumber: "01", openingBalance: "2 000 000 ₽", documentCount: 3 }),
    getRoomConflictNotice: () => { throw Error("corrupt fixture"); },
    formatRub, model
  };
  vm.runInNewContext(source, { window: { document, TAU_DAY01_READ_MODEL: broken }, document }, { filename: "day01-preview.js" });
  assert.equal(nodes["day01-summary"].attributes.role, "alert");
  assert.equal(nodes["day01-conflicts"].children.length, 0);
  assert.equal(nodes["day01-documents"].children.length, 0);
});
