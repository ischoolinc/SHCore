## 1. Performance Profiling Setup

- [x] 1.1 Implement `PerformanceTracker` utility in `Program.cs` or a new utility class.
- [x] 1.2 Instrument `Init_System` in `Program.cs` with granular timing logs.
- [x] 1.3 Instrument `MotherForm_Load` and `UpdatePreference` in `MotherForm.cs`.
- [x] 1.4 Add a summary report generator that outputs to `loading_perf_hs.txt` on application startup completion.

## 2. Asynchronous Data Access

- [x] 2.1 Refactor `LoginForm.cs` to use `Task` for `DSAServices.Login` and license loading.
- [x] 2.2 Add a loading indicator to `LoginForm` during the async login process.
- [x] 2.3 Refactor `SchoolInfoMangement.cs` to fetch school data in a background task.
- [x] 2.4 Ensure all UI updates in `LoginForm` and `SchoolInfoMangement` are marshaled back to the UI thread correctly.

## 3. Lazy Plugin Initialization

- [x] 3.1 Modify `ButtonAdapterPlugInManager` to support deferred registration.
- [x] 3.2 Update `ButtonItemPlugInManager` to store metadata instead of full objects where possible.
- [x] 3.3 Implement an event handler for ribbon tab selection to trigger lazy loading of tab content.
- [x] 3.4 Verify that critical plugins are still loaded immediately.

## 4. Verification and Optimization

- [x] 4.1 Compare `loading_perf_hs.txt` before and after changes.
- [x] 4.2 Validate that the UI remains responsive (no "Not Responding" title bar) during the entire startup sequence.
- [x] 4.3 Run smoke tests on all affected forms (Login, Main, School Info).
