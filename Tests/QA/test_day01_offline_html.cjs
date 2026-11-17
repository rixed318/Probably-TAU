// Independent Node regression against the exact checked-out UI HTML, not a mock reimplementation.
// Usage: node --test Tests/QA/test_day01_offline_html.cjs
const {test} = require('node:test');
const assert = require('node:assert/strict');
const fs = require('node:fs');
const path = require('node:path');
const vm = require('node:vm');
const htmlPath = path.resolve(__dirname, '../../prototypes/ui-day-one/day01-android-touch.html');
const html = fs.readFileSync(htmlPath, 'utf8');
const scripts = [...html.matchAll(/<script(?:\s[^>]*)?>([\s\S]*?)<\/script>/gi)].map(x => x[1]);
assert.ok(scripts.length >= 2, 'expected embedded read model and gameplay gateway');
const context = {console};
context.window = context;
context.globalThis = context;
vm.createContext(context);
vm.runInContext(scripts[0], context, {filename:'embedded-read-model.js'});
vm.runInContext(scripts[1], context, {filename:'embedded-gameplay.js'});
const make = () => context.TAU_DAY01_GAMEPLAY.createGateway();
const cost = rows => rows.reduce((sum,row)=>sum+BigInt(row.deltaKopecks),0n);
test('Day01: three documents and end-day gate', () => {
  const g=make();
  assert.equal(g.view().documents.length,3);
  assert.throws(()=>g.endDay(),/три документа/);
  for(const id of ['DOC-01','DOC-02','DOC-03'])g.decide(id,'reject');
  const result=g.endDay();
  assert.equal(result.closed,true);
  assert.equal(result.report.decisions.length,3);
  assert.equal(result.ledger.length,0);
  assert.deepEqual(g.endDay().report, result.report);
});
test('Day01: inspections gate discounted options; duplicate decisions cannot debit twice',()=>{
  const g=make();
  assert.throws(()=>g.decide('DOC-01','phase'),/проверьте/);
  g.inspect('DOC-01');
  g.decide('DOC-01','phase');
  const first=g.view();
  assert.equal(first.ledger.length,1);
  assert.equal(first.ledger[0].key,'day0:DOC-01');
  assert.throws(()=>g.decide('DOC-01','phase'),/Повторное/);
  assert.equal(g.view().balanceKopecks,first.balanceKopecks);
  assert.equal(g.view().ledger.length,1);
});
test('Day01: ledger replay and no immediate partner revenue',()=>{
  const g=make();
  g.decide('DOC-01','full');
  g.decide('DOC-02','full');
  g.decide('DOC-03','onsite');
  const v=g.endDay();
  const opening=BigInt(v.report.openingKopecks);
  assert.equal(opening+cost(v.report.ledger),BigInt(v.report.closingKopecks));
  assert.equal(v.report.ledger.length,3);
  assert.equal(new Set(v.report.ledger.map(x=>x.key)).size,3);
  assert.ok(v.report.ledger.every(x=>BigInt(x.deltaKopecks)<0n));
  assert.equal(v.documents[2].recognizedRevenueKopecks,'0');
  assert.equal(v.report.closingKopecks,'169500000');
  assert.throws(()=>g.decide('DOC-03','reject'),/завершён/);
});
