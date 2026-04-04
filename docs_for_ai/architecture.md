# LaserMonitor Architecture

## 1. Purpose

LaserMonitor is a WPF desktop application that:

- parses laser machine logs
- reconstructs automatic operation runs from scheduling logs
- analyzes daily operation by machine role
- stores results in SQLite
- visualizes KPIs and timelines

---

## 2. Core Analysis Concept（最重要）

This system must analyze one day from the viewpoint of:

- automatic operation time of the day
- laser machine working time inside that automatic operation
- sorter working time inside that automatic operation
- system transport / warehouse / pallet work inside that automatic operation

The analysis target is **not simply individual events**.
The analysis target is:

> "How much of the day's automatic operation time was used by each machine role?"

---

## 3. Daily Time Definition（最重要）

### 3.1 Day range

A daily report always targets:

- 00:00:00 to 23:59:59 of the selected date

---

### 3.2 Automatic operation time

Automatic operation time is defined by:

- `Start Scheduling`
- `Scheduling stopped by operator`
- `Scheduling interrupted`

One automatic run is one interval from Schedule START to Schedule STOP/INTERRUPT.
The sum of all such intervals inside the day is the day's **operation time**.

---

### 3.3 Cross-midnight rule（重要）

Runs may cross midnight.

Examples:

- a run starts before 00:00 and continues into the selected day
- a run starts in the selected day and ends after 24:00

Rule:

- first reconstruct the full scheduling interval
- then clip it to the selected day's range
- only the overlapped portion belongs to that day

Therefore:

- a run that started on the previous day can contribute operation time to the selected day
- a run that ends on the next day can also contribute operation time to the selected day

This rule is mandatory for all analyzers.

---

## 4. Machine-based Analysis Model（重要）

The project must analyze three different machine roles separately.

### 4.1 Laser machine

Inside automatic operation time, the laser machine is evaluated by:

- CUT = `Cutting started` → `Cutting completed`
- IDLE = automatic operation time minus CUT time
- optional subclasses may later split IDLE into WAIT / INTERRUPT / PALLET CHANGE related states

---

### 4.2 Sorter robot

Inside automatic operation time, the sorter is evaluated by:

- SORT = `Sorting started` → `Sorting completed`
- IDLE = automatic operation time minus SORT time

---

### 4.3 System

Inside automatic operation time, the system is evaluated by transport / warehouse / pallet actions such as:

- Load sheet
- Unload sheet
- Unload/Load
- PlaceProduct
- Pallet change
- Third pallet change
- drawer movement / material stocker movement

System working time is the union of these transport-related intervals.
System idle time is automatic operation time minus that union.

Important:

- system work may overlap with laser CUT
- system work may overlap with sorter work
- therefore machine-role totals are analyzed independently
- these role-based times must NOT be summed together as if they were exclusive states

---

## 5. Data Flow（最重要）

```text
Log file
 ↓
LogParser
 ↓
LogEvent[]
 ↓
OperationAnalyzer
 ↓
ScheduleInterval[]
 ↓
ScheduleSplitter
 ↓
DailyScheduleSegment[]
 ↓
 ├─ MachineAnalyzer
 ├─ SorterAnalyzer
 ├─ SystemAnalyzer
 ├─ LossAnalyzer
 ├─ ErrorAnalyzer
 ├─ TimeEfficiencyAnalyzer
 └─ BottleneckAnalyzer
 ↓
Builders
 ↓
DashboardService
 ↓
SQLiteService
 ↓
GUI
```

---

## 6. Architectural Intent

### 6.1 OperationAnalyzer

OperationAnalyzer is responsible for reconstructing scheduling runs.
It defines the denominator of daily operation.

### 6.2 ScheduleSplitter

ScheduleSplitter is responsible for clipping / splitting reconstructed runs into the selected day.
It is the owner of the cross-midnight rule.

### 6.3 Role analyzers

Role analyzers evaluate the selected day from different viewpoints.

- MachineAnalyzer = laser machine
- SorterAnalyzer = sorter robot
- SystemAnalyzer = transport / warehouse / pallet system

### 6.4 Cause analyzers

Cause analyzers explain non-working time or interruptions.

- LossAnalyzer
- ErrorAnalyzer
- BottleneckAnalyzer
- TimeEfficiencyAnalyzer

---

## 7. Key Rules（AI用）

### Allowed architecture

Log → Parser → Analyzer → Builder → Service → SQLite → GUI

### Forbidden

DO NOT:

- calculate daily operation time in GUI
- calculate cross-midnight clipping in GUI
- access SQLite directly from GUI
- merge role-based analyzer logic into Builder
- treat CUT / SORT / SYSTEM as one exclusive state machine unless explicitly designed
- break scheduling intervals directly in UI code

### Required

MUST:

- reconstruct schedule runs first
- split by date second
- analyze each machine role independently inside the same daily operation time
- keep overlap semantics explicit

---

## 8. Why this architecture changed

Previous analyzer design leaned too much toward generic interval analysis.
The validated log reading showed that the real production question is:

- what was the day's automatic operation time?
- how much of that time did the laser machine actually CUT?
- how much did the sorter actually SORT?
- how much did the system actually perform transport work?

Therefore the architecture must prioritize:

- schedule-based daily denominator
- machine-role specific numerators
- explicit cross-midnight handling

---

## 9. Related Documents

- AGENTS.md
- module_responsibilities.md
- analyzer_responsibility_map.md
- project_tree.md
- sqlite_design.md
- ui_design_constraints.md
