/* Day 01 read-model fixture derived from Content PR #11 (content-draft-0.2).
 * Separate from fixtures.js legacy one-request mock. DISPLAY ONLY: never posts
 * ledger entries, mutates GameState, or credits conditional partner revenue.
 * All people, room and finances are fictional gameplay data.
 */
(function (root) {
  "use strict";
  const kopecks = (value) => {
    if (typeof value !== "string" || !/^(0|[1-9][0-9]*)$/.test(value)) throw new TypeError("Canonical nonnegative kopeck string required");
    const amount = BigInt(value);
    if (amount > 9223372036854775807n) throw new RangeError("Int64 overflow");
    return amount;
  };
  const formatRub = (value) => {
    const amount = kopecks(value);
    const rub = (amount / 100n).toString().replace(/\B(?=(\d{3})+(?!\d))/g, " ");
    const fraction = (amount % 100n).toString().padStart(2, "0");
    return fraction === "00" ? rub + " ₽" : rub + "," + fraction + " ₽";
  };
  const freeze = (obj) => {
    if (obj && typeof obj === "object" && !Object.isFrozen(obj)) {
      Object.values(obj).forEach(freeze);
      Object.freeze(obj);
    }
    return obj;
  };
  const model = freeze({
    schema: "ui-day01-read-model-v1",
    source: "Content/CONTENT_SPEC_DRAFT.json content-draft-0.2 (PR #11)",
    status: "DRAFT_DISPLAY_ONLY",
    fictional: true,
    dayIndex: 0,
    dayNumber: 1,
    openingBalanceKopecks: "200000000",
    metrics: { educationQuality: 52, studentSatisfaction: 48, staffMorale: 50, employerTrust: 45, publicReputation: 47 },
    documents: [
      {
        id: "DOC-01", visitId: "D01_LAB", npcId: "npc_irina_korneeva",
        visitor: "Ирина Корнеева", role: "Координатор цифровых дисциплин",
        type: "procurement_request", title: "Оснащение компьютерного класса",
        amountKopecks: "22000000", room: "Л-204", slotDayIndex: 3, slot: "14:00-17:00",
        inspectionId: "check_seat_map", revealed: { realSeats: 20, phasedPriceKopecks: "16000000" }
      },
      {
        id: "DOC-02", visitId: "D01_STUDENT", npcId: "npc_dasha_polynskaya",
        visitor: "Даша Полынская", role: "Студенческий организатор",
        type: "student_project_proposal", title: "Студенческий проект",
        amountKopecks: "6500000", room: "Л-204", slotDayIndex: 3, slot: "14:00-17:00",
        roomStatus: "request-not-booking",
        inspectionId: "check_room_calendar", revealed: { roomReserved: false, mobilePriceKopecks: "3500000" }
      },
      {
        id: "DOC-03", visitId: "D01_PARTNER", npcId: "npc_mark_sviridov",
        visitor: "Марк Свиридов", role: "Представитель вымышленной фирмы «Пульс Инжиниринг»",
        type: "partner_offer", title: "Партнёрское предложение",
        conditionalRevenueKopecks: "12000000", recognizedRevenueKopecks: "0",
        revenueEarliestDayIndex: 4, room: "Л-204", slotDayIndex: 3, slot: "14:00-17:00",
        inspectionId: "check_contract_appendix",
        revealed: {
          onsiteMentorCostKopecks: "4000000", remoteMentorCostKopecks: "2500000",
          remoteConditionalRevenueKopecks: "9000000"
        }
      }
    ]
  });
  const api = freeze({
    model,
    formatRub,
    getDocument(id) { return model.documents.find((doc) => doc.id === id) || null; },
    getSummary() {
      return freeze({
        dayNumber: model.dayNumber,
        openingBalance: formatRub(model.openingBalanceKopecks),
        documentCount: model.documents.length,
        pendingDocuments: model.documents.map((doc) => doc.id)
      });
    }
  });
  if (typeof module === "object" && module.exports) module.exports = api;
  else root.TAU_DAY01_READ_MODEL = api;
})(typeof window !== "undefined" ? window : globalThis);
