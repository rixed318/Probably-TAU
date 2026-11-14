/* Presentation controller for the standalone UI concept. No GameState, no economy math. */
(function () {
  "use strict";
  if (!window.TAU_UI_MOCK) {
    document.body.textContent = "Не удалось загрузить демонстрационные данные.";
    return;
  }
  var gateway = window.TAU_UI_MOCK.createGateway();
  var vm = null;
  var activeTab = "overview";
  var working = false;
  var dayAdvanced = false;
  var lastReport = null;
  var toastTimer = null;
  function byId(id) { return document.getElementById(id); }
  function put(id, value) { var node = byId(id); if(node) node.textContent = value; }
  function empty(node) { while(node.firstChild) node.removeChild(node.firstChild); }
  function showToast(message) {
    var toast=byId("toast");
    toast.textContent=message;
    toast.hidden=false;
    if(toastTimer) clearTimeout(toastTimer);
    toastTimer=setTimeout(function(){toast.hidden=true;},4600);
  }
  function selectTab(name) {
    activeTab=name;
    document.querySelectorAll("[data-tab]").forEach(function(button){
      var selected=button.getAttribute("data-tab")===name;
      button.classList.toggle("active",selected);
      button.setAttribute("aria-selected",String(selected));
    });
    document.querySelectorAll("[data-section]").forEach(function(section){
      section.hidden=section.getAttribute("data-section")!==name;
    });
  }
  function renderList(id,items) {
    var list=byId(id); empty(list);
    items.forEach(function(text){
      var li=document.createElement("li");
      li.textContent=text;
      list.appendChild(li);
    });
  }
  function renderFactions(rows) {
    var list=byId("faction-list"); empty(list);
    rows.forEach(function(row){
      var div=document.createElement("div"); div.className="faction-item";
      var caption=document.createElement("span"); caption.textContent=row.label;
      var value=document.createElement("strong"); value.textContent=row.value+" / 100";
      var track=document.createElement("div"); track.className="faction-bar";
      var fill=document.createElement("b"); fill.style.width=Math.max(0,Math.min(100,row.value))+"%";
      track.appendChild(fill);div.appendChild(caption);div.appendChild(value);div.appendChild(track);list.appendChild(div);
    });
  }
  function render(view) {
    vm=view;
    put("hud-date",view.date);
    put("hud-budget",view.budget);
    put("hud-reputation",view.staffTrust);
    put("day-number",view.dayNumber);
    put("weekday",view.date);
    put("clock",view.clock);
    put("task-description",view.task);
    put("visitor-name",view.visitorName);
    put("visitor-role",view.visitorRole);
    put("visitor-quote",view.quote);
    put("dialogue-context",view.context);
    put("queue-name",view.visitorName);
    put("queue-role",view.visitorRole);
    put("queue-status",view.queueStatus);
    put("deadline-label",view.deadline);
    put("deadline-note",view.deadlineNote);
    put("status-explanation",view.explanation);
    put("document-title",view.docTitle);
    put("paper-title",view.docTitle);
    put("paper-author",view.author);
    put("paper-amount",view.amount);
    put("paper-deadline",view.docDeadline);
    put("paper-description",view.description);
    put("document-warning",view.warning);
    put("decision-help",view.decisionHelp);
    put("paper-stamp",view.resolved ? view.queueStatus : "НА РАССМОТРЕНИИ");
    put("evidence-help",view.allowInspect ? "Проверка сметы доступна." : view.allowClarify ? "Проверка выполнена. Можно запросить уточнение." : "Дополнительные действия недоступны.");
    renderList("facts-list",view.facts);
    renderList("history-list",view.history);
    renderFactions(view.factions);
    byId("guest-illustration").style.opacity=view.resolved ? ".6" : "1";
    byId("inspect-button").disabled=working||!view.allowInspect||view.resolved;
    byId("clarify-button").disabled=working||!view.allowClarify||view.resolved;
    ["approve-button","reject-button","defer-button"].forEach(function(id){
      byId(id).disabled=working||view.resolved||view.dayTwo;
    });
    ["scene-document","open-document","quick-document"].forEach(function(id){
      byId(id).disabled=working||view.dayTwo;
    });
    var endButton=byId("end-day");
    endButton.disabled=working||!view.resolved||view.dayTwo;
    endButton.textContent=dayAdvanced ? "Открыть отчёт ↗" : "Завершить день →";
    byId("open-document").textContent=view.resolved ? "Просмотреть решение ↗" : "Изучить заявку ↗";
    byId("guest-illustration").setAttribute("aria-label",view.visitorName);
  }
  function openDocument() {
    if (!vm||vm.dayTwo) {showToast("Этот экран пока не поддерживает новые заявки.");return;}
    selectTab("overview");
    var dialog=byId("document-dialog");
    if(!dialog.open) dialog.showModal();
  }
  async function send(type) {
    if(working) return;
    if(!vm||vm.dayTwo) {showToast("Сценарий первого дня уже завершён.");return;}
    working=true; render(vm);
    try {
      var result=await gateway.execute({type:type,documentId:"TAU-001",requestId:"ui-mock-001"});
      if (!result.ok) {showToast(result.error||"Команда отклонена.");return;}
      if(result.view) render(result.view);
      if(["ApproveProposal","RejectProposal","DeferProposal"].indexOf(type)>=0){
        byId("document-dialog").close();
        showToast(result.view.explanation);
      } else if(type==="InspectDocument") {
        selectTab("evidence");
        showToast("Проверка завершена. Появились новые сведения.");
      } else if(type==="RequestClarification"){
        selectTab("overview");
        showToast("Уточнённая смета получена: 210 000 ₽.");
      }
    } catch(error) {
      showToast("Не удалось выполнить действие: "+(error && error.message ? error.message : "ошибка запроса"));
    } finally {working=false;render(vm);}
  }
  function renderReport(report) {
    put("report-balance",report.balance);
    put("report-summary",report.summary);
    put("report-consequence",report.consequence);
    var ledger=byId("report-ledger");empty(ledger);
    report.ledger.forEach(function(entry){
      var line=document.createElement("div");
      line.className="ledger-row "+entry[2];
      var title=document.createElement("span");title.textContent=entry[0];
      var amount=document.createElement("strong");amount.textContent=entry[1];
      line.appendChild(title);line.appendChild(amount);ledger.appendChild(line);
    });
  }
  async function endDay() {
    if(dayAdvanced && lastReport) {
      renderReport(lastReport);
      if(!byId("report-dialog").open) byId("report-dialog").showModal();
      return;
    }
    if (!vm || !vm.resolved) {showToast("Сначала примите решение по текущему обращению.");return;}
    if(working) return;
    working=true;render(vm);
    try {
      var result=await gateway.execute({type:"AdvanceDay",requestId:"mock-end-day-01"});
      if(!result.ok){showToast(result.error||"Невозможно завершить день.");return;}
      dayAdvanced=true;lastReport=result.report;
      renderReport(result.report);
      byId("report-dialog").showModal();
    } catch(error){
      showToast("Ошибка завершения дня: "+(error && error.message ? error.message : "неизвестно"));
    } finally {working=false;render(vm);}
  }
  async function showDayTwo() {
    if(working) return;
    working=true;
    try {
      var result=await gateway.execute({type:"ShowDayTwo"});
      if(!result.ok){showToast(result.error||"Переход недоступен.");return;}
      byId("report-dialog").close();
      render(result.view);
      showToast("День 2 — только экран-заглушка. Перезагрузите страницу для нового выбора.");
    } catch(error){showToast("Переход не выполнен.");}
    finally{working=false;if(vm)render(vm);}
  }
  [["scene-document",openDocument],["open-document",openDocument],["quick-document",openDocument],["document-close",function(){byId("document-dialog").close();}],["inspect-button",function(){send("InspectDocument");}],["clarify-button",function(){send("RequestClarification");}],["approve-button",function(){send("ApproveProposal");}],["reject-button",function(){send("RejectProposal");}],["defer-button",function(){send("DeferProposal");}],["end-day",endDay],["next-day-button",showDayTwo],["help-button",function(){showToast("Кликните документ на столе или «Изучить заявку». В документе есть вкладки «Проверка» и «История». ESC закрывает окно.");}]].forEach(function(binding){
    byId(binding[0]).addEventListener("click",binding[1]);
  });
  document.querySelectorAll("[data-tab]").forEach(function(button){
    button.addEventListener("click",function(){selectTab(button.getAttribute("data-tab"));});
  });
  window.addEventListener("keydown",function(event){
    if(event.key==="Escape" && !byId("document-dialog").open && !byId("report-dialog").open){
      byId("toast").hidden=true;
    }
  });
  gateway.queryDashboard().then(function(initial){render(initial);}).catch(function(){
    showToast("Не удалось загрузить данные интерфейса.");
  });
})();
