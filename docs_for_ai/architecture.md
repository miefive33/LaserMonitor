# LaserMonitor Architecture

## 1. Purpose

LaserMonitor is a WPF desktop application that:

- parses laser machine logs
- reconstructs automatic operation blocks
- analyzes machine / sorter / system activity
- analyzes loss / error / bottleneck structure
- stores data in SQLite
- visualizes KPIs and timelines

---

## 2. Project Structure

Refer to:

- project_tree.md
- module_responsibilities.md

---

## 3. Core Architecture

Laser.Core is divided into:

- Parsers
- Models
- Analyzers
- Builders
- Services
- SQLite access

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
ScheduleSplitter
 ↓
Day-bounded intervals / events
 ↓
MachineAnalyzer / SorterAnalyzer / SystemAnalyzer
 ↓
LossAnalyzer / ErrorAnalyzer / TimeEfficiencyAnalyzer / BottleneckAnalyzer
 ↓
Builder
 ↓
DashboardService / SqliteService
 ↓
GUI
```

---

## 5. Daily Time Model（重要）

This system must use a shared daily denominator.

### Automatic operation block
A single automatic operation block is:

- `Start Scheduling`
  to
- `Scheduling stopped by operator`
  or
- `Scheduling interrupted`

### Daily operating time
Daily operating time is:

- the total overlap between automatic operation blocks and the target day

This rule applies to all analyzers.

---

## 6. Day-Crossing Rule（重要）

If one automatic operation block crosses midnight,
the time must be split by date.

### Responsibility
- OperationAnalyzer restores the original operation block
- ScheduleSplitter owns day-overlap handling

No other layer may redefine date ownership independently.

---

## 7. Three Analysis Perspectives（重要）

The system analyzes one day from three independent perspectives.

### 7.1 Laser machine
- active time = CUT time
- idle / non-cut time = remaining time inside the same daily operating denominator

### 7.2 Sorter
- active time = SORT time
- idle / non-sort time = remaining time inside the same daily operating denominator

### 7.3 System
- active time = load / unload / unload-load / place product / pallet change / drawer & warehouse movement related activity
- system activity may overlap with CUT or SORT

### Important
These three perspectives do NOT need to sum to 100%.
This is intentional.

---

## 8. Analyzer Design

Detailed responsibilities are defined in:

- analyzer_responsibility_map.md
- module_responsibilities.md

### Summary

#### OperationAnalyzer
Reconstruct automatic operation blocks from logs.

#### ScheduleSplitter
Split intervals by day and fix the daily denominator.

#### MachineAnalyzer
Analyze CUT and non-CUT from machine perspective.

#### SorterAnalyzer
Analyze SORT and non-SORT from sorter perspective.

#### SystemAnalyzer
Analyze transfer / loading / unloading / pallet / warehouse related activity.
System activity may overlap with CUT / SORT.

#### LossAnalyzer
Analyze non-value-added time during automatic operation.
Loss is treated as a phenomenon.

#### ErrorAnalyzer
Analyze explicit abnormal events and their interruption impact.
Error is treated as an abnormal cause.

#### BottleneckAnalyzer
Return improvement priority based on duration, recurrence, and interruption severity.

---

## 9. Loss / Error / Bottleneck Boundary（重要）

The boundary between these analyzers must remain clear.

### LossAnalyzer
Handles:
- waiting
- delay
- non-value-added structure
- machine / sorter / system loss view

### ErrorAnalyzer
Handles:
- explicit failure
- error code
- interruption
- recovery impact

### BottleneckAnalyzer
Handles:
- what should be fixed first
- ranking based on total impact and recurrence

This separation is mandatory.

---

## 10. Builder Role

Builders must:

- format analyzer outputs for UI
- prepare graph-friendly and card-friendly structures

Builders must NOT:

- recalculate KPIs
- reinterpret loss/error
- implement business logic

---

## 11. Service Role

### DashboardService
Acts as the orchestrator between Core and GUI.

Responsibilities:
- call analyzers in correct order
- provide integrated analysis results
- feed ViewModel safely

### SqliteService
Handles:
- SELECT
- INSERT
- DELETE for rebuild only

SqliteService must NOT:
- implement analysis logic
- format UI data

---

## 12. GUI Architecture

Laser.GUI is presentation only.

GUI may:
- trigger LoadLog(DateTime)
- bind ViewModel
- display results

GUI must NOT:
- calculate KPI
- decide date ownership
- perform analyzer logic
- access SQLite directly

---

## 13. Key Rules（AI用）

Do NOT:
- break architecture flow
- move analysis into GUI
- invent new architecture layers
- make system/cut/sort mutually exclusive without log evidence
- redefine the denominator in each analyzer

Do:
- preserve shared daily operating time
- preserve day-crossing split logic
- preserve analyzer responsibility boundaries
- keep changes minimal

---

## 14. Design Philosophy

This architecture is based on:

- shared denominator
- strict day-boundary handling
- independent machine / sorter / system views
- clear separation of phenomenon / abnormality / priority

This is necessary to produce analysis that is actually useful for shop-floor improvement.
