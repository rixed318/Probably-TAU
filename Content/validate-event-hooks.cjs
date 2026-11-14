#!/usr/bin/env node
// Static draft fixture validator: node Content/validate-event-hooks.cjs
"use strict";
const fs=require("node:fs"),path=require("node:path"),assert=require("node:assert/strict");
const spec=JSON.parse(fs.readFileSync(path.join(__dirname,"EVENT_HOOKS_DRAFT.json"),"utf8"));
const validate=(s)=>{
const errors=[],defs=s.definitions,ids=new Set();
function checkCond(c,path){if(!c||typeof c!=="object")return errors.push(path+" missing condition");if(c.op==="all"||c.op==="any"){if(!Array.isArray(c.items)||!c.items.length)errors.push(path+" empty group");else c.items.forEach((x,i)=>checkCond(x,path+"/"+i));}else if(c.op==="always"){}else if(c.op==="flagEquals"){if(typeof c.key!=="string"||typeof c.value!=="string")errors.push(path+" flag");}else if(c.op==="factExists"){if(typeof c.factId!=="string")errors.push(path+" fact");}else if(c.op==="dayIndexAtLeast"){if(!Number.isInteger(c.value)||c.value<0)errors.push(path+" day");}else errors.push(path+" unknown op");}
for(const d of defs){if(ids.has(d.definitionId))errors.push("duplicate "+d.definitionId);ids.add(d.definitionId);if(!Number.isInteger(d.dueDayIndex)||d.dueDayIndex<0)errors.push("invalid day "+d.definitionId);checkCond(d.condition,d.definitionId);for(const e of d.effects){if(e.type==="proposeEarnedIncome"){if(!/^(0|[1-9][0-9]*)$/.test(e.deltaKopecks)||BigInt(e.deltaKopecks)>9223372036854775807n)errors.push("invalid money "+d.definitionId);if(!e.earnedIdTemplate||!e.postingKeyTemplate||!e.earnedIdTemplate.includes("{sourceInstanceId}"))errors.push("unstable posting "+d.definitionId);if(d.dueDayIndex<4)errors.push("premature income "+d.definitionId);if(!JSON.stringify(d.condition).includes("E07_delivered"))errors.push("missing delivery "+d.definitionId);}else if(!["presentEvent","showSettlementReport"].includes(e.type))errors.push("unknown effect "+d.definitionId);if(d.definitionId==="E09"&&e.type!=="showSettlementReport")errors.push("E09 double settlement");}}
// Static guard for ADR-C0-002 documentation metadata; NOT a runtime queue test.
const contract=s.instanceContract||{};
for(const field of ["instanceId","definitionId","priority","receiptKey","postingKey","queueOrdering","dueSnapshot","sameDayChildren"])
  if(typeof contract[field]!=="string"||!contract[field].trim())errors.push("missing ADR-C0-002 "+field);
if(typeof contract.instanceId==="string"&&!contract.instanceId.includes("StableEventId"))errors.push("instanceId must be StableEventId");
if(typeof contract.priority==="string"&&(!contract.priority.includes("Int32")||!contract.priority.includes("0 once")))errors.push("missing persisted Int32 default priority");
if(typeof contract.queueOrdering==="string"&&(!contract.queueOrdering.includes("dueDayIndex ascending")||!contract.queueOrdering.includes("priority descending")||!contract.queueOrdering.includes("instanceId ascending")||!contract.queueOrdering.includes("ordinal")))errors.push("wrong due-event ordering");
if(typeof contract.dueSnapshot==="string"&&!contract.dueSnapshot.includes("after project ticks"))errors.push("wrong due-stage snapshot");
if(typeof contract.sameDayChildren==="string"&&(!contract.sameDayChildren.includes("dueDayIndex <= n")||!contract.sameDayChildren.includes("atomically")))errors.push("same-day child policy missing");
for(const [id,day] of [["E02",1],["E03",1],["E09",4]])if(!defs.some(d=>d.definitionId===id&&d.dueDayIndex===day))errors.push("missing schedule "+id);
return errors;
};
assert.deepEqual(validate(spec),[],"valid fixture");
assert.ok(validate({...spec,definitions:[...spec.definitions,spec.definitions[0]]}).some(x=>x.includes("duplicate")));
assert.ok(validate({...spec,definitions:spec.definitions.map(d=>d.definitionId==="PARTNER_REVENUE_ONSITE"?{...d,dueDayIndex:0}:d)}).length>0);
assert.ok(validate({...spec,definitions:spec.definitions.map(d=>d.definitionId==="E09"?{...d,effects:[{type:"proposeEarnedIncome",deltaKopecks:"100",earnedIdTemplate:"partner/{sourceInstanceId}",postingKeyTemplate:"x"}]}:d)}).length>0);
assert.ok(validate({...spec,definitions:spec.definitions.map(d=>d.definitionId==="PARTNER_REVENUE_REMOTE"?{...d,effects:[{...d.effects[0],deltaKopecks:1.5}]}:d)}).length>0);
// Metadata drift negatives: static assertions only, not executable Core scheduling.
const mutateContract=(field,value)=>({...spec,instanceContract:{...spec.instanceContract,[field]:value}});
assert.ok(validate(mutateContract("queueOrdering","dueDayIndex ascending, phase, creationOrdinal")).some(x=>x.includes("ordering")));
assert.ok(validate(mutateContract("priority","priority omitted")).some(x=>x.includes("priority")));
assert.ok(validate(mutateContract("sameDayChildren","defer silently")).some(x=>x.includes("child policy")));
assert.ok(validate(mutateContract("instanceId","definitionId")).some(x=>x.includes("StableEventId")));
console.log("PASS: 9 deterministic fixture/negative checks (including 4 ADR metadata drift checks)");
