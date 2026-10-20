# [客服部] 學生資料項目變更狀態後，學生清單未即時更新 — 修改方案與測試流程

- Basecamp 待辦：[系統功能修改/新增 事由[客服部]學生資料項目進行狀態變更，清單無即時更新，需重新整理操作問題](https://app.basecamp.com/4399967/buckets/15765350/todos/10421515849)（ID `10421515849`）
- 來源：客服部 2026/10/20 回報。左下角篩選沒有勾選某個狀態時（例：只勾「在校學生」），在右側學生資料上方的狀態標籤把學生改成該狀態（例：「畢業或離校」），中間的學生清單沒有即時更新，需要手動重新整理。
- 參考：JHCore 已修正相同問題，見 `JHCore/JHSchool/docs/updateSpec_學生狀態變更後畫面同步_20261014.md`（commit `43db72a`，協同中學待辦 `9999684572`）。本方案依 JHCore 的做法調整成 SHCore 的寫法。
- 測試環境：`demo.h.beibei.lai`／從 Visual Studio 啟動修改後的 SHCore
- 版本：待定（目前 `SmartSchool.Core.General` 為 1.0.9.50）
- 狀態：**方案，尚未修改程式**

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

> 類別模組的分析參考 `ischool-desktop/MOD_Tagging`（組織舊帳號，最後更新 2017），現行版本可能已有調整。SHCore 的問題 1 已在待處理檢視實測確認；問題 3 的行為 JHCore 已在實際環境確認，SHCore 要在測試環境重現後再確認。

---

## 三、修改方案

### 修改的檔案

| 檔案 | 編碼 | 修改 |
|---|---|---|
| `SmartSchool.Core.General/StudentRelated/Student.cs` | UTF-8 BOM | 問題 2、3 |
| `SmartSchool.Core.General/StudentRelated/ChangeStatusBatch.cs` | UTF-8 BOM | 問題 1、3 |
| `SmartSchool.Core.General/StudentRelated/Process/StudentIUD/StudentIDUProcess.cs` | UTF-8 BOM | 問題 1、3 |

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
| 右鍵「重新整理」（`Student.cs:594`） | 呼叫 `SyncAllBackground()` 前先 `_KeepKeys.Clear()` |

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

### 不需修改的部分

- **年級樹（JHCore 問題 4）**：SHCore 的 `ClassDivider.Divide()` 沿用舊的結點物件，選取結點用物件比對，不受人數文字改變影響；展開狀態需實測確認（T7）。若實測會收合，再另外修改 `ClassDivider.cs`。注意 `ClassDivider.cs` 是 **Big5** 編碼，修改時要保持原編碼或另外確認轉檔。
- **「刪除」按鈕身分證號檢查（JHCore 問題 5）**：SHCore 的 `StudentIDUProcess.cs:125-148` 學號、身分證號分別用各自的清單比對，沒有 JHCore 的誤用問題。

### 使用上的改變（回覆客服時需說明）

- 從狀態標籤變更後，學生清單與年級樹會依篩選即時更新，不需要手動重新整理。
- 右鍵批次變更或上方「刪除」後，右側正在顯示的學生的狀態標籤會即時更新，不需要切換學生。
- 狀態變更後，右側正在顯示的學生會**暫時留在清單**（即使已不符合篩選），年級樹人數也暫時把他算在內；改選其他學生、點選年級樹結點、變更篩選勾選或重新整理後，才依篩選移除、人數更新。只打開篩選選單、沒有改變勾選就關閉時，學生仍會保留。
- 批次變更時，只有右側正在顯示的那一位暫時保留，其他一起選取的學生在變更完成後馬上依篩選移除。
- 上方「刪除」按鈕按下後，學生不會像修改前一樣馬上從清單消失，標籤會顯示「刪除」。

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

- [ ] 用修改前的版本照客服回報的步驟操作，確認從狀態標籤改成「畢業或離校」後，學生仍留在清單中。
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

### 測試後還原

- [ ] 3 位測試學生的狀態與英文姓名還原成測試前記錄的值，測試用的異動紀錄已刪除。
- [ ] 學生清單的篩選條件還原成測試前勾選的項目。
- [ ] 待處理清單：移除測試用的學生。

---

## 五、尚未處理（不在這次修改範圍）

- 類別模組 `StudentBar.StatusCheckedChanged` 在確認視窗按「No」、學號或身分證號重複被擋下時，選單的勾會與標籤不一致。SHCore 收不到這些情況的通知，要修改需另外處理類別模組並發佈。
- 問題 3 的根本原因在類別模組（標籤不在畫面上就不更新），這次以 SHCore 的保留機制避開，不修改類別模組。
- SHCore 原本被註解掉的 `Palmerworm/StudentDescriptionPane`（`Student.cs:600`）不再使用，這次不處理。
