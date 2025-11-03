# Todo — Fix: When "缺" → numeric score, clear only `<Extension><UseText>` (preserve all other nodes)

## Goal
When a user changes a score from a 缺考文字 (e.g., `缺` → DB score `-1/-2`) to a **numeric** score (e.g., `60`, `100`), make sure the database **only** clears `<Extension><UseText>` (to empty) and **does not modify any other nodes/attributes** within `<Extension>`.  
Also ensure saving works even if the user doesn't leave the edited cell (commit edit before saving).

## Scope
- File: `CourseScorePalmerwormItem.cs`
- Functions to touch: `Save()`, `SaveExamScore()` (update & insert branches only)
- Constraint: **Only modify the value of `<UseText>`**. Do not add/remove/reorder other nodes under `<Extension>` (except when `<Extension>` is missing, we may create a minimal `<Extension><UseText></UseText></Extension>`).

---

## 1) Commit last cell edit before saving
**Search**: `public override void Save()`

**Patch**
```csharp
public override void Save()
{
+   // Ensure the last in-place edit is committed so the row is marked as edited (Yellow).
+   dgvScore.EndEdit();

    if (!isAllValid())
    {
        MsgBox.Show("資料輸入不正確，請修正後再行儲存：" + Title);
        return;
    }
    ...
}
```

---

## 2) Update branch: Only clear or set `<UseText>`; preserve other `<Extension>` content
**Search**: Inside `SaveExamScore()` → the **update** branch (i.e., `// 修改` case):
```csharp
if (cell.Style.BackColor == Color.Yellow && cell.Value != null && !string.IsNullOrEmpty(info.SecID))
{
    // existing code that builds updateHelper, strValue, strScore and loads xmlExtension
}
```
Right **after** we compute `strValue`, `strScore`, and have `xmlExtension` (the current DB value for this SCE_TAKE), apply the following logic:

**Rules**
- If `strValue` is a 缺考文字 (`ScoreValueMangTextDict.ContainsKey(strValue)`):
  - Map `strScore = ScoreValueMangTextDict[strValue]`.
  - In `<Extension>`, set `<UseText>` to that 缺考文字。**Only touch `<UseText>` value**; keep any other nodes intact.
- Else (normal numeric value):
  - Keep `strScore` as the numeric string.
  - In `<Extension>`, set `<UseText>` to **empty string**. **Only touch `<UseText>` value**; keep other nodes intact.
- If no `<Extension>` exists in DB for this row (`xmlExtension` empty or unparsable):
  - Create **minimal** `<Extension><UseText>(value or empty)</UseText></Extension>` just for this record.
- Push the SQL `UPDATE` for `extension` **only if** the serialized XML actually changed from the original (to avoid noisy updates).

**Patch sketch**
```csharp
string strValue = cell.Value == null ? "" : cell.Value.ToString();
string strScore = strValue;

// remember original extension for diff check
string originalXml = xmlExtension ?? string.Empty;

// helper: escape SQL literals
// (Add the method in section 4 if missing)
string Escape(string s) => EscapeSqlLiteral(s);

// Build an editable XDocument of the extension
System.Xml.Linq.XElement root = null;
if (!string.IsNullOrEmpty(originalXml))
{
    try { root = System.Xml.Linq.XElement.Parse(originalXml); }
    catch { root = null; }
}

if (ScoreValueMangTextDict.ContainsKey(strValue))
{
    // 缺考文字 → map score and set UseText to that text
    strScore = ScoreValueMangTextDict[strValue];
    if (root == null) root = new System.Xml.Linq.XElement("Extension");

    // Only modify <UseText> value; do not touch other nodes
    var useText = root.Element("UseText") ?? root.Descendants("UseText").FirstOrDefault();
    if (useText == null)
    {
        useText = new System.Xml.Linq.XElement("UseText", strValue);
        root.Add(useText);
    }
    else
    {
        useText.Value = strValue;
    }
}
else
{
    // Normal numeric → clear UseText only
    if (root == null) root = new System.Xml.Linq.XElement("Extension");

    var useText = root.Element("UseText") ?? root.Descendants("UseText").FirstOrDefault();
    if (useText == null)
    {
        useText = new System.Xml.Linq.XElement("UseText", string.Empty);
        root.Add(useText);
    }
    else
    {
        useText.Value = string.Empty;
    }
}

// write the numeric or mapped score
updateHelper.AddElement("ScoreSheetList/ScoreSheet", "Score", strScore);

// serialize without formatting to minimize noise
string newXml = root.ToString(System.Xml.Linq.SaveOptions.DisableFormatting);

// only push UPDATE when changed
if (!string.Equals(originalXml, newXml, System.StringComparison.Ordinal))
{
    string sql = string.Format(@"
        UPDATE sce_take
           SET extension = '{0}'
         WHERE id = {1};",
        Escape(newXml),
        info.SecID);
    updateExtensionList.Add(sql);
}
```

**Notes**
- We only touch the `<UseText>` node's **value**:
  - If `<Extension>` contains other nodes (e.g., `<OtherInfo>...</OtherInfo>`), they remain unchanged.
  - If `<UseText>` is missing, we **add exactly one** `<UseText>`; we don't remove, reorder, or alter anything else.

---

## 3) Insert branch: Set minimal `<UseText>` only (no other nodes)
For the **insert** branch (`// 新增` case), if we need to create an extension:
- 缺考文字 → `<UseText>` set to that text.
- 正常數字 → `<UseText>` set to empty string.
- Since no prior extension exists for a new SCE_TAKE record, creating the minimal `<Extension><UseText>...</UseText></Extension>` is acceptable and does not violate the constraint.

**Patch sketch**
```csharp
string xmlExtension = "";
string strValue = cell.Value == null ? "" : cell.Value.ToString();
string strScore = strValue;

if (ScoreValueMangTextDict.ContainsKey(strValue))
{
    strScore = ScoreValueMangTextDict[strValue];
    var root = new System.Xml.Linq.XElement("Extension");
    root.SetElementValue("UseText", strValue);
    xmlExtension = root.ToString(System.Xml.Linq.SaveOptions.DisableFormatting);
}
else
{
    var root = new System.Xml.Linq.XElement("Extension");
    root.SetElementValue("UseText", string.Empty);
    xmlExtension = root.ToString(System.Xml.Linq.SaveOptions.DisableFormatting);
}

insertHelper.AddElement("ScoreSheetList/ScoreSheet", "Score", strScore);

if (!string.IsNullOrEmpty(xmlExtension))
{
    string sql = string.Format(@"
        UPDATE sce_take
           SET extension = '{0}'
         WHERE ref_exam_id = {1}
           AND ref_sc_attend_id = {2};",
        EscapeSqlLiteral(xmlExtension),
        ((KeyValuePair<string, string>)cboExam.SelectedItem).Key,
        info.AttendID);

    insertExtensionLList.Add(sql);
}
```

---

## 4) Add a tiny helper for SQL literal escaping (if not present)
**Search**: anywhere private inside `CourseScorePalmerwormItem`.
```csharp
private static string EscapeSqlLiteral(string s)
{
    return (s ?? string.Empty).Replace("'", "''");
}
```

---

## 5) Verification
- [ ] Case A: 原本 `extension` = `<Extension><UseText>缺</UseText><Other>ABC</Other></Extension>`，改成 `100` 後：  
      - `score` 變 `100`；`extension` → `<Extension><UseText></UseText><Other>ABC</Other></Extension>`（**Only `<UseText>` 改空**）。
- [ ] Case B: 原本 `extension` 有多層節點，`<UseText>` 不是直屬子節點：改成 `100` 後只見 `<UseText>` 變空，其他節點不變。
- [ ] Case C: 原本無 `extension`：改成 `100` 後 → `<Extension><UseText></UseText></Extension>`（最小化結構）。
- [ ] Case D: 改回「缺」：`<UseText>缺</UseText>` 被填入，其他節點仍保持原樣。
- [ ] 不切換儲存格直接儲存：因 `dgvScore.EndEdit()`，仍會正確更新 `<UseText>`。
