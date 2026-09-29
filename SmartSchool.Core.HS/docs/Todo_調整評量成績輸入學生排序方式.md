# Todo: 調整評量成績輸入學生排序方式

## Goal

調整「評量成績輸入」畫面開啟後的學生排序方式。

目前課程修課學生的排序方式為：

```sql
ORDER BY class_name, seat_no, student_number
```

請修改為優先依照 `class.display_order` 排序，再依照 `class.class_name` 排序。

修改完成後，請將調整內容記錄於：

```text
評量成績輸入排序調整0929.md
```

---

## Target File

主要檢查與修改：

```text
IDataProvider.cs
```

類別：

```csharp
SmartSchoolDataProvider
```

目前取得課程修課學生的 SQL 約為：

```csharp
string qryCourseStudent = "SELECT " +
    "sc_attend.id AS sc_attend_id" +
    ",student.id AS student_id" +
    ",class.class_name" +
    ",student.seat_no" +
    ",student.student_number" +
    ",student.name AS student_name " +
    ",student.status AS student_status " +
    "FROM " +
    "sc_attend INNER JOIN student " +
    "ON sc_attend.ref_student_id = student.id " +
    "LEFT JOIN class " +
    "ON student.ref_class_id = class.id " +
    "WHERE ref_course_id = " + courseid + " " +
    "ORDER BY class_name,seat_no,student_number";
```

目前排序：

```text
班級名稱
→ 座號
→ 學號
```

---

## Required Changes

### 1. 加入 class.display_order

取得課程學生資料時，加入：

```sql
class.display_order
```

例如：

```sql
SELECT
    sc_attend.id AS sc_attend_id,
    student.id AS student_id,
    class.display_order,
    class.class_name,
    student.seat_no,
    student.student_number,
    student.name AS student_name,
    student.status AS student_status
```

---

### 2. 修改班級排序規則

班級排序優先順序：

```text
1. class.display_order 有數字的班級排前面
2. display_order 數字由小到大
3. display_order 沒有數字（NULL）排最後
4. display_order 相同時，再依 class.class_name 排序
5. 同班級學生再依 student.seat_no 排序
6. 最後使用 student.student_number 作為穩定排序
```

預期排序概念：

```text
display_order
→ class_name
→ seat_no
→ student_number
```

---

## SQL Sorting

PostgreSQL 請使用明確的 NULL 排序規則，例如：

```sql
ORDER BY
    class.display_order ASC NULLS LAST,
    class.class_name ASC,
    student.seat_no ASC,
    student.student_number ASC
```

請確認 `class.display_order` 的實際資料型態。

如果 `display_order` 本身為數值型態，直接使用：

```sql
class.display_order ASC NULLS LAST
```

不要轉成字串排序，避免：

```text
1
10
11
2
3
```

這種錯誤順序。

---

## Expected Result

假設班級資料：

```text
display_order    class_name
-------------    ----------
1                一年甲班
2                一年乙班
10               一年丁班
NULL             一年丙班
NULL             一年戊班
```

成績輸入畫面的班級順序應為：

```text
一年甲班
一年乙班
一年丁班
一年丙班
一年戊班
```

其中沒有 `display_order` 的班級統一排在有設定的班級之後，再依 `class_name` 排序。

同一班級內維持：

```text
座號 ASC
→ 學號 ASC
```

---

## Important Constraints

- 僅調整「評量成績輸入」學生顯示排序。
- 不修改成績讀取邏輯。
- 不修改 `sc_attend` 資料處理。
- 不修改 `sce_take` 評量成績讀取與儲存。
- 不修改學生狀態篩選邏輯。
- 不修改「顯示所有學生」功能。
- 不修改成績欄位及評量欄位產生方式。
- 不修改 `DataGridViewItem` 的其他功能。
- 不影響成績新增、修改、刪除及儲存。
- 保留原本同班學生的 `seat_no → student_number` 排序。
- 不進行與本需求無關的重構。

---

## Verification

修改完成後至少確認以下案例：

1. 多個班級都有 `display_order`
   - 確認依數字小至大排序。

2. `display_order = 2` 與 `display_order = 10`
   - 必須是 `2 → 10`，不能是字串排序造成 `10 → 2`。

3. 部分班級 `display_order = NULL`
   - NULL 班級必須排在最後。

4. 多個班級的 `display_order` 相同
   - 再依 `class_name` 排序。

5. 同一班級多位學生
   - 維持 `seat_no ASC → student_number ASC`。

6. 確認成績輸入、修改及儲存功能與修改前一致。

---

## Change Log

完成修改後新增或更新：

```text
評量成績輸入排序調整0929.md
```

記錄：

- 修改檔案
- 修改位置
- 原排序方式
- 新排序方式
- `class.display_order` 的 NULL 處理方式
- 測試結果
- 確認其他評量成績輸入與儲存邏輯未變動