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
4. ui_design_constraints.md
5. Existing code

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

---

## ✔ Allowed Actions

AI CAN:

* call existing methods
* extend ViewModel safely
* add event handling
* update UI content within constraints

---

# 🖥️ GUI Rules

## Responsibilities

GUI is ONLY for:

* displaying data
* triggering events

---

## ❌ GUI MUST NOT

* calculate KPI
* transform data
* access database
* contain business logic

---

## ✔ GUI CAN

* call LoadLog(DateTime)
* bind ViewModel
* update UI components

---

# 📊 Analyzer Rules

## Responsibilities

Analyzer is responsible for:

* KPI calculation
* time analysis
* interval processing

---

## ❌ Analyzer MUST NOT

* access UI
* format UI output
* access database

---

# 🧩 Builder Rules

## Responsibilities

Builder is responsible for:

* formatting data for UI

---

## ❌ Builder MUST NOT

* calculate KPI
* access database
* implement business logic

---

# 🧠 Service Rules（NEW / 重要）

## Responsibilities

Service is responsible for:

* orchestrating analyzers
* controlling data flow
* providing data to ViewModel

---

## ❌ Service MUST NOT

* implement analysis logic
* format UI data
* directly manipulate UI

---

## 🔥 DashboardService（重要）

Acts as:

👉 Orchestrator between Analyzer and GUI

Responsibilities:

* call multiple analyzers
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

---

# ⚙️ Implementation Rules

When implementing features:

1. Prefer existing methods
2. Keep changes minimal
3. Do NOT modify unrelated files
4. Do NOT rename existing classes or methods

---

# 📦 Output Rules

AI MUST:

* Provide FULL modified code
* Clearly indicate changes
* Explain where to place code

---

# ⚠️ Safety Rules

If requirements are unclear:

→ ASK instead of guessing

---

# 🎯 Design Philosophy

* Simplicity over complexity
* Stability over optimization
* Consistency over flexibility

---

# 🧠 System Design Concept（重要）

This system is based on:

👉 「設備 × 原因 × 時間 × 優先度」

---

## Analysis Layers

### ① What happened（現象）

* LossAnalyzer
* TimeEfficiencyAnalyzer

---

### ② Why happened（原因）

* ErrorAnalyzer
* SystemAnalyzer

---

### ③ Where happened（対象）

* MachineAnalyzer
* SorterAnalyzer
* SheetAnalyzer

---

### ④ What to fix（優先度）

* BottleneckAnalyzer

---

👉 These layers must remain independent

---

# 🚀 Summary

AI should behave like:

"A careful engineer who respects architecture, avoids unnecessary changes, and builds only what is required."
