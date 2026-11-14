# UI/UX specification · probably-TAU · Issue #5

**Status:** proposal / prototype, awaiting design approval by chat 00. **Created:** 2026-11-14.  
**Scope:** first playable day; 2D Windows-first UI; Unity 6 presentation layer **after** Core gate C0.  
**Reference:** GDD §Г9, docs/CONCEPT.md, docs/ARCHITECTURE.md, docs/MECHANICS.md, AGENTS.md and Issue #5.

## 1. Visual thesis — «Кабинет решений»

The interface is a tangible management desk, not a grid of dashboards. The player sees a stylized office of a *fictional* president of an academic institution. Every important action is tied to a comprehensible object: visitor, paper, clock or report.

- **Direction proposed, not approved:** illustrated 2D with subtly grainy / pixel-textured details; readable vector-like UI surfaces, restrained character motion, layered parallax in the final sprites.
- **Tone proposed, not approved:** intelligent management drama with gentle student humor. No ridicule of real faculty, students, persons or the institution.
- **Originality:** do not use or imitate copyrighted art / fonts / layouts / character silhouettes from Probably Stolen. The sample emblem is fictional and not an academy logo.
- **Composition:** dark natural office framing contrasting with warm document paper. Players should glance at the desk first and learn the rest from side rails.

### Color & typography tokens (temporary)

| Token | Hex | Use |
|---|---|---|
| Forest / 950 | #0B1917 | backdrop |
| Forest / 800 | #193A31 | panels |
| Paper | #EDE2C9 | application sheets |
| Paper light | #F7EFDF | daily report |
| Ink | #263E36 | text on paper |
| Brass | #C6A36C | attention, select, divider |
| Positive | #85BA9B | upward / available (with text) |
| Caution | #B76D60 | costs / warnings (with text) |

Web mock uses browser system sans + Georgia serif; not a final Unity font license commitment. Maintain contrast >= 4.5:1 for normal text where practicable; confirm through automated contrast checks before shipping.

## 2. Screen hierarchy & wireframes

**Screen A — cabinet / home at 16:9**

~~~text
┌────────────────────────────────────────────────────────────────────────────┐
│ [TAU: PRESIDENT]  1 SEP / DAY 01    RESERVE 1 500 000 ₽   STAFF 61 [HELP] │
├────────────────┬────────────────────────────────────────┬──────────────────┤
│ CALENDAR       │ OFFICE  ·  INTERACTIVE 2D ROOM         │ RECEPTION        │
│                │                                        │                  │
│ 01             │   [WINDOW]    [EMBLEM]    [BOOKS]       │ Visitor 01       │
│ 09:20          │              [VISITOR]                 │ Visitor 02 next  │
│                │                                        │                  │
│ Agenda:        │            [DESK: DOCUMENT]            │ Deadline         │
│ - Application  │                                        │ Interests        │
│ - Verify       ├────────────────────────────────────────┤ Students         │
│ - Report       │ [PORTRAIT]  [DIALOGUE LINE]   [OPEN]    │ Staff            │
│                ├────────────────────────────────────────┤ Partners         │
│ Secretary note │ reason / explain       [END DAY]       │ Society          │
└────────────────┴────────────────────────────────────────┴──────────────────┘
~~~

**Screen B — document / evaluation overlay**

~~~text
┌───────────────────────────────────────────────────────────────────────┐
│ CASE TAU-001 · COMPUTER CLASS                                        X │
├───────────────────────────────────────────────────────────────────────┤
│ [CONTENTS] [INSPECTION] [HISTORY]                                      │
│ ┌───────────────────────────────────────────────┐  YOUR DECISION       │
│ │             PHYSICAL PAPER                    │  [APPROVE]           │
│ │ Title · Author · Amount · Deadline            │  [REJECT]            │
│ │ Detailed request and missing evidence         │  [DEFER]             │
│ │                                               │                      │
│ │ Inspector observations appear as text         │  Disabled reasons    │
│ │ Written history / verification step           │  and warnings        │
│ └───────────────────────────────────────────────┘                      │
└───────────────────────────────────────────────────────────────────────┘
~~~

**Screen C — end-of-day report**

~~~text
┌───────────────────────────────────────────────────────────────────────┐
│                        END OF DAY / DAY 01                            │
│                        Decision summary                               │
│                                                                      │
│     START RESERVE                   FINAL RESERVE                     │
│     1 500 000 ₽                    1 255 000 ₽                       │
│                                                                      │
│     LEDGER from returned report (not calculated by UI)              │
│     Approved amount                         −210 000 ₽              │
│     Running expenses                         −35 000 ₽              │
│                                                                      │
│     Reason / delayed consequence preview                              │
│                                                [DAY TWO PREVIEW]     │
└───────────────────────────────────────────────────────────────────────┘
~~~

At 1366x768 preserve the office, budget and primary CTA. At 1110px the reception panel moves underneath the office, without hiding actions. At 760px a compact stacked layout is used for the **prototype only**; mobile is not a target platform.

## 3. First-day interaction contract

1. Read-only dashboard projection shows day, budget, visitor, queues and actionable request. Start state has no submission in progress.
2. Open the physical file using desk hotspot or action button. The same command is keyboard reachable.
3. Contents tab gives title, author, proposed cost, deadline and missing evidence. Inspection and history stay text-accessible.
4. Optionally inspect the estimate; the server/Core may reveal a fact, consume time, unlock new actions, or return a validation error. UI only renders the response.
5. Optionally ask for a revised quote. Mock snapshot changes the proposed amount from 240 000 ₽ to 210 000 ₽; no UI arithmetic or negotiation algorithm exists.
6. Issue one of three choices: approve / reject / defer. Disable submission while pending, present the reason for rejection where available, refresh all projections after success.
7. Allow End Day only after the current case is resolved. Invoke one authorized application command and display its authoritative report (ledger lines and explicit reasons).
8. Day two in this preview is intentionally a static placeholder; generated visitors, scheduling and Save/Load are outside UI scope.

### Example mock outcomes — **not game balance decisions**

| Scenario | Reserve after decision | Reserve in report | Staff trust |
|---|---:|---:|---:|
| Approve without clarification | 1 260 000 ₽ | 1 225 000 ₽ | 63 |
| Clarify then approve | 1 290 000 ₽ | 1 255 000 ₽ | 63 |
| Reject | 1 500 000 ₽ | 1 465 000 ₽ | 58 |
| Defer | 1 500 000 ₽ | 1 465 000 ₽ | 60 |

All values are prewritten fixture strings. They are neither real institutional data nor recommended simulation formulas.

## 4. Projection and command ports — **proposal for C0 review**

No class in this document is claimed to exist. Chat 01 / coordinator 00 own exact names and signatures. **UI must depend on approved Application interfaces, not on concrete GameState.**

~~~text
UI (Unity UI Toolkit, visual UXML/USS + Presenter)
  -> IOfficeReadPort.GetDashboard() : OfficeDashboardView
  -> IOfficeReadPort.GetDocument(documentId) : DocumentDetailView
  -> IOfficeCommandPort.Execute(command, requestId) : CommandResponse
  <- command result / rejection reason / domain event or changed read model
  -> IOfficeReadPort.GetDayReport(dayId) : DayReportView
~~~

Suggested **view fields**: day label & phase; currency-formatted display strings paired with explicit currency/units in the approved read model; reputation group label/value/explanation; visitor ID, display name and role; document ID, proposal fields, available actions, denial reasons, observations, deadlines; report ledger rows/changes/causes.

Suggested **command semantics** (NOT final API names): inspect document; request clarification; approve; reject; defer; advance day. Stable IDs and deduplication are Application/Core concerns. Submit state and focus state belong to UI.

**Event presentation only:** on success requery the authoritative read models; never compute new balances, debts, deadlines, trust or faction values from events in the presenter.

## 5. States, interaction and accessibility

- **Loading:** skeleton or passive “loading…”; block action buttons until read model arrives.
- **Pending command:** disable other submits and show nonblocking progress; avoid optimistic budget updates.
- **Rejected command:** clear inline error, explicit reason (insufficient funds, expired deadline, already resolved, wrong phase), no optimistic progress.
- **Confirmed decision:** update budget and consequences as read; offer easy drilldown to the explanation and report.
- **Modal return:** Escape closes document; the active entity and read model remain intact; do not start or undo a command on modal close.
- **Keyboard:** Tab navigates document hotspot, document sections and decisions; Enter/Space activates focused buttons; Escape exits dialog; after closing a dialog, browser focus returns to trigger where supported.
- **Accessibility:** all important colors have text labels, live region for noncritical alerts, descriptive action names, >=44px pointer target for primary controls, 100–150% UI scale without clipping (to verify in Unity), reduced-motion preference, no information exclusively in animation.
- **Tooltip semantics:** disabled action should have a visible reason, not solely a native tooltip. In the mock, the end-day hint stays visible.
- **Fiction disclaimer:** permanently visible in the preview, with a detailed disclaimer in documents/reports.

## 6. Ownership and implementation stages

**Current branch scope:** standalone /prototypes/ui-day-one/ browser concept, UI spec, handoff, portable mock smoke tests. No Unity folders or external packages added because Unity project does not yet exist.

**After coordinator approves visual direction and chat 01 publishes C0:**
- Produce original source assets (separate layers office / desk / character), unify scale and legal asset licensing.
- Translate DOM structure to Unity UI Toolkit UXML/USS and author PresidentOffice scene with SpriteRenderer layers, avoiding accidental business logic.
- Implement Unity Presentation adapter that maps exact approved read models/commands, preserving unique IDs and actual game phases.
- Integrate keyboard/game input, layouts, DPI and build/PlayMode testing, error state checks, Alt-Tab and loading/saving of view state.
- Request 05 QA and 00 review, then merge only with owner approval.

## 7. Open decisions for coordinator 00

- O-002: accept illustrated 2D / painted grain or choose strict pixel art.
- O-003: tone blend (management drama + restrained student humor).
- O-005: final cabinet arrangement, representation of academy identity, approval for institutional marks/photographs.
- Confirm action availability/inspection cost/EndDay eligibility and formatted currency via C0 interfaces.
- Confirm whether first scenario and mock costs should become actual balance/content (Content 04 owns game balance), otherwise replace them entirely.
