# UI Design Constraints（最重要）

## 1. Base UI

This project MUST follow the existing UI layout.

The reference image is:
👉 system dashboard screenshot (provided)

---

## 2. Layout Structure（固定）

### Top Row
- 左：本日のサマリー（円グラフ）
- 中：ロス分析（テキスト）
- 右：ボトルネック（ランキング）

---

### Middle
- タイムライン表示（Sheetごとの稼働）

---

### Bottom Row（重要）
- 左：時間帯別稼働率
- 中：週間稼働率（グラフ）
- 右：シート詳細

---

## 3. Absolute Rules

❌ 新しいパネルを勝手に追加しない  
❌ レイアウト構造を変更しない  
❌ UI階層を変えない  

⭕ 既存パネルの中身だけ改善する  

---

## 4. Weekly Summary Rule

BottomPanel中央：

👉 週間稼働率グラフを表示する

- データはCoreから渡す
- GUIで計算しない

---

## 5. Design Philosophy

- ダークテーマ維持
- 情報密度を優先
- “現場で一目で分かる”UI

---

If UI change is required:
→ MUST explain before modifying

## 🤖 AI UI RULES（Codex用）

### Layout（絶対固定）

DO NOT:

* add new panels
* change layout structure
* move existing components

---

### ✔ ALLOWED

* add controls inside HeaderView
* update content only

---

### HeaderView Rule（重要）

HeaderView is the ONLY place for:

* date selection
* filters

---

### Data Handling

DO NOT:

* compute data in UI
* transform data in UI

ONLY:

* display data
