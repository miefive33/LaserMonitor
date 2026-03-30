# 🤖 AGENTS.md

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

# 🧱 Architecture Rules

## Allowed Flow (STRICT)

Log → Parser → Analyzer → Builder → SQLite → GUI

---

## ❌ Forbidden Actions

AI MUST NOT:

* modify architecture flow
* call Analyzer from GUI
* access SQLite directly from GUI
* perform calculations in GUI
* introduce new layers (Service, Manager, etc.)
* refactor unrelated code

---

## ✔ Allowed Actions

AI CAN:

* call existing methods
* add UI components within constraints
* extend ViewModel safely
* add event handling

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

Analyzer is responsible for:

* KPI calculation
* time analysis
* interval processing

---

## ❌ Analyzer MUST NOT

* access UI
* format UI output

---

# 🧩 Builder Rules

Builder is responsible for:

* formatting data for UI

---

## ❌ Builder MUST NOT

* calculate KPI
* access database

---

# 💾 Database Rules

SQLiteService is responsible for:

* SELECT
* INSERT

---

## ❌ MUST NOT

* perform analysis
* contain business logic

---

# 🎨 UI Layout Constraints

AI MUST follow:

* DO NOT change layout structure
* DO NOT add new panels
* DO NOT move existing components

---

## ✔ Allowed

* Add controls inside existing views (e.g. HeaderView)

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

# 🚀 Summary

AI should behave like:

"A careful engineer who follows strict architecture rules and avoids unnecessary changes."
