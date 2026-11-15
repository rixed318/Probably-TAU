#!/usr/bin/env node
"use strict";
// Draft content-only financial wire validation. No Core ledger mutation.
const fs = require("node:fs");
const path = require("node:path");
const assert = require("node:assert/strict");
const INT64_MIN = -(1n << 63n);
const INT64_MAX = (1n << 63n) - 1n;
const CANONICAL = /^(0|-?[1-9][0-9]*)$/;
function validateMoney(value) {
  if (typeof value !== "string" || !CANONICAL.test(value)) return false;
  const amount = BigInt(value);
  return amount >= INT64_MIN && amount <= INT64_MAX;
}
function inspect(value, pointer = "$", errors = [], moneyPaths = []) {
  if (Array.isArray(value)) {
    value.forEach((item, i) => inspect(item, pointer + "[" + i + "]", errors, moneyPaths));
  } else if (value !== null && typeof value === "object") {
    for (const [key, item] of Object.entries(value)) {
      const at = pointer + "." + key;
      if (key.endsWith("Kopecks")) {
        moneyPaths.push(at);
        if (!validateMoney(item)) errors.push(at + ": expected canonical signed Int64 decimal string");
      } else {
        inspect(item, at, errors, moneyPaths);
      }
    }
  }
  return { errors, moneyPaths };
}
const spec = JSON.parse(fs.readFileSync(path.join(__dirname, "CONTENT_SPEC_DRAFT.json"), "utf8"));
const baseline = inspect(spec);
assert.equal(spec.meta.specVersion, "content-draft-0.3");
assert.equal(baseline.moneyPaths.length, 21, "money field inventory changed; review new fields");
assert.deepEqual(baseline.errors, [], "draft content financial fields");
const clone = x => JSON.parse(JSON.stringify(x));
function fails(value) {
  const altered = clone(spec);
  altered.opening.budgetKopecks = value;
  assert.equal(inspect(altered).errors.length, 1, "expected rejection: " + String(value));
}
fails(200000000);                 // JSON number, even when currently safe
fails(1.5);                       // float
fails("01");                      // leading zeros
fails("-0");                      // noncanonical signed zero
fails("+1");                      // plus prefix
fails("9223372036854775808");    // above signed Int64
fails("-9223372036854775809");   // below signed Int64
fails("1e3");                     // exponent notation
fails(null);                      // absent wire value
fails(true);                      // boolean, not a money amount
fails(" 1");                      // whitespace
fails("1.00");                    // fractional representation
assert.equal(validateMoney("9007199254740993"), true, "large exact Int64 decimal string");
function failsAt(path, value) {
  const altered = clone(spec);
  let target = altered;
  for (const segment of path.slice(0, -1)) target = target[segment];
  target[path[path.length - 1]] = value;
  const errors = inspect(altered).errors;
  assert.equal(errors.length, 1, "expected one rejection at " + path.join("."));
  assert.ok(errors[0].includes(path.join(".")), "expected field-specific diagnostic");
}
failsAt(["opening", "weeklyRevenueKopecks"], 34000000);
// Nested money fields must use the same exact wire format as opening fields.
failsAt(["documentTypes", 0, "visible", "amountKopecks"], 22000000);
failsAt(["documentTypes", 2, "visible", "conditionalRevenueKopecks"], "1e7");
failsAt(["day1", "visits", 0, "choices", 0, "effects", 0, "deltaKopecks"], "-01");

assert.equal(validateMoney("-9223372036854775808"), true);
assert.equal(validateMoney("9223372036854775807"), true);
assert.equal(validateMoney("0"), true);
console.log("PASS: 21 money fields; 16 negative mutations; 4 boundary positives");
