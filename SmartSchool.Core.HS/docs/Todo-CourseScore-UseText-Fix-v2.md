# Todo — Only clear `<UseText>` and preserve other `<Extension>` data

## Goal
When a score that previously mapped to a缺考文字 (e.g., score = -1 → `<UseText>缺</UseText>`) is changed to a normal numeric (e.g., 60), **clear only `<UseText>`** to empty while **preserving ALL other nodes/attributes inside `<Extension>`**. Do **not** replace the entire `<Extension>` blob.

## Context
- Target file: `CourseScorePalmerwormItem.cs`
- Main areas: `Save()` and `SaveExamScore()` (update & insert branches)
- We still need to make sure the last in-place edit is committed before saving.

---

## Changes

### 1) Commit last cell edit on Save
**Search**: `public override void Save()`

**Patch**
```csharp
public override void Save()
{
+   // Commit any in-place edit so change detection (Yellow) takes effect.
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

### 2) Update branch (existing SCE_TAKE): only clear `<UseText>` and keep everything else
**Search**: Inside `SaveExamScore()` update branch (the `// 修改` case). After you compute `strValue`, `strScore`, and have `xmlExtension` (possibly empty).

**Rules**:
- If the edited value is a 缺考文字 (exists in `ScoreValueMangTextDict`), set `<UseText>` to that text (create if missing). Keep other nodes intact.
- Else (normal numeric): set `<UseText>` to **empty string** (create if missing). Keep other nodes intact.
- If there was no `extension` previously, create the minimal `<Extension><UseText></UseText></Extension>` only (no other nodes).
- Push an UPDATE only if the serialized XML actually changed, or if there was no extension at all.

**Patch sketch**
```csharp
// Decide score text & extension manipulation
string strValue = cell.Value == null ? "" : cell.Value.ToString();
string strScore = strValue;

// Load current DB extension into xmlExtension if present
string originalXml = xmlExtension ?? string.Empty;

XElement elmRoot = null;

// helper to get or create <UseText> (only one)
Func<XElement, XElement> getOrCreateUseText = (root) =>
{
    // Prefer direct child <UseText>, otherwise fallback to first descendant named UseText
    var direct = root.Element("UseText");
    if (direct != null) return direct;

    var any = root.Descendants("UseText").FirstOrDefault();
    if (any != null) return any;

    var created = new XElement("UseText");
    root.Add(created);
    return created;
};

if (ScoreValueMangTextDict.ContainsKey(strValue))
{
    // 缺考：map value and set UseText to the text (e.g., "缺")
    strScore = ScoreValueMangTextDict[strValue];
    if (string.IsNullOrEmpty(originalXml))
    {
        elmRoot = new XElement("Extension");
        getOrCreateUseText(elmRoot).Value = strValue;
    }
    else
    {
        try
        {
            elmRoot = XElement.Parse(originalXml);
            getOrCreateUseText(elmRoot).Value = strValue;
        }
        catch
        {
            // Fallback: create minimal but DO NOT drop other data on purpose—
            // parsing failed means we must replace with safe structure
            elmRoot = new XElement("Extension");
            getOrCreateUseText(elmRoot).Value = strValue;
        }
    }
}
else
{
    // Normal numeric: set UseText to empty, preserve other nodes
    if (string.IsNullOrEmpty(originalXml))
    {
        elmRoot = new XElement("Extension");
        getOrCreateUseText(elmRoot).Value = string.Empty;
    }
    else
    {
        try
        {
            elmRoot = XElement.Parse(originalXml);
            getOrCreateUseText(elmRoot).Value = string.Empty;
        }
        catch
        {
            // Parsing failed → create minimal Extension with empty UseText
            elmRoot = new XElement("Extension");
            getOrCreateUseText(elmRoot).Value = string.Empty;
        }
    }
}

// Apply Score
updateHelper.AddElement("ScoreSheetList/ScoreSheet", "Score", strScore);

// Serialize without formatting to reduce diff churn
string newXml = (elmRoot ?? new XElement("Extension")).ToString(SaveOptions.DisableFormatting);

// Only push UPDATE when xml actually changed, OR when original was empty
if (!string.Equals(originalXml, newXml, StringComparison.Ordinal))
{
    string sql = string.Format(@"
        UPDATE sce_take
           SET extension = '{0}'
         WHERE id = {1};",
        EscapeSqlLiteral(newXml),
        info.SecID);
    updateExtensionList.Add(sql);
}
```

> This logic **never deletes or rewrites other nodes/attributes** under `<Extension>`. It only touches `<UseText>` (create if missing → set value to 缺考文字 or empty string).

---

### 3) Insert branch (new SCE_TAKE): write `<UseText>` minimally
**Search**: The `// 新增` branch inside `SaveExamScore()`.

**Rules**:
- For 缺考文字 → map score and set `<UseText>` to that text.
- For normal numeric → create minimal `<Extension><UseText></UseText></Extension>`.
- Since this is a new SCE_TAKE, there is no prior extension to preserve.

**Patch sketch**
```csharp
string xmlExtension = "";
string strValue = cell.Value == null ? "" : cell.Value.ToString();
string strScore = strValue;

if (ScoreValueMangTextDict.ContainsKey(strValue))
{
    strScore = ScoreValueMangTextDict[strValue];
    var root = new XElement("Extension");
    root.SetElementValue("UseText", strValue);
    xmlExtension = root.ToString(SaveOptions.DisableFormatting);
}
else
{
    var root = new XElement("Extension");
    root.SetElementValue("UseText", string.Empty);
    xmlExtension = root.ToString(SaveOptions.DisableFormatting);
}

insertHelper.AddElement("ScoreSheetList/ScoreSheet", "Score", strScore);

// Always apply the extension after insert
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

### 4) Add a small SQL escape helper (if not already added)
**Search**: inside `CourseScorePalmerwormItem` (private scope).

**Patch**
```csharp
private static string EscapeSqlLiteral(string s)
{
    return (s ?? string.Empty).Replace("'", "''");
}
```

---

## Post-Change Checklist
- [ ] 原本 `extension` 內含多個節點且有 `<UseText>缺</UseText>`：改 60 後，只有 `<UseText>` 變空，其它節點原封不動。
- [ ] 原本 `extension` 只有 `<UseText>缺</UseText>`：改 60 後變 `<UseText></UseText>`。
- [ ] 原本沒有 `extension`：改 60 後得到 `<Extension><UseText></UseText></Extension>`。
- [ ] 改回缺：`<UseText>缺</UseText>` 被設定，其它節點不變。
- [ ] 不離開儲存格直接儲存仍有效（因為 `dgvScore.EndEdit()`）。
