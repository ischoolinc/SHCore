# Todo — Preserve `<Extension>` structure; change only `<UseText>`

## Objective
Fix score editing so that when users change from a 缺考文字（如「缺」）到**數字**或反之，**只修改 `<Extension><UseText>` 的值**，其餘 `<Extension>` 內的節點/屬性/順序都保持不變。並確保未離開儲存格也能正確儲存（提交最後一格編輯）。

---

## Scope
- File: `CourseScorePalmerwormItem.cs`
- Functions to modify: `Save()`, `SaveExamScore()` (update & insert branches)
- Must not overwrite or delete other nodes inside `<Extension>` (e.g., `<UseTextA>Test</UseTextA>`).

---

## 1) Commit last edit before saving
**Search**: `public override void Save()`  

**Patch**
```csharp
public override void Save()
{
+   // Commit any in-place edit so change detection works (ensures Yellow state).
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

## 2) Helpers (place as private methods in the class)
**Add** (if not present):
```csharp
// Return existing <UseText> or create one; do NOT touch other nodes.
private static System.Xml.Linq.XElement GetOrCreateUseText(System.Xml.Linq.XElement root)
{
    var node = root.Element("UseText") ?? root.Descendants("UseText").FirstOrDefault();
    if (node == null)
    {
        node = new System.Xml.Linq.XElement("UseText");
        root.Add(node);
    }
    return node;
}

private static string EscapeSqlLiteral(string s)
    => (s ?? string.Empty).Replace("'", "''");
```

---

## 3) Update branch — change `<UseText>` only, preserve others
**Search**: In `SaveExamScore()` inside the **update** case:
```csharp
if (cell.Style.BackColor == Color.Yellow && cell.Value != null && !string.IsNullOrEmpty(info.SecID))
{
    // you already compute: strValue, xmlExtension, etc.
}
```
**Replace the extension-handling part with:**
```csharp
string strValue = cell.Value == null ? "" : cell.Value.ToString();
string strScore = strValue;

// Keep a copy of original for diff
string originalXml = xmlExtension ?? string.Empty;

// Build editable root (preserve existing XML if any)
System.Xml.Linq.XElement root = null;
if (!string.IsNullOrEmpty(originalXml))
{
    try { root = System.Xml.Linq.XElement.Parse(originalXml); }
    catch { root = null; } // if parsing fails, fallback to new root
}
if (root == null) root = new System.Xml.Linq.XElement("Extension");

// 缺考文字 → map score & set UseText to that text; else numeric → clear UseText
if (ScoreValueMangTextDict.ContainsKey(strValue))
{
    strScore = ScoreValueMangTextDict[strValue];
    GetOrCreateUseText(root).Value = strValue;          // only change UseText value
}
else
{
    GetOrCreateUseText(root).Value = string.Empty;      // numeric → empty UseText
}

// write score
updateHelper.AddElement("ScoreSheetList/ScoreSheet", "Score", strScore);

// serialize (no formatting to minimize diffs)
string newXml = root.ToString(System.Xml.Linq.SaveOptions.DisableFormatting);

// only update DB when changed
if (!string.Equals(originalXml, newXml, System.StringComparison.Ordinal))
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

> 此段只會改 `<UseText>` 的 Value；其他節點（如 `<UseTextA>` 等）完全不變。

---

## 4) Insert branch — create **minimal** `<Extension>` with `<UseText>` only
**Search**: In `SaveExamScore()` inside the **insert** case (`// 新增`).  
**Replace the construction of `xmlExtension` with:**
```csharp
string xmlExtension = "";
string strValue = cell.Value == null ? "" : cell.Value.ToString();
string strScore = strValue;

var root = new System.Xml.Linq.XElement("Extension");

if (ScoreValueMangTextDict.ContainsKey(strValue))
{
    strScore = ScoreValueMangTextDict[strValue];
    root.SetElementValue("UseText", strValue);          // only UseText
}
else
{
    root.SetElementValue("UseText", string.Empty);      // numeric → empty UseText
}

xmlExtension = root.ToString(System.Xml.Linq.SaveOptions.DisableFormatting);

// score
insertHelper.AddElement("ScoreSheetList/ScoreSheet", "Score", strScore);

// apply extension AFTER insert
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

> 新增時無既有 extension，可以安全建立最小 `<Extension>`；往後如果再更新，會按「更新分支」僅更動 `<UseText>`。

---

## 5) Validation / Test
- [ ] 原本：`<Extension><UseText>缺</UseText><UseTextA>Test</UseTextA></Extension>` → 改 100 分：  
      結果：`<UseText>` 變空，`<UseTextA>Test</UseTextA>` **仍在**。
- [ ] 改回「缺」：只把 `<UseText>` 改為「缺」，其他節點不變。
- [ ] 不切換儲存格直接儲存：仍會成功（因 `dgvScore.EndEdit()`）。
- [ ] 缺 → 缺（相同值）：若 XML 未變，不會多餘 UPDATE。

