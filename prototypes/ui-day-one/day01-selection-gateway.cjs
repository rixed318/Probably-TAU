/* Presentation-only selection gateway. Canonical fixture stays immutable; no commands or finance. */
(function (root) {
  "use strict";
  function createSelectionGateway(readModel) {
    if (!readModel || !readModel.model || !Array.isArray(readModel.model.documents)) throw new TypeError("Day01 read model required");
    const ids = readModel.model.documents.map(item => item.id);
    if (!ids.length || new Set(ids).size !== ids.length) throw new TypeError("Unique document IDs required");
    let selectedId = ids[0];
    return Object.freeze({
      list() { return ids.slice(); },
      selected() { return readModel.getDocument(selectedId); },
      select(id) {
        if (!ids.includes(id)) return false;
        selectedId = id;
        return true;
      }
    });
  }
  if (typeof module === "object" && module.exports) module.exports = { createSelectionGateway };
  else root.TAU_DAY01_SELECTION = { createSelectionGateway };
})(typeof window !== "undefined" ? window : globalThis);
