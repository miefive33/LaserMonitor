# 🤖 AGENTS.md（完全版 / LaserMonitor）

## AI Development Rules for LaserMonitor

---

# 🎯 Purpose

This file defines strict rules for AI (Codex) when modifying this project.

AI must follow these rules at all times.

---

# 📚 Source of Truth (Priority Order)

1. AGENTS.md (this file)
2. architecture.md
3. module_responsibilities.md
4. analyzer_responsibility_map.md
5. ui_design_constraints.md
6. Existing code

If conflicts occur:
→ Follow this order strictly

---

# 🧱 Architecture Rules（最重要）

## Allowed Flow（STRICT）

Log
→ Parser
→ Analyzer
→ Builder
→ Service
→ SQLite
→ GUI

---

## ❌ Forbidden Actions（絶対禁止）

AI MUST NOT:

* modify architecture flow
* call Analyzer from GUI
* access SQLite directly from GUI
* perform calculations in GUI
* introduce new architecture layers
* refactor unrelated code
* move cross-midnight logic into GUI or Builder
* mix machine-role results into one fake exclusive state model without design approval

---

## ✔ Allowed Actions

AI CAN:

* call existing methods
* extend ViewModel safely
* add event handling
* update UI content within constraints
* add or refine models needed for schedule-based analysis

---

# 🖥️ GUI Rules

## Responsibilities

GUI is ONLY for:

* displaying data
* triggering events
* selecting date / machine / filters

---

## ❌ GUI MUST NOT

* calculate KPI
* transform raw log data into analysis result
* access database directly
* contain business logic
* clip cross-midnight intervals
* classify CUT / SORT / SYSTEM time

---

## ✔ GUI CAN

* call LoadLog(DateTime)
* bind ViewModel
* update UI components

---

# 📊 Analyzer Rules

## Responsibilities

Analyzer is responsible for:

* schedule reconstruction
* cross-midnight aware daily segmentation
* KPI calculation
* time analysis
* machine-role analysis
* interruption / error analysis

---

## ❌ Analyzer MUST NOT

* access UI
* format UI output
* access database

---

# 🔥 Validated Domain Rule（最重要）

The validated business rule is:

> Schedule START to Schedule STOP/INTERRUPT is one automatic operation run.
> The sum of such runs inside the selected day is the day's operation time.

Inside that same daily operation time:

* Laser machine uses CUT time
* Sorter uses SORT time
* System uses LOAD / UNLOAD / transport / pallet work time

These role-based times:

* share the same daily denominator
* may overlap with each other
* must be analyzed independently

AI MUST preserve this rule.

---

# 🌙 Cross-Midnight Rule（絶対）

Runs may cross midnight.

AI MUST:

1. reconstruct the original schedule interval first
2. clip that interval to the selected day second
3. analyze machine roles only inside the clipped daily segments

AI MUST NOT:

* assign the whole run only to the start date
* assign the whole run only to the end date
* ignore overlap with the selected day

---

# 🧩 Builder Rules

## Responsibilities

Builder is responsible for:

* formatting data for UI
* converting analyzer results into cards, charts, tables, and text

---

## ❌ Builder MUST NOT

* calculate KPI
* access database
* implement business logic
* reconstruct schedule intervals
* decide cross-midnight ownership

---

# 🧠 Service Rules（重要）

## Responsibilities

Service is responsible for:

* orchestrating analyzers
* controlling data flow
* providing data to ViewModel

---

## ❌ Service MUST NOT

* implement analysis logic
* format UI data too deeply
* directly manipulate UI

---

## 🔥 DashboardService

Acts as:

👉 Orchestrator between Analyzer and GUI

Responsibilities:

* call parser
* call OperationAnalyzer
* call ScheduleSplitter
* call MachineAnalyzer / SorterAnalyzer / SystemAnalyzer
* call LossAnalyzer / ErrorAnalyzer / BottleneckAnalyzer / TimeEfficiencyAnalyzer
* aggregate results
* pass data to ViewModel

---

# 💾 Database Rules

## SqliteService Responsibilities

* SELECT
* INSERT
* DELETE（rebuild only）

---

## ❌ MUST NOT

* perform analysis
* contain business logic
* interact with UI

---

# 🎨 UI Layout Constraints（絶対固定）

AI MUST follow:

* DO NOT change layout structure
* DO NOT add new panels
* DO NOT move existing components

---

## ✔ Allowed

* add controls inside HeaderView
* update content inside existing panels
* change binding targets as needed to show corrected analyzer results

---

# ⚙️ Implementation Rules

When implementing features:

1. Prefer existing methods
2. Keep changes minimal
3. Do NOT modify unrelated files
4. Do NOT rename existing classes or methods unless the design truly requires it
5. When changing analyzer behavior, update docs and code consistently

---

# 📦 Output Rules

AI MUST:

* Provide FULL modified code
* Clearly indicate modified files
* Explain where to place code
* Reference AGENTS.md when proposing changes

---

# ⚠️ Safety Rules

If requirements are unclear:

→ ASK instead of guessing

However, if the design rule is already defined in these docs:

→ FOLLOW the docs, do not invent a new interpretation

---

# 🎯 Design Philosophy

* Simplicity over complexity
* Stability over optimization
* Consistency over flexibility
* Daily denominator must be trustworthy

---

# 🧠 System Design Concept（重要）

This system is based on:

👉 「日次自動運転時間 × 設備役割 × 原因 × 優先度」

---

## Analysis Layers

### ① Denominator

* OperationAnalyzer
* ScheduleSplitter

👉 day-level operation time

---

### ② Role-based activity

* MachineAnalyzer
* SorterAnalyzer
* SystemAnalyzer

👉 who used that time

---

### ③ Cause analysis

* LossAnalyzer
* ErrorAnalyzer
* TimeEfficiencyAnalyzer

👉 why time was not fully productive

---

### ④ Improvement priority

* BottleneckAnalyzer

👉 what to fix first

---

These layers must remain independent.

---

# 🚀 Summary

AI should behave like:

"A careful engineer who respects architecture, preserves the validated daily analysis rule, and avoids inventing hidden assumptions."
