## 1. Audit and Refactor Initialization Logic

- [x] 1.1 Review `SmartSchool.Core.General.Program.cs` (`Init_Student_Class_Teacher_Load` and `Init_Student_Class_Teacher`) to remove any calls that implicitly trigger synchronous access to `Items` across the singleton components.
- [x] 1.2 Inspect `Student.cs` constructor and initialization methods. Find any instances where `Student.Instance.Items` is accessed directly.
- [x] 1.3 Inspect `Class.cs` constructor and initialization methods. Find any instances where `Class.Instance.Items` is accessed directly.
- [x] 1.4 Inspect `Teacher.cs` constructor and initialization methods. Find any instances where `Teacher.Instance.Items` is accessed directly.

## 2. Decouple Initial Rendering from Data

- [x] 2.1 Update `Student` views (e.g., list pane, cross-reference columns like `Class`) to check the `Loaded` property. Return placeholders (e.g., "Loading...") when `Loaded` is false.
- [x] 2.2 Update `Class` views (e.g., list pane, cross-reference columns like `TeacherUniqName` or `studentCountField`) to check the `Loaded` property. Return placeholders when `Loaded` is false.
- [x] 2.3 Update `Teacher` views to gracefully handle a false `Loaded` state without invoking `Items`.
- [x] 2.4 Set the initial `ShowLoading` state to `true` on the `NLDPanels` (e.g., `K12.Presentation.NLDPanels.Student.ShowLoading = true;`) during initialization instead of forcing a load.

## 3. Implement Event-Driven Loading Architecture

- [x] 3.1 Hook into the `ItemLoaded` event for `Student.Instance` to trigger `.RefillListPane()` and invoke rendering updates *only* when data is successfully populated in the background. Ensure thread safety if FISCA requires explicit marshalling.
- [x] 3.2 Hook into the `ItemLoaded` event for `Class.Instance` to trigger `.RefillListPane()` and invoke cross-reference updates for `Student` and `Teacher` dependent columns.
- [x] 3.3 Hook into the `ItemLoaded` event for `Teacher.Instance` to trigger `.RefillListPane()` and invoke cross-reference updates for `Class` dependent columns.

## 4. Testing and Verification

- [x] 4.1 Build and run the `SHCore` application. Verify the `MotherForm` opens immediately.
- [x] 4.2 Validate that the Student, Class, and Teacher tabs correctly display "Loading..." states initially.
- [x] 4.3 Validate that upon the completion of background threads, the "Loading..." states disappear and data successfully populates across the tabs.
- [x] 4.4 Verify no cross-thread operation exceptions occur during `ItemLoaded` UI updates.
