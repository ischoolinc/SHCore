## Context

Currently, the SHCore application suffers from severe blocking during startup. While the initialization explicitly creates background threads to fetch module data via `Class.Instance.SyncAllBackground()`, `Student.Instance.SyncAllBackground()`, etc., the main UI thread continues on to build the initial views (the `NLDPanels`). During this view construction, the code accesses the `Items` property of these singleton instances (e.g., to populate a DataGridView with data).

Because these instances are derived from `CacheManager<T>`, accessing the `Items` property triggers a wait on an `AutoResetEvent` named `_Loading`. The main UI thread completely halts, effectively putting the application into a synchronous queue, destroying the asynchronous benefits, and causing the `MotherForm` to delay its initial render.

## Goals / Non-Goals

**Goals:**
- Eliminate the main UI thread lock during startup.
- Decouple the initial presentation of `NLDPanels` (Student, Teacher, Class) from the synchronous fetching of their underlying data.
- Ensure the `MotherForm` opens swiftly and renders an "empty/loading" state while background threads process data.
- Populate the views reactively once the `ItemLoaded` events fire.

**Non-Goals:**
- Completely rewriting the `CacheManager<T>` class or replacing the Data Service Architecture (DSA).
- Changing the backend data structure or API payloads.
- Implementing complex virtualization in the DataGridView if reactive loading resolves the perceived startup hang.

## Decisions

1. **Lazy Loading UI Initialization:** 
   - *Decision*: Modify `Student.cs`, `Class.cs`, and `Teacher.cs` constructors (and their setup methods) so they *never* access `Items` before the initial data load finishes.
   - *Rationale*: Accessing `Items` invokes the `_Loading.WaitOne()` lock. By preventing this access, the main thread can breeze past module initialization and execute `Application.Run(MotherForm.Instance)`.
2. **Event-Driven UI Population:**
   - *Decision*: Move the logic that populates the DataGridView or relies on cross-module data into the `ItemLoaded` event handlers.
   - *Rationale*: Ensures the UI thread is only updating views when the data is fully ready. The UI thread handles the rendering natively via standard event loops.
3. **Empty/Loading States:**
   - *Decision*: Use existing indicators (like `ShowLoading = true`) within the `NLDPanels` to signal to the user that data is being fetched.
   - *Rationale*: Provides immediate visual feedback to the user upon clicking the executable, improving perceived performance.
4. **Cross-Module Dependency Handling:**
   - *Decision*: If Module A (e.g., Student) needs Module B (e.g., Class) data for rendering a specific column, the rendering logic should fall back to a placeholder ("Loading...") if Module B is not yet `Loaded`. Once Module B fires `ItemLoaded`, the specific column/view in Module A is refreshed.
   - *Rationale*: Prevents secondary deadlocks where Module A finishes, tries to render, but blocks waiting for Module B.

## Risks / Trade-offs

- **Risk:** Unintentional UI thread manipulation from a background thread. `ItemLoaded` is fired from a background worker thread.
  - *Mitigation:* We must ensure that any UI-manipulating logic inside `ItemLoaded` handlers is properly marshalled to the UI thread using `Control.Invoke` or `Control.BeginInvoke`, or that the underlying presentation framework (`FISCA.Presentation`) handles this marshalling internally.
- **Risk:** NullReferenceExceptions if legacy code assumes `Items` is fully populated immediately after the singleton is accessed.
  - *Mitigation:* Audit usages within the `Init_` routines to ensure all synchronous accesses are converted to reactive patterns or wrapped in a `Loaded` check.