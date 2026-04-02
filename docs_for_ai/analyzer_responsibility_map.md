# Analyzer Responsibility Map

---

# 全体フロー（完全版）

LogEvent[]
 ↓
OperationAnalyzer
 ↓
OperationInterval[]
 ↓
ScheduleSplitter
 ↓
分割済Interval
 ↓
 ├─ MachineAnalyzer
 ├─ SorterAnalyzer
 ├─ SystemAnalyzer
 ├─ LossAnalyzer
 ├─ ErrorAnalyzer
 ├─ TimeEfficiencyAnalyzer
 ├─ BottleneckAnalyzer
 ↓
結果統合
 ↓
KpiBuilder

---

# 🔥 最大の進化ポイント

👉 分析を3階層に分離

---

## ① 現象（What happened）

- LossAnalyzer
- TimeEfficiencyAnalyzer

👉 状態把握

---

## ② 原因（Why happened）

- ErrorAnalyzer
- SystemAnalyzer

👉 原因特定

---

## ③ 対象（Where happened）

- MachineAnalyzer
- SorterAnalyzer
- SheetAnalyzer

👉 どの設備か

---

## ④ 優先度（What to fix）

- BottleneckAnalyzer

👉 改善順序

---

# 🎯 設計の本質

この構造は：

「設備 × 原因 × 時間 × 優先度」

---

# Analyzer間ルール

❌ 相互依存禁止  
✔ OperationInterval基準  

---

# 結論

この構造で

👉 現場改善ができる分析になる