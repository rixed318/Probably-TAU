/* Isolated fictional browser demo; not Core, SaveService or authoritative GameState. */
(function(root) {
"use strict";
const source = typeof module === "object" && module.exports ? require("./day01-read-model.cjs") : root.TAU_DAY01_READ_MODEL;
const OPTIONS = Object.freeze({
 "DOC-01": [{id:"full",label:"Полная закупка · 220 000 ₽",cost:"22000000",score:{Q:7,M:2,R:-1}},{id:"phase",label:"Поэтапная закупка · 160 000 ₽ (после проверки)",cost:"16000000",needsInspect:true,score:{Q:5,M:1,R:2}},{id:"reject",label:"Отклонить",cost:"0",score:{Q:-3,M:-2,R:-1}}],
 "DOC-02": [{id:"full",label:"Поддержать проект · 65 000 ₽",cost:"6500000",score:{S:7,R:3}},{id:"mobile",label:"Мобильный формат · 35 000 ₽ (после проверки)",cost:"3500000",needsInspect:true,score:{S:5,R:2}},{id:"reject",label:"Отклонить",cost:"0",score:{S:-4,R:-2}}],
 "DOC-03": [{id:"onsite",label:"Принять с очным наставником · 40 000 ₽",cost:"4000000",score:{E:6,Q:2}},{id:"remote",label:"Дистанционный наставник · 25 000 ₽ (после проверки)",cost:"2500000",needsInspect:true,score:{E:4,Q:1}},{id:"reject",label:"Отказаться",cost:"0",score:{E:-4,R:-1}}]
});
const clone = x => JSON.parse(JSON.stringify(x));
function createGateway() {
 const docs=source.model.documents, opening=BigInt(source.model.openingBalanceKopecks);
 let balance=opening, selected=docs[0].id, closed=false, report=null;
 const metrics={Q:52,S:48,M:50,E:45,R:47}, inspected=new Set(), decisions=new Map(), ledger=[];
 function assertOpen(){if(closed)throw Error("День уже завершён");}
 function valid(id){if(!docs.some(d=>d.id===id))throw Error("Неизвестный документ");}
 function view(){return clone({dayIndex:0,dayNumber:1,selected,balanceKopecks:balance.toString(),metrics,inspected:[...inspected],decisions:[...decisions.values()],ledger,closed,report,documents:docs,conflicts:source.getRoomConflictNotice()});}
 return Object.freeze({
  view,
  select(id){assertOpen();valid(id);selected=id;return view();},
  inspect(id){assertOpen();valid(id);if(decisions.has(id))throw Error("Решение уже принято");inspected.add(id);return view();},
  options(id){valid(id);return clone(OPTIONS[id].map(o=>({...o,enabled:!decisions.has(id)&&(!o.needsInspect||inspected.has(id))})));},
  decide(id,choice){assertOpen();valid(id);if(decisions.has(id))throw Error("Повторное решение запрещено");
   const option=OPTIONS[id].find(o=>o.id===choice);if(!option)throw Error("Неизвестное решение");
   if(option.needsInspect&&!inspected.has(id))throw Error("Сначала проверьте документ");
   const cost=BigInt(option.cost);if(cost>balance)throw Error("Недостаточно средств");
   balance-=cost;for(const [k,v] of Object.entries(option.score))metrics[k]=Math.max(0,Math.min(100,metrics[k]+v));
   const decision={id,choice,label:option.label,costKopecks:cost.toString()};decisions.set(id,decision);
   if(cost)ledger.push({key:"day0:"+id,description:option.label,deltaKopecks:(-cost).toString()});
   return view();
  },
  endDay(){if(report)return view();if(decisions.size!==3)throw Error("Сначала рассмотрите все три документа");
   closed=true;report={dayIndex:0,openingKopecks:opening.toString(),closingKopecks:balance.toString(),decisions:[...decisions.values()],ledger:clone(ledger),metrics:clone(metrics),notes:["Бронь Л-204 не создавалась автоматически.","Доход партнёра при подписании не начисляется.","Отложенные последствия и SaveService не подключены."]};return view();
  }
 });
}
const api={createGateway};if(typeof module==="object"&&module.exports)module.exports=api;else root.TAU_DAY01_GAMEPLAY=api;
})(typeof window!=="undefined"?window:globalThis);
