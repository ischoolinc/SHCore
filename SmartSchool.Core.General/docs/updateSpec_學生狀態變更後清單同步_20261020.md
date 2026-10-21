# [客服部] 學生資料項目變更狀態後，學生清單未即時更新 — 修改方案與測試流程

- Basecamp 待辦：[系統功能修改/新增 事由[客服部]學生資料項目進行狀態變更，清單無即時更新，需重新整理操作問題](https://app.basecamp.com/4399967/buckets/15765350/todos/10421515849)（ID `10421515849`）
- 來源：客服部 2026/10/20 回報。左下角篩選沒有勾選某個狀態時（例：只勾「在校學生」），在右側學生資料上方的狀態標籤把學生改成該狀態（例：「畢業或離校」），中間的學生清單沒有即時更新，需要手動重新整理。
- 參考：JHCore 已修正相同問題，見 `JHCore/JHSchool/docs/updateSpec_學生狀態變更後畫面同步_20261014.md`（commit `43db72a`，協同中學待辦 `9999684572`）。本方案依 JHCore 的做法調整成 SHCore 的寫法。
- 測試環境：`demo.h.beibei.lai`／從 Visual Studio 啟動修改後的 SHCore
- 版本：待定（目前 `SmartSchool.Core.General` 為 1.0.9.50）
- 狀態：**已依修改方案 1～6 修改程式（2026/10/21），測試完成（第一輪 T6-4 未通過，修正後第二輪重測通過）**（結果見「五、測試結果」）

---

## 一、JHCore 與 SHCore 取資料方式比較

修改前先確認 JHCore 的寫法能不能直接搬到 SHCore。兩邊的學生快取都是自己實作的 `CacheManager`，不是 K12.Data，所以「K12.Data 更新了，自己的快取不知道」這個問題兩邊一樣。

| 項目 | JHCore | SHCore |
|---|---|---|
| 學生快取類別 | `JHSchool.Student : LegacyPresentBase<StudentRecord>`（底層 `Framework.CacheManager`） | `SmartSchool.StudentRelated.Student : CacheManager<BriefStudentData>`（`SmartSchool.Core/CacheManager.cs`） |
| 快取資料來源 | `Feature.QueryStudent.GetAllStudents()`／`GetStudents()` | `SmartSchool.Feature.QueryStudent.GetAbstractList()`／`GetAbstractInfo()`（DSA XML 轉 `BriefStudentData`） |
| 單筆背景同步 | `SyncDataBackground(keys)` | `SyncDataBackground(keys)`，在背景執行緒取資料，完成後引發 `ItemUpdated`（背景執行緒時先排隊，再回 UI 執行緒引發） |
| 清單面板 | `Present`（`LegacyPresentBase` 包一層 `NLDPanel`） | 直接用 `K12.Presentation.NLDPanels.Student` |
| 選取變更事件 | `this.SelectedListChanged`（`LegacyPresentBase` 轉發 `Present.SelectedSourceChanged`） | `K12.Presentation.NLDPanels.Student.SelectedSourceChanged` |
| 篩選 | `FilterMenu[name].Click` → `FillFilter()` | `FilterMenu.PopupClose` → `SetSource()` → `FillFilter()` |
| 其他畫面通知快取更新的方式 | 直接呼叫 `Student.Instance.SyncAllBackground()`／`SyncDataBackground()` | 廣播 `SmartSchool.Broadcaster.Events.Items["學生/資料變更"]`：`BeforeInvoke` 中**同步**呼叫 `SyncData()`，`Handler` 中呼叫 `K12.Data.Student.RemoveByIDs()` |
| 是否訂閱 `K12.Data.Student.AfterUpdate` | 修正後有 | **沒有**（只訂閱 `AfterDelete`，`Student.cs:595`） |
| 年級樹 | `GradeYear_Class_View.Layout()`：每次清掉重建新的結點，需修正（JHCore 問題 4） | `Divider/ClassDivider.Divide()`：沿用舊的結點物件重新加入，選取結點以物件記住（`_SelectedNode`），預期沒有 JHCore 問題 4，需實測確認 |
| 另外使用的資料 Library | — | `SHSchool.Data`（`SHStudent`，包裝 K12.Data） |

### 共用 DLL 的差異

| DLL | JHCore | SHCore | 說明 |
|---|---|---|---|
| FISCA.Presentation | 1.1.0.19 | 1.1.0.19 | 檔案相同 |
| FISCA.Data | 1.0.0.3 | 1.0.0.3 | 檔案相同 |
| K12.Data | 2.0.0.15 | 2.0.0.15 | 版本號相同、檔案不同 |
| K12.Presentation | 1.0.0.0 | 1.0.0.0 | 版本號相同、檔案不同 |
| Framework | 1.0.0.10 | 1.0.0.2 | SHCore 的學生快取不使用 Framework 的 `CacheManager`，不影響本方案 |

JHCore 修正用到的 API，已用反射確認 SHCore 的 `Library` 中都有：

- `K12.Data.Student.AfterUpdate`（另有 `AfterInsert`、`AfterDelete`、`AfterChange`）
- `FISCA.Presentation.NLDPanel.DetailContents`、`SelectedSource`、`SelectedSourceChanged`
- `FISCA.Features.FeatureMap`、`FISCA.Features.Invoke`

**結論**：差異不大，JHCore 的做法可以沿用。要改的是呼叫方式：把 `Present.xxx` 換成 `K12.Presentation.NLDPanels.Student.xxx`，並配合 SHCore 的廣播機制調整呼叫順序（見「三、修改方案」）。

---

## 二、問題原因

學生狀態有三個變更途徑，各自用不同方式通知其他畫面：

| | 學生資料上方的狀態標籤 | 學生清單右鍵「變更學生狀態」 | 上方功能按鈕「刪除」 |
|---|---|---|---|
| 程式位置 | 類別模組 `Tagging.BaseModel.StudentBar`（不在 SHCore） | `StudentRelated/ChangeStatusBatch.cs` | `StudentRelated/Process/StudentIUD/StudentIDUProcess.cs` |
| 寫入方式 | `K12.Data.Student.Update()` | `Feature.EditStudent.ChangeStudentStatus()`（直接呼叫 DSA） | `Feature.EditStudent.ChangeStudentStatus()` |
| 引發 K12.Data `AfterUpdate` | 會 | 不會 | 不會 |
| 狀態標籤是否更新 | 會（類別模組有訂閱 `AfterUpdate`） | **不會 → 問題 1** | **不會 → 問題 1** |
| 學生清單是否更新 | **不會 → 問題 2（本次回報）** | 會（廣播「學生/資料變更」） | 會（廣播「學生/資料變更」） |

1. **問題 2（本次回報）**：從狀態標籤變更時，只會引發 K12.Data 的 `AfterUpdate`，不會廣播「學生/資料變更」。SHCore 的學生快取（`Student.Instance`）沒有訂閱 `AfterUpdate`，快取中的 `BriefStudentData.Status` 一直是舊值，`FillFilter()` 用舊狀態篩選，所以學生留在清單中，要按右鍵「重新整理」才會正確。
2. **問題 1（與 JHCore 相同，2026/10/20 已在待處理檢視實測確認）**：右鍵批次變更與上方「刪除」按鈕只廣播「學生/資料變更」，沒有通知類別模組，不切換學生時，狀態標籤會停在舊狀態。
   - 廣播的 `BeforeInvoke` 呼叫 `SyncData()` 更新 SHCore 快取並重新篩選清單；`Handler` 呼叫的 `K12.Data.Student.RemoveByIDs()` 只移除 K12.Data 的快取（IL 為 `EntityCache.Remove`），不會引發任何事件。
   - 類別模組只在 K12.Data `AfterUpdate`、類別變更、`學生.Tag.Reload`、右側顯示的學生改變（`OnPrimaryKeyChanged`）時重新讀取，上述兩個動作都不會觸發。
   - 清單刷新後，FISCA.Presentation 的 `NavContentPresentation.CheckSelection()`：右側顯示的學生仍被選取時不做任何事；沒有任何選取時隱藏右側頁面，但 `DetailPane` 仍記著原本的學生。`DetailPane.set_PrimaryKey` 設定相同學生時直接略過，所以不會重新讀取。
   - **介面上不容易發現的原因**：在班級檢視、篩選「在校學生」下改成休學或畢業時，學生被篩選掉、右側頁面隱藏，通常會先點選其他學生再點回來，右側學生改變，標籤就會重新讀取。
   - **會看到舊狀態的情況**：變更後學生仍留在清單且被選取（待處理檢視、篩選「所有學生」、新狀態仍符合篩選，例：「在校學生」下由「一般」改成「延修」）；或學生被移出清單後，沒有先點選其他學生就直接到待處理點回同一位學生。
   - FISCA.Presentation 兩邊為同一個檔案，K12.Data 的 IL 除組件識別碼外相同，類別模組也相同，所以這部分 SHCore 與 JHCore 的行為一致。
3. **問題 3（兩個模組競跑）**：JHCore 修正問題 2 後發現：清單刷新後，學生不符合篩選被移除，右側學生資料頁面隱藏；類別模組讀取新狀態時，判斷標籤已不在畫面上就不更新（`StudentBar` 的 `if (!StatusVisible) return;`），標籤停在舊狀態。
   - 從狀態標籤變更：SHCore 加上問題 2 的同步後，SHCore 刷新清單與類別模組更新標籤會同時進行，會有同樣的時序問題。
   - 批次變更、「刪除」按鈕：加上問題 1 的通知後，類別模組重新讀取時，右側頁面可能已因學生被篩選掉而隱藏，同樣需要保留機制。

4. **問題 4（切換學生時的例外，2026/10/21 重現問題 2 時發現）**：上一位學生有班級時，點選另一位學生，「學生班級資訊」（`ClassInfoPalmerwormItem`）會拋出 `NullReferenceException`。例外被 `catch { }` 吃掉，畫面沒有錯誤，但從 VS 偵錯執行時會停下來。
   - 觸發順序：`OnBackgroundWorkerCompleted` → `Initialize()` 第 97 行 `cboGradeYear.SelectedItem = null` → `cboGradeYear_SelectedIndexChanged_1` 第 368 行 `cboClass.SelectedItem = null` → 班級選取變成 -1，引發 `cboClass_SelectedIndexChanged_1` → 第 402 行把 null 拆箱成 `KeyValuePair<string, string>`（值型別）。
   - `ComboBoxAdv.OnSelectedIndexChanged` 在選取為 -1 時直接引發事件（`ComboBoxAdv.cs:30-34`）；同一事件中的 `GetValue()` 有檢查 null，只有第 402 行直接轉型。
   - 例外發生時 `cboClass.Items.Count` 為 9（上一位學生 3 年級的 8 個班加「<空白>」），表示發生在第 368 行清除選取時，第 369 行 `Items.Clear()` 之前。
   - 結果不受影響：這段 `try` 是設定科別浮水印，班級隨後會被清空，載入新學生時會再引發一次事件並正常設定。
5. **問題 5（下拉選單有資料但不顯示文字，2026/10/21 發現）**：「學生班級資訊」的年級、班級、科別，以及「基本資料」的性別一直是空白；正式版（`demo.ischool.h`）與 VS 開發版都會發生，與本次修改無關。
   - 資料有選到：打開下拉選單，選項都在且有反白（年級反白第 4 列「3」、班級反白第 3 列「普303」），只是每一列都沒有文字。
   - 原因：`ComboBoxAdv` 是 DotNetBar `ComboBoxEx`（`DrawMode.OwnerDrawFixed`），設計檔的列高 `ItemHeight = 16`，但微軟正黑體 9.75pt 在 96 DPI 的字型高度是 18px。列高比字型高度小時，DotNetBar 會畫背景與反白，但不畫文字。
   - 驗證：用測試視窗建立相同設定的 `ComboBoxEx`，列高 16、17 不顯示文字，18、20 正常；改用字串選項結果相同，與 `KeyValuePair` 無關。直接呼叫 DotNetBar 的 `DrawObjectItem` 畫到 24px 高的點陣圖，文字正常。DotNetBar 9.5.0.1、7.9.0.5 結果相同，正式版與開發版使用的版本也相同（9.5.0.1）。
   - 受影響的 `ComboBoxAdv`（列高 16）：

     | 畫面 | 欄位 | 樣式 | 現象 |
     |---|---|---|---|
     | 學生班級資訊（`ClassInfoPalmerwormItem.designer.cs`） | 年級、班級、科別 | `DropDownList` | 平常就空白 |
     | 基本資料（`BaseInfoPalmerwormItem.Designer.cs`） | 性別 | `DropDownList` | 平常就空白 |
     | 地址（`AddressPalmerwormItem.Designer.cs`） | 縣市、鄉鎮 | `DropDown` | 平常有文字（輸入框），打開清單時選項可能空白 |

     家長資訊的 `ComboBoxAdv` 列高為 18，不受影響。另有一般 `ComboBoxEx` 列高 16、17（例：獎懲的學年度、學期、事由），可能有相同狀況，不在這次範圍。

> 類別模組的分析參考 `ischool-desktop/MOD_Tagging`（組織舊帳號，最後更新 2017），現行版本可能已有調整。SHCore 的問題 1 已在待處理檢視實測確認；問題 3 的行為 JHCore 已在實際環境確認，SHCore 要在測試環境重現後再確認。

---

## 三、修改方案

### 修改的檔案

| 檔案 | 編碼 | 修改 |
|---|---|---|
| `SmartSchool.Core.General/StudentRelated/Student.cs` | UTF-8 BOM | 問題 2、3 |
| `SmartSchool.Core.General/StudentRelated/ChangeStatusBatch.cs` | UTF-8 BOM | 問題 1、3 |
| `SmartSchool.Core.General/StudentRelated/Process/StudentIUD/StudentIDUProcess.cs` | UTF-8 BOM | 問題 1、3 |
| `SmartSchool.Core.General/StudentRelated/Palmerworm/ClassInfoPalmerwormItem.cs` | **Big5** | 問題 4 |
| `SmartSchool.Core.General/StudentRelated/Palmerworm/ComboBoxAdv.cs` | **Big5** | 問題 5 |

### 1. `Student.cs`：訂閱 `AfterUpdate` 同步快取（問題 2）

在學生面板初始化（`Filter` 區段之後、`MotherForm.AddPanel(K12.Presentation.NLDPanels.Student)` 之前，約 `Student.cs:594`）加入：

```csharp
// 其他模組(如類別模組的學生狀態標籤)透過 K12.Data 變更學生時，同步這些學生的快取，讓清單與年級樹更新
K12.Data.Student.AfterUpdate += delegate(object sender, K12.Data.DataChangedEventArgs e)
{
    List<string> primaryKeys = new List<string>(e.PrimaryKeys);
    Action sync = delegate
    {
        KeepDisplayedInList(primaryKeys);
        SyncDataBackground(primaryKeys);
    };
    // K12.Data 的事件可能在背景執行緒引發，需回到 UI 執行緒再同步，避免跨執行緒更新清單
    if (MotherForm.Form.InvokeRequired)
        MotherForm.Form.BeginInvoke(sync);
    else
        sync();
};
```

- `SyncDataBackground` 完成後引發 `ItemUpdated`，再由既有的 `SetupPresentation()` 訂閱（`Student.cs:221`）呼叫 `SetSource()` 重新篩選，年級樹也會跟著更新。
- **不採用的做法**：在 `AfterUpdate` 中改成廣播「學生/資料變更」。廣播會通知所有訂閱的 SH 模組重新讀取，而 SHCore 內已有功能（例：`UpdateRecordForm.cs:217` 的 `SHStudent.Update`）本來就會更新 K12.Data，影響範圍比較大。所以比照 JHCore，只同步 SHCore 自己的學生快取。

### 2. `Student.cs`：暫時保留正在顯示的學生（問題 3）

比照 JHCore 新增欄位與方法：

```csharp
// 狀態變更後暫時保留在清單中、右側學生資料頁面正在顯示的學生，避免頁面在類別模組更新狀態標籤前就被隱藏；
// 改選其他學生、點選年級樹其他結點、切換篩選或重新整理時才依篩選移除
private List<string> _KeepKeys = new List<string>();
private bool _FillingFilter = false;

internal void KeepDisplayedInList(IEnumerable<string> keys)
{
    string displayedKey = "";
    foreach (DetailContent content in K12.Presentation.NLDPanels.Student.DetailContents)
    {
        if (!string.IsNullOrEmpty(content.PrimaryKey))
        {
            displayedKey = content.PrimaryKey;
            break;
        }
    }
    if (displayedKey == "") return;

    List<string> selected = new List<string>(K12.Presentation.NLDPanels.Student.SelectedSource);
    foreach (string key in keys)
        if (key == displayedKey && selected.Contains(key) && !_KeepKeys.Contains(key))
            _KeepKeys.Add(key);
}
```

其他要調整的位置：

| 位置 | 修改 |
|---|---|
| `FillFilter()`（`Student.cs:781`） | 記錄這次套用的篩選勾選（`_AppliedFilterState`）；依篩選算出 `list` 後，加入 `_KeepKeys` 中仍存在於 `Items` 的學生；`SetFilteredSource(list)` 前後設定 `_FillingFilter`（`try/finally`） |
| 選取變更 | 訂閱 `K12.Presentation.NLDPanels.Student.SelectedSourceChanged`：`_FillingFilter` 期間或 `_KeepKeys` 為空時不處理；不再選取的學生從 `_KeepKeys` 移除，有移除時重新 `FillFilter()` |
| 篩選選單 `FilterMenu.PopupClose`（`Student.cs:522`） | 篩選勾選與上次套用的不同時，才在呼叫 `SetSource()` 前 `_KeepKeys.Clear()`（見下方說明） |
| `ReloadData()`（`Student.cs`） | 呼叫 `SyncAllBackground()` 前先 `_KeepKeys.Clear()`。年級樹右鍵「重新整理」（`NavigationPlanner/StudentDividerProvider.cs:27`）、缺曠／獎懲檢視的「重新整理」（`Divider/AttendanceDivider.cs`、`Divider/DisciplineDivider.cs`）與匯入學生後都呼叫此方法 |

> 修正紀錄（2026/10/21）：原方案寫成修改 `NavPaneContexMenu` 的「重新整理」（`Student.cs:594`），但年級樹上看到的「重新整理」是 `StudentDividerProvider` 自己的選單，第一輪測試 T6-4 因此未通過。已改為在 `ReloadData()` 清除，`NavPaneContexMenu` 的處理維持原本寫法。

- `DetailContent` 需要 `using FISCA.Presentation;`（`Student.cs` 已有）。

#### 篩選勾選有改變才解除保留

JHCore 的篩選項目每點一次就馬上重新篩選，解除保留寫在點選事件（`FilterMenu[name].Click`）中，所以只有勾選真的改變時才會解除。SHCore 的篩選選單勾選期間不刷新，關閉選單（`PopupClose`）時才重新篩選一次；若在 `PopupClose` 直接清除保留，只是打開選單、不改勾選就關閉，保留的學生也會被移除。

為了與 JHCore 的行為一致，記錄上次套用的篩選勾選，關閉選單時比對，有改變才解除：

```csharp
// 上次套用到清單的篩選勾選，用來判斷關閉篩選選單時勾選是否有改變
private string _AppliedFilterState = "";

private string GetFilterState()
{
    return string.Join(",", new string[] {
        "" + btn一般生.Checked, "" + btn休學生.Checked, "" + btn延修生.Checked,
        "" + btn畢業及離校生.Checked, "" + btn刪除.Checked });
}
```

- `FillFilter()`：依篩選算出清單時，記錄 `_AppliedFilterState = GetFilterState();`。
- `FilterMenu.PopupClose`（`Student.cs:522`）改成：

```csharp
K12.Presentation.NLDPanels.Student.FilterMenu.PopupClose += delegate
{
    // 篩選勾選有改變時才完全依篩選顯示，不再保留狀態變更的學生；只打開再關閉選單時維持保留
    if (GetFilterState() != _AppliedFilterState)
        _KeepKeys.Clear();
    SetSource();
};
```

- 只比對實際用來篩選的 5 個項目（一般生、休學生、延修生、畢業及離校生、刪除）。「所有學生」「在校學生」「學籍在校學生」是快速勾選這 5 項的捷徑，不另外比對：例如由「在校學生」改成自訂勾選相同的項目，篩選結果不變，維持保留。
- 勾選後又改回原本的勾選再關閉選單，視為沒有改變，維持保留。

### 3. `ChangeStatusBatch.cs`：批次變更後通知類別模組（問題 1、3）

`_BKW_RunWorkerCompleted`（`ChangeStatusBatch.cs:266-270`）改成：

```csharp
if (idlist.Count > 0)
{
    FISCA.Presentation.MotherForm.SetStatusBarMessage("變更學生狀態成功");
    // 右側正在顯示的學生暫時保留在清單中，學生資料頁面不會被隱藏，狀態標籤才能完成更新
    // 廣播的 BeforeInvoke 會同步呼叫 SyncData 並刷新清單，所以要在廣播前先保留
    Student.Instance.KeepDisplayedInList(idlist);
    SmartSchool.Broadcaster.Events.Items["學生/資料變更"].Invoke(idlist.ToArray());
    // 批次變更直接呼叫 DSA，不會引發 K12.Data 的 AfterUpdate，需通知類別模組重新整理學生資料上方的狀態標籤
    if (FISCA.Features.FeatureMap.ContainsKey("學生.Tag.Reload"))
        FISCA.Features.Invoke("學生.Tag.Reload");
}
```

- SHCore 教師管理已有相同寫法：`TeacherRelated/RibbonBars/Manage.cs:129` 的 `FISCA.Features.Invoke("教師.Tag.Reload")`。
- `學生.Tag.Reload` 由類別模組 `StudentBar` 以 `FISCA.Features.TryRegister` 註冊，執行時呼叫 `OnPrimaryKeyChanged` 重新讀取目前學生；沒有載入類別模組時以 `FeatureMap.ContainsKey` 略過。
- 與 JHCore 的差異：JHCore 呼叫 `SyncAllBackground()`（背景），SHCore 是廣播（同步更新快取並刷新清單），所以 `KeepDisplayedInList` 一定要放在廣播**之前**，`學生.Tag.Reload` 放在廣播**之後**。

### 4. `StudentIDUProcess.cs`：上方「刪除」按鈕（問題 1、3）

廣播「學生/資料變更」（`StudentIDUProcess.cs:164`）前呼叫 `Student.Instance.KeepDisplayedInList(new string[] { stu_id })`，廣播後比照批次變更呼叫 `學生.Tag.Reload`（同樣先用 `FeatureMap.ContainsKey` 確認已註冊）。

### 5. `ClassInfoPalmerwormItem.cs`：班級沒有選取時不轉型（問題 4）

`cboClass_SelectedIndexChanged_1` 的 `try` 區塊（`ClassInfoPalmerwormItem.cs:400-408`）改成：

```csharp
try
{
    // 切換學生時會先清除班級選取並引發此事件，沒有選取班級時不需要讀取科別
    if (cboClass.SelectedItem == null) return;

    KeyValuePair<string, string> kv = ((KeyValuePair<string, string>)cboClass.SelectedItem);
    DSResponse rsp = QueryClass.GetClass(kv.Key);

    // (string.IsNullOrEmpty(cboDept.Text))
    cboDept.WatermarkText = rsp.GetContent().GetText("Class/Department");
}
catch { }
```

- 檢查放在第 390、398 行（`OnValueChanged`、`GetClassSeatNoList`）之後，原本清除班級時要做的值變更與座號清單重設不受影響，只略過讀取科別。
- 選擇「<空白>」項目時 `SelectedItem` 不是 null，維持原本行為（`QueryClass.GetClass("")` 失敗由 `catch` 處理），不在這次範圍。
- 檔案是 **Big5** 編碼，修改後要確認仍是 Big5，中文註解沒有變成亂碼（例：`git diff` 只出現新增的行）。

### 6. `ComboBoxAdv.cs`：列高至少等於字型高度（問題 5）

在 `ComboBoxAdv` 加入：

```csharp
protected override void OnHandleCreated(EventArgs e)
{
    base.OnHandleCreated(e);
    FitItemHeightToFont();
}

protected override void OnFontChanged(EventArgs e)
{
    base.OnFontChanged(e);
    FitItemHeightToFont();
}

/// <summary>
/// 自行繪製選項時，列高比字型高度小，DotNetBar 不會畫出選項文字，所以將列高調整為字型高度
/// </summary>
private void FitItemHeightToFont()
{
    if (DrawMode == DrawMode.OwnerDrawFixed && ItemHeight < Font.Height)
        ItemHeight = Font.Height;
}
```

- 設計檔先設定 `Font`、再設定 `ItemHeight = 16`，所以只在 `OnFontChanged` 調整會被設計檔蓋回去；`OnHandleCreated` 在設計檔設定完、控制項第一次顯示前執行，確保最後是調整後的列高。實作後要確認畫面上的列高確實變成 18。
- 只在列高**小於**字型高度時調高，不會超過字型高度；列高已足夠的選單（例：家長資訊的 18）不變。
- 檔案是 **Big5** 編碼，修改後要確認仍是 Big5，原有的中文註解沒有變成亂碼。

#### 對版面的影響（測試時特別注意，見 T14）

選單外框高度為「列高 + 6px」，列高變大，選單也會跟著變高：

| 字型（微軟正黑體） | 字型高度 | 修改前（列高 16） | 修改後 |
|---|---|---|---|
| 9.75pt（目前） | 18px | 22px，文字不顯示 | 24px（+2） |
| 12pt | 22px | 22px，文字不顯示 | 28px（+6） |
| 14.625pt（約 150% 縮放） | 26px | 22px，文字不顯示 | 32px（+10） |

| 畫面 | 與下一列的間距 | 版面縮放 | 何時會重疊 |
|---|---|---|---|
| 學生班級資訊 | 年級、班級下方 19px | 會（`AutoScaleMode.Font`），位置等比放大 | 不會 |
| 地址 | 縣市、鄉鎮下方 11px | — | 字型高度超過 27px |
| 基本資料 | 性別（y=79）到生日（y=107）只有 6px，修改後剩 4px | **不會**（`AutoScaleMode.None`） | 字型高度超過 22px（約 125% 縮放） |

- ischool 一般執行時沒有宣告支援高 DPI，Windows 會整個放大視窗，程式內的字型高度固定 18px，所以一般使用只會高 2px。
- 字型真的變大的情況：VS 偵錯時被改成 DPI aware（見 `desktop-操作注意事項.md`），或之後有人改了字型。這時基本資料的文字框本來就會因為不縮放而擠在一起，性別欄只是其中之一。
- 修改後年級、班級高 24px，同一列的座號選單、學號文字框是 25px，高度會比修改前更一致。

### 不需修改的部分

- **年級樹（JHCore 問題 4）**：SHCore 的 `ClassDivider.Divide()` 沿用舊的結點物件，選取結點用物件比對，不受人數文字改變影響；展開狀態需實測確認（T7）。若實測會收合，再另外修改 `ClassDivider.cs`。注意 `ClassDivider.cs` 是 **Big5** 編碼，修改時要保持原編碼或另外確認轉檔。
- **「刪除」按鈕身分證號檢查（JHCore 問題 5）**：SHCore 的 `StudentIDUProcess.cs:125-148` 學號、身分證號分別用各自的清單比對，沒有 JHCore 的誤用問題。

### 使用上的改變（回覆客服時需說明）

- 從狀態標籤變更後，學生清單與年級樹會依篩選即時更新，不需要手動重新整理。
- 右鍵批次變更或上方「刪除」後，右側正在顯示的學生的狀態標籤會即時更新，不需要切換學生。
- 狀態變更後，右側正在顯示的學生會**暫時留在清單**（即使已不符合篩選），年級樹人數也暫時把他算在內；改選其他學生、點選年級樹結點、變更篩選勾選或重新整理後，才依篩選移除、人數更新。只打開篩選選單、沒有改變勾選就關閉時，學生仍會保留。
- 批次變更時，只有右側正在顯示的那一位暫時保留，其他一起選取的學生在變更完成後馬上依篩選移除。
- 上方「刪除」按鈕按下後，學生不會像修改前一樣馬上從清單消失，標籤會顯示「刪除」。
- 「學生班級資訊」的年級、班級、科別，以及「基本資料」的性別會正常顯示（修改前是空白）；這幾個下拉選單會比修改前高一點（約 2px）。

---

## 四、測試流程

### 測試前準備

1. 從 Visual Studio 啟動修改後的 SHCore，登入 `demo.h.beibei.lai`。
2. 選定一個班級（例：普303）的 3 位測試學生，記錄目前的狀態與英文姓名，並記錄年級樹上 3 年級、普303 的人數。
3. 把這 3 位學生「加入至待處理」。學生改成非在校狀態後會從班級清單消失，從待處理才能繼續選到。
4. 記錄學生清單篩選目前勾選的項目。測試時除特別註明外，**篩選選「在校學生」**（一般生、延修生）。
5. 確認測試學生的學號、身分證號沒有和「畢業或離校」「休學」「刪除」狀態的學生重複。

### 名詞

- **班級檢視**：左側年級樹選取普303 後顯示的清單，受篩選條件影響。
- **待處理檢視**：左下「待處理學生」的清單，不受篩選條件影響。
- **保留**：狀態變更後，右側正在顯示的學生暫時留在班級清單中。

### 判斷結果的依據

- **狀態標籤**：右側學生資料上方的狀態文字與底色（一般：白、畢業或離校：綠、休學：黃、延修：淺綠、刪除：紅）。
- **學生清單與年級樹**：清單中的學生，以及年級樹各結點的人數、展開狀態、選取的結點。
- **系統歷程**：從狀態標籤變更為「學生狀態」；批次變更為「變更狀態」；「刪除」按鈕為「刪除學生」。

### 修改前先重現

- [x] 用修改前的版本照客服回報的步驟操作，確認從狀態標籤改成「畢業或離校」後，學生仍留在清單中（問題 2，2026/10/21 已確認）。
  - 篩選「在校學生」、普303（11 人），把座號 7 從「一般」改成「畢業或離校」，標籤即時變綠；點選其他學生後仍留在清單，人數仍為 11。右鍵「重新整理」後才移除（普303 10 人、所有學生 451 → 450）。
  - 反方向也一樣：在待處理改回「一般」後，回到普303 仍是 10 人、學生沒有出現，重新整理後才恢復 11 人。
  - 從 VS 偵錯執行時，切換學生會在 `ClassInfoPalmerwormItem.cs:402` 中斷（`NullReferenceException`，`cboClass.SelectedItem` 為 null）。例外會被同一段的 `catch { }` 處理，按 Continue 即可；原因與修正見問題 4。
- [x] 在待處理檢視用右鍵批次變更一位學生為「休學」，不切換學生，狀態標籤停在舊狀態（問題 1，2026/10/20 已確認）。

### 測試案例

| 分類 | 案例 |
|---|---|
| 問題 1：批次變更、刪除後狀態標籤即時更新 | T11、T12 |
| 問題 2、3：從狀態標籤變更 | T1、T2 |
| 問題 1、3：批次變更、刪除按鈕（班級檢視） | T3、T4、T5 |
| 問題 3：解除保留的時機 | T6 |
| 年級樹 | T7 |
| 回歸與副作用 | T8、T9、T10 |
| 問題 4：切換學生時的例外 | T13 |
| 問題 5：下拉選單顯示與版面 | T14 |

#### T1. 從狀態標籤變更，清單同步並保留正在顯示的學生（客服回報情境）

1. 篩選選「在校學生」，班級檢視選取普303，點選一位「一般」學生。
2. 點右側狀態標籤 →「畢業或離校」→ 確認視窗按「Yes」。
3. 點選普303 其他學生。
4. 到待處理點選這位學生，用狀態標籤改回「一般」，再點選待處理的其他學生，回到班級檢視普303。

**預期結果**

- [ ] 第 2 步：狀態標籤即時變成「畢業或離校」（綠底），學生暫時留在清單，右側頁面不消失。
- [ ] 第 3 步：這位學生從清單消失，年級樹人數減 1，普303 維持選取。修改前要手動「重新整理」。
- [ ] 第 4 步：標籤顯示「畢業或離校」；改回「一般」後依篩選重新出現在普303，人數恢復。
- [ ] 系統歷程有「學生狀態」的紀錄。

#### T2. 從狀態標籤變更為其他狀態（待處理檢視）

在待處理用狀態標籤對同一位學生依序變更為「休學」→「一般」→「延修」→「一般」，每一步都不要重新整理。

**預期結果**

- [ ] 每一步標籤都即時更新，學生一直留在待處理清單中。
- [ ] 回到班級檢視時，清單依目前狀態與篩選顯示。

#### T3. 批次變更單一學生（班級檢視）

1. 班級檢視選取普303，點選一位學生，右鍵「變更學生狀態」→「休學」→ Yes。
2. 點選普303 其他學生。
3. 到待處理點選這位學生，確認標籤後改回「一般」。

**預期結果**

- [ ] 第 1 步：學生暫時留在清單、右側頁面不消失，標籤即時變成「休學」。
- [ ] 第 2 步：這位學生才從清單消失，普303 人數減 1。
- [ ] 第 3 步：標籤是「休學」。

#### T4. 批次變更多位學生，只保留正在顯示的學生

1. 班級檢視選取普303，選取 3 位學生（記下右側顯示的是哪一位），右鍵改成「休學」。
2. 點選普303 其他學生。
3. 到待處理把 3 位改回「一般」，逐一點選確認標籤。

**預期結果**

- [ ] 第 1 步：只有右側正在顯示的學生留在清單，標籤即時變成「休學」；另外 2 位馬上從清單消失。
- [ ] 第 2 步：保留的學生從清單消失，普303 人數為正確值。
- [ ] 第 3 步：3 位的標籤都是「一般」。

#### T5. 上方功能按鈕「刪除」學生

1. 班級檢視選取普303，只點選一位學生，按上方「刪除」→ Yes。
2. 點選普303 其他學生。
3. 到待處理點選這位學生，用狀態標籤改回「一般」。

**預期結果**

- [ ] 第 1 步：沒有錯誤訊息；狀態標籤即時變成「刪除」（紅底），學生暫時留在清單。
- [ ] 第 2 步：學生從清單消失，普303 人數減 1。
- [ ] 第 3 步：改回「一般」後依篩選回到普303。
- [ ] 系統歷程有一筆「刪除學生」的紀錄。

#### T6. 解除保留的時機

每一項都先用狀態標籤把普303 一位學生改成「休學」（學生保留在清單），再用指定的操作離開，測完把學生改回「一般」再測下一項：

1. 點選普303 其他學生。
2. 點選年級樹其他結點（例：普303 → 普305）。
3. 打開篩選選單，變更勾選（例：勾選「休學生」）後關閉，再打開選單取消「休學生」後關閉。
4. 在年級樹按右鍵「重新整理」。
5. 打開篩選選單，不改任何勾選就關閉。
6. 打開篩選選單，勾選「休學生」後再取消（回到原本的勾選），然後關閉。

**預期結果**

- [ ] 1～4：被保留的學生依篩選從班級清單與年級樹移除，人數正確。
- [ ] 5、6：勾選沒有改變，被保留的學生仍留在清單，右側頁面不消失；之後點選普303 其他學生時才移除。
- [ ] 清單不會出現重複的學生，也沒有明顯閃爍或錯誤訊息。

#### T7. 年級樹展開與選取狀態

1. 展開 3 年級與 1 年級，保持 2 年級收合，選取普303。
2. 用狀態標籤把普303 一位學生改成「休學」，再點選普303 其他學生。
3. 用右鍵批次把另一位學生改成「休學」，再點選普303 其他學生。

**預期結果**

- [ ] 每一次清單刷新後，年級樹維持原本的展開狀態，普303 維持選取，清單不會被清空。
- [ ] 若會全部收合或選取消失，記錄情況，另外評估修改 `ClassDivider.cs`。

#### T8. 其他功能透過 K12.Data 更新學生（副作用檢查）

這次的修改會在任何功能透過 K12.Data（`SHStudent.Update`、`K12.Data.Student.Update`）更新學生時，多同步一次這些學生的快取。

1. 班級檢視點選一位學生，修改基本資料（例：英文姓名）並儲存，再改回原值。
2. 對一位學生新增一筆異動紀錄並儲存（`UpdateRecordForm` 會呼叫 `SHStudent.Update`），再刪除該筆異動。

**預期結果**

- [ ] 存檔正常完成，沒有錯誤訊息，清單與右側資料顯示修改後的內容。
- [ ] 學生仍在清單中（狀態沒有改變，仍符合篩選）。
- [ ] 畫面沒有明顯變慢、長時間顯示讀取中，或反覆刷新（確認廣播 `Handler` 中的 `K12.Data.Student.RemoveByIDs` 不會再引發 `AfterUpdate` 造成迴圈）。

#### T9. 狀態相同或取消變更（回歸）

1. 從狀態標籤選擇與目前相同的狀態。
2. 從狀態標籤選擇不同狀態，在確認視窗按「No」。

**預期結果**

- [ ] 兩種情況都不會變更狀態，也沒有新的系統歷程，清單沒有變化。

> 備註：第 2 步按「No」後，選單的勾會停在剛選的狀態、但標籤仍是原狀態。這是類別模組 `StudentBar` 原本的行為，不在這次 SHCore 的修改範圍內。

#### T10. 篩選切換（回歸）

依序切換篩選「所有學生」→「學籍在校學生」→「在校學生」，以及自訂勾選「畢業及離校生」。

**預期結果**

- [ ] 每次關閉篩選選單後，清單依篩選正確顯示，篩選選單標題（「篩選-所有學生」等）正確。

#### T11. 批次變更後狀態標籤即時更新（待處理檢視，問題 1）

1. 在待處理選取一位學生，確認右側狀態標籤為「一般」。
2. 右鍵「變更學生狀態」→「休學」→ Yes。**不要切換學生、不要重新整理。**
3. 同一位學生再用右鍵改回「一般」。
4. 在待處理選取 3 位學生（右側顯示其中一位），右鍵改成「休學」，再逐一點選這 3 位，最後一起改回「一般」。

**預期結果**

- [ ] 第 2 步：狀態標籤馬上變成「休學」（黃底），點開標籤的選單，打勾的是「休學」。修改前會停在「一般」，要切換學生才會變。
- [ ] 第 3 步：標籤馬上變回「一般」。
- [ ] 第 4 步：變更完成時右側學生的標籤馬上變成「休學」；逐一點選時每一位都是「休學」；3 位一直留在待處理清單中。
- [ ] 狀態列顯示「變更學生狀態成功」；系統歷程有對應的「變更狀態」紀錄。

#### T12. 移出清單後直接點回同一位學生（問題 1）

1. 篩選選「在校學生」，班級檢視選取普303，只點選一位學生，右鍵改成「休學」。
2. 不點選其他學生，直接到待處理點選這位學生。
3. 改回「一般」後，換成上方「刪除」按鈕重做第 1、2 步，最後用狀態標籤改回「一般」。

**預期結果**

- [ ] 第 2 步：標籤顯示「休學」。修改前因右側學生沒有改變、類別模組不重新讀取，會顯示舊的「一般」。
- [ ] 第 3 步：標籤顯示「刪除」（紅底）。

#### T13. 切換學生不再拋出例外（問題 4）

從 VS 偵錯（F5）啟動，維持「擲回 `NullReferenceException` 時中斷」的設定。

1. 班級檢視選取普303，點選一位學生，再點選普303 另一位學生。
2. 點選其他年級的學生（例：1 年級）。
3. 點選一位沒有班級的學生（例：「未分年級」）後，再點回普303 的學生。
4. 在一位學生的「學生班級資訊」中，把班級改成同年級的其他班（不儲存），確認科別浮水印，再按取消或切換學生放棄修改。

**預期結果**

- [ ] 1～3：VS 不會在 `ClassInfoPalmerwormItem.cs` 中斷；Output 視窗沒有新的 `NullReferenceException`。
- [ ] 1～3：「學生班級資訊」的年級、班級、座號、科別浮水印與修改前顯示相同。
- [ ] 4：改班級後科別浮水印換成該班的科別，座號清單更新；放棄修改後資料沒有變動。

#### T14. 下拉選單顯示文字，版面不跑版（問題 5，**特別注意版面**）

修改前先截圖：點選普303 一位有指定性別、地址的學生，截下「基本資料」「學生班級資訊」「地址」三個項目，作為對照。

1. 點選同一位學生，確認下列欄位顯示文字：年級（例：3年級）、班級（普303）、科別（有指定時顯示科別名稱，沒有時顯示班級科別的浮水印）、性別。
2. 逐一打開年級、班級、科別、性別、縣市、鄉鎮的下拉清單，確認每一列都有文字，目前的值有反白。
3. **版面檢查**（與修改前的截圖比對）：
   - 基本資料：性別選單與下一列「生日」不重疊、沒有被切掉，兩者之間仍有間距。
   - 學生班級資訊：年級、班級與同一列的座號選單、學號文字框高度相近，與第二列（學號、科別）不重疊。
   - 地址：縣市、鄉鎮與下一列地址文字框不重疊。
   - 整體：項目的高度沒有改變，下方的項目沒有被推開或蓋住。
4. 在班級資訊中把年級改成其他年級，確認班級清單換成該年級的班級且有文字；再按取消或切換學生放棄修改。
5. 家長資訊（關係、職業、學歷、國籍、存歿）的下拉選單外觀與修改前相同（列高原本就是 18，不應改變）。
6. 用 VS 偵錯（F5）啟動時再看一次步驟 1、3，記錄有沒有因為 DPI 改變造成的跑版（`desktop-操作注意事項.md` 已知偵錯時可能被改成 DPI aware；若筆電 150% 縮放下基本資料本來就擠在一起，記錄情況即可，不屬於本次修改造成）。

**預期結果**

- [ ] 1、2：所有欄位與下拉清單都顯示文字，值與修改前（空白時）實際選取的值相同。
- [ ] 3：選單只比修改前高約 2px，沒有任何欄位重疊或被切掉。
- [ ] 4：改年級後班級清單正確，放棄修改後資料沒有變動，系統歷程沒有新紀錄。
- [ ] 5：家長資訊外觀沒有變化。
- [ ] 6：記錄偵錯模式下的版面狀況。

### 測試後還原

- [ ] 3 位測試學生的狀態與英文姓名還原成測試前記錄的值，測試用的異動紀錄已刪除。
- [ ] 學生清單的篩選條件還原成測試前勾選的項目。
- [ ] 待處理清單：移除測試用的學生。

---

## 五、測試結果

### 第一輪（2026/10/21，`demo.h.beibei.lai`，VS 偵錯執行）

- 測試學生：普303 座號 8 宋子（070510）、9 塗子琳（070577）、10 顧小（070588），測試前皆為「一般」；篩選「在校學生」，普303 10 人、所有學生 450。
- 測試前需先刪除 `_ischoolLoader_h\1campus_desktop_x64\modules\SHSchoolLoader\` 中線上更新的 `SmartSchool.Core.General.dll`，否則會載入線上版本而不是 VS 建置的版本（PDB 路徑為 `C:\MyJob_Desktop\Github\SHCore\...`）。

| 案例 | 結果 | 說明 |
|---|---|---|
| T1 | ✅ | 標籤即時變綠、暫時保留；改選其他學生後移除（普303 10→9、所有學生 450→449）；在待處理改回一般後年級樹即時恢復 |
| T2 | ✅ | 休學→一般→延修→一般，每步標籤即時更新，一直留在待處理 |
| T3 | ✅ | 批次休學後標籤即時更新、暫時保留，狀態列「變更學生狀態成功」；改選後移除 |
| T4 | ✅ | 3 位批次休學，只保留右側顯示的學生，另 2 位馬上移除（普303 8 人、所有學生 448）；改選後再移除（普303 7 人） |
| T5 | ✅ | 「刪除」按鈕無錯誤，標籤即時變紅、暫時保留；改選後移除；改回一般後回到普303 |
| T6-1 | ✅ | 同 T1、T3（改選其他學生） |
| T6-2 | ✅ | 點選年級樹普305 後移除（普303 9、所有學生 449） |
| T6-3 | ✅ | 勾選休學生（學籍在校學生）再取消後，依篩選移除 |
| T6-4 | ❌ | 年級樹右鍵「重新整理」後學生仍留在清單（點兩次皆同），改選其他學生才移除。原因見下方 |
| T6-5 | ✅ | 只打開篩選選單再關閉，維持保留 |
| T6-6 | ✅ | 勾選休學生再取消後關閉，維持保留 |
| T7 | ✅ | 1、3 年級展開、2 年級收合下，狀態標籤變更、批次變更、改選、篩選變更後，展開狀態與選取結點皆維持 |
| T8 | ✅（步驟 1） | 英文姓名改為測試值並儲存、再改回，皆正常，學生留在清單，無反覆刷新。步驟 2（異動紀錄）未測：此環境未顯示異動項目 |
| T9 | ✅ | 選相同狀態無反應；選休學後按 No 無變更，系統歷程無新增紀錄 |
| T10 | ✅ | 所有學生（508）→學籍在校學生（472）→在校學生（450）→自訂勾選畢業及離校生（470），標題與清單正確 |
| T11 | ✅ | 待處理批次休學後標籤即時變休學（選單打勾也是休學），改回一般即時更新；3 位批次變更後逐一點選皆正確 |
| T12 | ✅ | 批次休學／刪除後不改選、直接到待處理點回，標籤分別為休學／刪除 |
| T13 | ✅ | 同班、跨年級（商101）、未分年級學生間切換，VS 不再於 `ClassInfoPalmerwormItem.cs` 中斷 |
| T14 | ✅（部分） | 年級、班級、性別正常顯示，下拉清單有文字且反白正確；性別與生日不重疊，年級／班級與座號／學號高度相近；改年級後班級清單、科別浮水印正確，取消後還原。地址、家長資訊未測：此環境未顯示這兩個項目 |

- 系統歷程有對應紀錄：「學生狀態／變更」（標籤）、「系統歷程／變更狀態」（批次）、「系統歷程／刪除學生」、「系統歷程／修改」（英文姓名）。
- 測試期間曾出現伺服器端 `DSAServerException: Get DB Connection Error`（右側各項目背景載入失敗，例外傳到 `Application.Run`），重新啟動 ischool 後未再發生，與本次修改無關。
- 測試後：3 位學生皆為「一般」、英文姓名已還原、已移出待處理，篩選為「在校學生」。

#### T6-4 未通過的原因

年級樹結點右鍵選單的「重新整理」是 `NavigationPlanner/StudentDividerProvider.cs:27` 註冊的，呼叫 `Student.Instance.ReloadData()`；方案第 2 項修改的是 `NavPaneContexMenu` 的「重新整理」（`Student.cs`），兩者不同，`ReloadData()` 沒有清除 `_KeepKeys`。

已修正（2026/10/21）：`ReloadData()` 開頭加上 `_KeepKeys.Clear()`，`NavPaneContexMenu` 的處理還原為原本寫法。

### 第二輪（2026/10/21，重測 T6-4）

| 案例 | 結果 | 說明 |
|---|---|---|
| T6-4 | ✅ | 普303 宋子用狀態標籤改成休學（暫時保留，所有學生 450）→ 年級樹右鍵「重新整理」後依篩選移除（普303 9、所有學生 449），年級樹展開與選取維持 |

- 測試後：宋子已改回「一般」並移出待處理，普303 10 人、所有學生 450。

## 六、尚未處理（不在這次修改範圍）

- 類別模組 `StudentBar.StatusCheckedChanged` 在確認視窗按「No」、學號或身分證號重複被擋下時，選單的勾會與標籤不一致。SHCore 收不到這些情況的通知，要修改需另外處理類別模組並發佈。
- 問題 3 的根本原因在類別模組（標籤不在畫面上就不更新），這次以 SHCore 的保留機制避開，不修改類別模組。
- SHCore 原本被註解掉的 `Palmerworm/StudentDescriptionPane`（`Student.cs:600`）不再使用，這次不處理。
