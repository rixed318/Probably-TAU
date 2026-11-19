'use strict';
// Issue #20: executable static gate for the EXACT distributed single-file HTML.
// This is not Chromium interaction, a Unity test, or a gameplay assertion.
const test = require('node:test');
const assert = require('node:assert/strict');
const fs = require('node:fs');
const path = require('node:path');
const vm = require('node:vm');

const htmlPath = path.resolve(__dirname, '../../ui-source/prototypes/ui-day-one/day01-android-offline.html');
const html = fs.readFileSync(htmlPath, 'utf8');

test('Android distribution is a self-contained HTML document', () => {
  assert.match(html, /^\s*<!doctype html>/i);
  assert.match(html, /<meta\s+name=["']viewport["'][^>]*viewport-fit=cover/i);
  assert.doesNotMatch(html, /<(?:script|link|img|iframe|source)\b[^>]*(?:src|href)\s*=\s*["'](?:https?:|\/\/)/i);
  assert.doesNotMatch(html, /<script\b[^>]*\bsrc\s*=/i);
  assert.doesNotMatch(html, /\b(?:fetch\s*\(|XMLHttpRequest|WebSocket|EventSource|importScripts\s*\()/);
});

test('Every embedded JavaScript block parses without execution', () => {
  const scripts = [...html.matchAll(/<script\b([^>]*)>([\s\S]*?)<\/script\s*>/gi)];
  assert.ok(scripts.length >= 1, 'expected at least one inline script');
  scripts.forEach((match, index) => {
    assert.doesNotMatch(match[1], /\bsrc\s*=/i);
    assert.ok(match[2].trim(), 'inline script must not be empty');
    assert.doesNotThrow(() => new vm.Script(match[2], {filename: 'offline-inline-' + index + '.js'}));
  });
});

test('Mobile CSS and day-end vocabulary are present (static only)', () => {
  assert.match(html, /touch-action\s*:\s*manipulation/i);
  assert.match(html, /safe-area-inset/i);
  assert.match(html, /EndDay|endDay/);
  assert.match(html, /ledger/i);
});
