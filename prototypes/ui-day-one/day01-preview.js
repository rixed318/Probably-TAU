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
    if (documents) { documents.replaceChildren(); documents.setAttribute("aria-busy", "false"); }
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
    for (const [index, item] of data.model.documents.entries()) {
      const card = add(documents, "article", "", "document");
      const heading = add(card, "h3", item.id + " · " + item.title);
      heading.setAttribute("id", "day01-document-" + (index + 1) + "-title");
      card.setAttribute("aria-labelledby", "day01-document-" + (index + 1) + "-title");
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
    const queue = doc.getElementById("day01-queue");
    const selectionApi = root.TAU_DAY01_SELECTION;
    if (queue && selectionApi && typeof selectionApi.createSelectionGateway === "function") {
      const gateway = selectionApi.createSelectionGateway(data);
      const cards = documents.children;
      const buttons = [];
      const updateSelection = () => {
        const selected = gateway.selected();
        for (let i = 0; i < cards.length; i++) cards[i].hidden = data.model.documents[i].id !== selected.id;
        for (let i = 0; i < buttons.length; i++) {
          buttons[i].setAttribute("aria-pressed", String(data.model.documents[i].id === selected.id));
        }
      };
      queue.replaceChildren();
      for (const item of data.model.documents) {
        const button = add(queue, "button", item.id + " · " + item.visitor);
        button.setAttribute("type", "button");
        button.addEventListener("click", () => {
          if (gateway.select(item.id)) updateSelection();
        });
        buttons.push(button);
      }
      updateSelection();
    }
    documents.setAttribute("aria-busy", "false");
  }
  try {
    render();
  } catch (_error) {
    showLoadError();
  }
})(typeof window !== "undefined" ? window : globalThis);
