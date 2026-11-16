(function(root){"use strict";
const model=root.TAU_DAY01_READ_MODEL, api=root.TAU_DAY01_GAMEPLAY, d=root.document;
if(!d||!model||!api)return;
const g=api.createGateway(), el=id=>d.getElementById(id);
function node(parent,tag,value){const n=d.createElement(tag);n.textContent=String(value);parent.appendChild(n);return n;}
function money(v){const n=BigInt(v),sign=n<0n?"−":"";return sign+model.formatRub((n<0n?-n:n).toString());}
function clear(id){el(id).replaceChildren();}
function button(parent,label,fn,disabled=false,pressed=null){const b=node(parent,"button",label);b.type="button";b.disabled=disabled;if(pressed!==null)b.setAttribute("aria-pressed",String(pressed));b.addEventListener("click",()=>{try{fn();el("error").textContent="";render();}catch(e){el("error").textContent=e.message;}});return b;}
function render(){
 const focusedQueueIndex=Array.from(el("queue").querySelectorAll("button")).indexOf(d.activeElement);
 const s=g.view(), item=s.documents.find(x=>x.id===s.selected), done=new Set(s.decisions.map(x=>x.id));
 el("hud").textContent="День "+s.dayNumber+" · Бюджет: "+money(s.balanceKopecks)+" · Q "+s.metrics.Q+" / S "+s.metrics.S+" / M "+s.metrics.M+" / E "+s.metrics.E+" / R "+s.metrics.R;
 el("progress").textContent="Решений: "+done.size+" из 3";
 clear("queue");for(const doc of s.documents)button(el("queue"),doc.id+" · "+doc.visitor+(done.has(doc.id)?" ✓":""),()=>g.select(doc.id),false,doc.id===s.selected);
 if(focusedQueueIndex>=0){const next=el("queue").querySelectorAll("button")[focusedQueueIndex];if(next)next.focus();}
 el("doc-title").textContent=item.id+" · "+item.title;clear("doc-body");
 node(el("doc-body"),"p",item.visitor+" — "+item.role);
 node(el("doc-body"),"p","Запрос на Л-204, день 04, 14:00–17:00 (не бронь)");
 if(item.amountKopecks)node(el("doc-body"),"p","Запрошено: "+money(item.amountKopecks));
 else node(el("doc-body"),"p","Условный будущий доход: "+money(item.conditionalRevenueKopecks)+"; при подписании 0 ₽");
 el("inspect").disabled=done.has(item.id)||s.inspected.includes(item.id);
 el("inspection").textContent=s.inspected.includes(item.id)?"Проверка: "+Object.entries(item.revealed).map(([k,v])=>k+": "+(k.toLowerCase().includes("kopecks")?money(v):v)).join(" · "):"Проверка ещё не выполнена";
 clear("choices");if(done.has(item.id))node(el("choices"),"p","Решение зафиксировано: "+s.decisions.find(x=>x.id===item.id).label);
 else for(const option of g.options(item.id))button(el("choices"),option.label,()=>g.decide(item.id,option.id),!option.enabled);
 el("end").disabled=done.size!==3;
 if(s.closed){el("game").hidden=true;el("report").hidden=false;clear("report-body");
 node(el("report-body"),"p","Старт: "+money(s.report.openingKopecks)+" · Остаток: "+money(s.report.closingKopecks));
 const ul=node(el("report-body"),"ul","");for(const x of s.report.decisions)node(ul,"li",x.id+" — "+x.label);
 node(el("report-body"),"h3","Ledger (только mock)");const ll=node(el("report-body"),"ul","");
 for(const row of s.report.ledger)node(ll,"li",row.key+": "+money(row.deltaKopecks)+" · "+row.description);
 node(el("report-body"),"p","Q/S/M/E/R: "+Object.values(s.report.metrics).join(" / "));
 for(const note of s.report.notes)node(el("report-body"),"p",note);
 el("report").focus();
 }
}
el("inspect").addEventListener("click",()=>{try{g.inspect(g.view().selected);render();}catch(e){el("error").textContent=e.message;}});
el("end").addEventListener("click",()=>{try{g.endDay();render();}catch(e){el("error").textContent=e.message;}});
render();
})(typeof window!=="undefined"?window:globalThis);
