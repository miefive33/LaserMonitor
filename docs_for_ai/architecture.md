# LaserMonitor Architecture

## 1. Purpose
LaserMonitor is a WPF desktop application that:

- parses laser machine logs
- analyzes machine operation
- stores data in SQLite
- visualizes KPIs and timelines

---

## 2. Project Structure

👉 project_tree.md を参照

---

## 3. Core Architecture

Laser.Core is divided into:

- Parsers
- Models
- Analyzers
- Builders
- Services (SQLite)

---

## 4. Data Flow（最重要）

```text
Log file
 ↓
LogParser
 ↓
LogEvent[]
 ↓
OperationAnalyzer
 ↓
Other Analyzers
 ↓
Builder
 ↓
SQLiteService
 ↓
GUI
```

## 5. Analyzer Design
👉 analyzer_responsibility_map.md を参照

## 6. Database Design
👉 sqlite_design.md を参照

## 7. GUI Architecture
Laser.GUI is presentation only.
👉 ui_design_constraints.md を参照

## 8. Key Rules（AI用）
project_tree.md にないファイルは作らない
Analyzerにロジックを集約する
DB処理はSqliteServiceのみ
GUIは表示のみ
UI構造は固定

## 9. Related Documents
project_tree.md
dependency_rules.md
module_responsibilities.md
ui_design_constraints.md
sqlite_design.md
analyzer_responsibility_map.md


🤖 AI STRICT RULES（Codex用・最重要）
Allowed Flow（絶対）

Log → Parser → Analyzer → Builder → SQLite → GUI

❌ FORBIDDEN（絶対禁止）

DO NOT:

call Analyzer from GUI
access SQLite from GUI
calculate KPI outside Analyzer
modify data flow
add new architecture layers
✔ ALLOWED
GUI can trigger LoadLog(DateTime)
GUI can display ViewModel data
Priority（判断基準）
Do NOT break architecture
Do minimal changes
Reuse existing methods