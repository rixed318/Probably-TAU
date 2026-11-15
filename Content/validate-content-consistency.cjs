#!/usr/bin/env node
"use strict";
// Cross-file contract drift guard for draft content only; not a Unity/runtime validator.
const fs=require("node:fs"),path=require("node:path"),assert=require("node:assert/strict");
const load=name=>JSON.parse(fs.readFileSync(path.join(__dirname,name),"utf8"));
function validateCrossFile(spec,hooks){
 const errors=[];
 // A Map silently overwrites duplicate IDs; reject ambiguous content before lookup.
 for(const [items,key,label] of [[spec.eventManifest,"id","eventManifest"],[hooks.definitions,"definitionId","definitions"]]){
  const seen=new Set();
  for(const item of items){
   const id=item[key];
   if(typeof id!=="string"||!id.trim())errors.push(`${label}: invalid ${key}`);
   else if(seen.has(id))errors.push(`${label}: duplicate ${key} ${id}`);
   else seen.add(id);
  }
 }
 // Validate every manifest entry, not just the E02/E03/E09 hook subset.
 // DayIndex is canonical 0-based; legacy human day must be exactly DayIndex+1.
 for(const e of spec.eventManifest){
  if(!Number.isSafeInteger(e.dayIndex)||e.dayIndex<0||!Number.isSafeInteger(e.day)||e.day!==e.dayIndex+1)
   errors.push(`${e.id}: invalid 0-based manifest calendar`);
 }
 // Every declared feed must resolve to an event, project, or explicit future chapter hook.
 // A feed is a narrative dependency, NOT a request to run the target immediately.
 const eventById=new Map(spec.eventManifest.map(e=>[e.id,e]));
 const projectIds=new Set((spec.projects||[]).map(p=>p.id));
 const externalHooks=new Set(["CHAPTER2_HOOK"]); // explicitly deferred outside current 13-event chapter
 for(const event of spec.eventManifest){
  if(!Array.isArray(event.feeds)){errors.push(`${event.id}: feeds must be an array`);continue;}
  const seenFeeds=new Set();
  for(const target of event.feeds){
   if(typeof target!=="string"||!target.trim()){errors.push(`${event.id}: invalid feed target`);continue;}
   if(seenFeeds.has(target))errors.push(`${event.id}: duplicate feed ${target}`);
   seenFeeds.add(target);
   if(target===event.id)errors.push(`${event.id}: self-referential feed`);
   const destination=eventById.get(target);
   if(destination){
    if(Number.isSafeInteger(event.dayIndex)&&Number.isSafeInteger(destination.dayIndex)&&destination.dayIndex<event.dayIndex)
     errors.push(`${event.id}: backward feed to ${target}`);
   }else if(!projectIds.has(target)&&!externalHooks.has(target))
    errors.push(`${event.id}: unresolved feed ${target}`);
  }
 }
 // Same-day feeds are allowed, but cycles make narrative dependencies impossible to resolve.
 // Inspect event-to-event edges only; projects and the deferred chapter hook are terminal metadata.
 // This does NOT execute or schedule events.
 const visited=new Set(),active=new Set(),reportedCycles=new Set();
 function visitFeed(id){
  if(active.has(id))return;
  if(visited.has(id))return;
  visited.add(id);active.add(id);
  const event=eventById.get(id);
  if(event&&Array.isArray(event.feeds)){
   for(const target of event.feeds){
    if(!eventById.has(target))continue;
    if(active.has(target)){
     const edge=id+" -> "+target;
     if(!reportedCycles.has(edge)){errors.push(`eventManifest: cyclic feed ${edge}`);reportedCycles.add(edge);}
    }else visitFeed(target);
   }
  }
  active.delete(id);
 }
 for(const event of spec.eventManifest)if(typeof event.id==="string")visitFeed(event.id);
 const manifest=new Map(spec.eventManifest.map(e=>[e.id,e]));
 const definitions=new Map(hooks.definitions.map(d=>[d.definitionId,d]));
 for(const id of ["E02","E03","E09"]){
  const e=manifest.get(id),d=definitions.get(id);
  if(!e||!d){errors.push(`${id}: missing manifest or hook`);continue;}
  if(d.dueDayIndex!==e.dayIndex)errors.push(`${id}: dayIndex mismatch`);
  if(e.dayIndex!==e.day-1)errors.push(`${id}: 0-based calendar mismatch`);
  if(d.fallbackDefinitionId!==id)errors.push(`${id}: fallback mismatch`);
 }
 const e09=definitions.get("E09");
 if(e09&&(e09.dispatch!=="settlement-report-only"||e09.effects.length!==1||e09.effects[0].type!=="showSettlementReport"||e09.effects[0].reportForDayIndex!==spec.opening.weeklySettlementFirstClosingDayIndex))errors.push("E09: report-only settlement mismatch");
 const choices=spec.day1.visits.find(v=>v.id==="D01_PARTNER")?.choices||[];
 const partnerDefs=hooks.definitions.filter(d=>d.definitionId.startsWith("PARTNER_REVENUE_"));
 const postingTemplates=new Set();
 for(const d of partnerDefs){
  const mode=d.definitionId.slice("PARTNER_REVENUE_".length).toLowerCase();
  const choice=choices.find(c=>c.flag?.PARTNER===mode);
  const effect=d.effects.find(e=>e.type==="proposeEarnedIncome");
  if(!choice||!effect){errors.push(`${mode}: missing choice/effect`);continue;}
  if(!/^(0|[1-9][0-9]*)$/.test(effect.deltaKopecks)||BigInt(effect.deltaKopecks)!==BigInt(choice.conditionalRevenueKopecks))errors.push(`${mode}: revenue mismatch`);
  if(d.dueDayIndex!==choice.revenueEarliestDayIndex||d.dueDayIndex<spec.opening.weeklySettlementFirstClosingDayIndex)errors.push(`${mode}: premature recognition`);
  // Required facts/branch must hold on EVERY successful condition path.
  // Merely finding a leaf under "any" is unsafe: another OR branch may bypass it.
  function guaranteedLeaves(node){
    if(!node||typeof node!=="object")return [];
    if(node.op==="all"&&Array.isArray(node.items))
      return node.items.flatMap(guaranteedLeaves);
    if(node.op==="any"&&Array.isArray(node.items)&&node.items.length){
      const branches=node.items.map(guaranteedLeaves);
      return branches[0].filter(leaf=>branches.slice(1).every(b=>b.some(x=>
        x.op===leaf.op&&x.factId===leaf.factId&&x.key===leaf.key&&x.value===leaf.value)));
    }
    return node.op==="factExists"||node.op==="flagEquals"?[node]:[];
  }
  const conditions=guaranteedLeaves(d.condition);
  for(const fact of choice.requirements)
    if(!conditions.some(c=>c.op==="factExists"&&c.factId===fact))
      errors.push(`${mode}: missing required fact ${fact}`);
  if(!conditions.some(c=>c.op==="flagEquals"&&c.key==="PARTNER"&&c.value===mode))
    errors.push(`${mode}: missing branch flag`);
  if(!effect.earnedIdTemplate?.includes("{sourceInstanceId}")||effect.postingKeyTemplate!=="earned/{earnedId}")errors.push(`${mode}: unstable posting template`);
  postingTemplates.add(effect.postingKeyTemplate);
 }
 if(partnerDefs.length!==2)errors.push("partner: expected two conditional branches");
 if(postingTemplates.size!==1)errors.push("partner: inconsistent idempotency namespace");
 return errors;
}
const spec=load("CONTENT_SPEC_DRAFT.json"),hooks=load("EVENT_HOOKS_DRAFT.json");
const clone=x=>JSON.parse(JSON.stringify(x));
assert.deepEqual(validateCrossFile(spec,hooks),[],"baseline content consistency");
let s=clone(spec);s.eventManifest.find(e=>e.id==="E03").dayIndex=3;
assert.ok(validateCrossFile(s,hooks).length>0,"schedule drift must fail");
s=clone(spec);s.day1.visits.find(v=>v.id==="D01_PARTNER").choices.find(c=>c.flag.PARTNER==="remote").conditionalRevenueKopecks=999;
assert.ok(validateCrossFile(s,hooks).length>0,"revenue drift must fail");
let h=clone(hooks);h.definitions.find(d=>d.definitionId==="PARTNER_REVENUE_ONSITE").condition.items=h.definitions.find(d=>d.definitionId==="PARTNER_REVENUE_ONSITE").condition.items.filter(x=>x.factId!=="E07_delivered");
assert.ok(validateCrossFile(spec,h).length>0,"missing delivery fact must fail");
h=clone(hooks);h.definitions.find(d=>d.definitionId==="PARTNER_REVENUE_ONSITE").condition.items
  .find(c=>c.op==="factExists"&&c.factId==="E07_delivered").factId="E07_delivered_fake";
assert.ok(validateCrossFile(spec,h).some(e=>e.includes("missing required fact E07_delivered")),
  "similar-looking fact ID must fail");
h=clone(hooks);h.definitions.find(d=>d.definitionId==="PARTNER_REVENUE_REMOTE").condition.items
  .find(c=>c.op==="flagEquals").key="OTHER_PARTNER";
assert.ok(validateCrossFile(spec,h).some(e=>e.includes("missing branch flag")),
  "wrong flag key must fail even with matching value");
h=clone(hooks);{
  const d=h.definitions.find(d=>d.definitionId==="PARTNER_REVENUE_ONSITE");
  const fact=d.condition.items.find(c=>c.op==="factExists"&&c.factId==="E07_delivered");
  d.condition.items=d.condition.items.filter(c=>c!==fact);
  d.condition.items.push({op:"any",items:[fact,{op:"always"}]});
}
assert.ok(validateCrossFile(spec,h).some(e=>e.includes("missing required fact E07_delivered")),
  "delivery fact under bypassable OR must fail");
h=clone(hooks);{
  const d=h.definitions.find(d=>d.definitionId==="PARTNER_REVENUE_REMOTE");
  const flag=d.condition.items.find(c=>c.op==="flagEquals");
  d.condition.items=d.condition.items.filter(c=>c!==flag);
  d.condition.items.push({op:"any",items:[flag,{op:"always"}]});
}
assert.ok(validateCrossFile(spec,h).some(e=>e.includes("missing branch flag")),
  "partner flag under bypassable OR must fail");
h=clone(hooks);h.definitions.find(d=>d.definitionId==="E09").effects.push({type:"proposeEarnedIncome"});
assert.ok(validateCrossFile(spec,h).length>0,"E09 double posting must fail");
s=clone(spec);s.eventManifest.push(clone(s.eventManifest.find(e=>e.id==="E02")));
assert.ok(validateCrossFile(s,hooks).some(e=>e.includes("eventManifest: duplicate id E02")),
  "duplicate manifest ID must fail even when both copies match");
h=clone(hooks);h.definitions.push(clone(h.definitions.find(d=>d.definitionId==="E09")));
assert.ok(validateCrossFile(spec,h).some(e=>e.includes("definitions: duplicate definitionId E09")),
  "duplicate hook definition ID must fail even when both copies match");
s=clone(spec);s.eventManifest.find(e=>e.id==="E14").dayIndex=10;
assert.ok(validateCrossFile(s,hooks).some(e=>e.includes("E14: invalid 0-based manifest calendar")),
  "late event E14 calendar drift must fail");
s=clone(spec);s.eventManifest.find(e=>e.id==="E10").dayIndex=1.5;
assert.ok(validateCrossFile(s,hooks).some(e=>e.includes("E10: invalid 0-based manifest calendar")),
  "fractional DayIndex must fail");
s=clone(spec);s.eventManifest.find(e=>e.id==="E14").feeds=["UNKNOWN_EVENT"];
assert.ok(validateCrossFile(s,hooks).some(e=>e.includes("E14: unresolved feed UNKNOWN_EVENT")),
  "unknown feed target must fail");
s=clone(spec);s.eventManifest.find(e=>e.id==="E07").feeds.push("E02");
assert.ok(validateCrossFile(s,hooks).some(e=>e.includes("E07: backward feed to E02")),
  "backward event feed must fail");
s=clone(spec);s.eventManifest.find(e=>e.id==="E10").feeds.push("P02");
assert.ok(validateCrossFile(s,hooks).some(e=>e.includes("E10: duplicate feed P02")),
  "duplicate project feed must fail");
s=clone(spec);s.eventManifest.find(e=>e.id==="E03").feeds.push("E02");
assert.ok(validateCrossFile(s,hooks).some(e=>e.includes("eventManifest: cyclic feed")),
  "same-day two-event cycle E02 -> E03 -> E02 must fail");
s=clone(spec);s.eventManifest.find(e=>e.id==="E05").feeds.push("E04");
s.eventManifest.find(e=>e.id==="E04").feeds.push("E05");
assert.ok(validateCrossFile(s,hooks).some(e=>e.includes("eventManifest: cyclic feed")),
  "same-day two-event cycle E04 -> E05 -> E04 must fail");
console.log("PASS: 18 cross-file fixture/negative checks");
