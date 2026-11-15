/* Read-only standalone Day 01 preview. No gateway, mutations, ledger, or room reservations. */
(function (root) {
  "use strict";
  const data = root.TAU_DAY01_READ_MODEL;
  const doc = root.document;
  if (!doc) return;

  function showLoadError() {
    const summary = doc.getElementById("day01-summary");
    const conflicts = doc.getElementById("day01-conflicts");
    const documents = doc.getElementById("day01-documents");
    if (summary) {
      summary.textContent = "Не удалось загрузить данные первого дня. Проверьте файлы предпросмотра.";
      summary.setAttribute("role", "alert");
    }
    if (conflicts) conflicts.replaceChildren();
    if (documents) documents.replaceChildren();
  }

  // Never leave a misleading loading state when the fixture is unavailable.
  if (!data || typeof data.getSummary !== "function" ||
      typeof data.getRoomConflictNotice !== "function" ||
      typeof data.formatRub !== "function" ||
      !data.model || !Array.isArray(data.model.documents)) {
    showLoadError();
    return;
  }

  function add(parent, tag, content, className) {
    const node = doc.createElement(tag);
    if (className) node.className = className;
    node.textContent = String(content);
    parent.appendChild(node);
    return node;
  }
  function render() {
    const summary = doc.getElementById("day01-summary");
    const conflicts = doc.getElementById("day01-conflicts");
    const documents = doc.getElementById("day01-documents");
    if (!summary || !conflicts || !documents) return;
    const view = data.getSummary();
    summary.textContent = "День " + view.dayNumber + " · Стартовый бюджет: " +
      view.openingBalance + " · Документов: " + view.documentCount;
    conflicts.replaceChildren();
    documents.replaceChildren();
    for (const notice of data.getRoomConflictNotice()) {
      const panel = add(conflicts, "div", "", "notice");
      panel.setAttribute("role", "status");
      add(panel, "h2", "Конфликт заявок: " + notice.room);
      add(panel, "p", "День " + (notice.slotDayIndex + 1) + ", " +
        notice.slot + " · " + notice.documentIds.join(", "));
      add(panel, "p", notice.message);
    }
    for (const item of data.model.documents) {
      const card = add(documents, "article", "", "document");
      add(card, "h3", item.id + " · " + item.title);
      add(card, "p", item.visitor + " — " + item.role);
      add(card, "p", "Запрошено: " + item.room + ", день " +
        (item.slotDayIndex + 1) + ", " + item.slot + ". Не подтверждено.");
      if (item.amountKopecks !== undefined) {
        add(card, "p", "Запрошенные средства: " + data.formatRub(item.amountKopecks));
      }
      if (item.conditionalRevenueKopecks !== undefined) {
        add(card, "p", "Условный будущий доход: " +
          data.formatRub(item.conditionalRevenueKopecks) +
          " (не начислен при подписании)");
      }
    }
  }
  try {
    render();
  } catch (_error) {
    showLoadError();
  }
})(typeof window !== "undefined" ? window : globalThis);
