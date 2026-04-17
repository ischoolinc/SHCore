## 1. Instrumentation and Analysis

- [x] 1.1 Add granular `LogPerf` calls to `Program.cs` methods: `Init_Student_Class_Teacher` and `Init_Core_Others` to track individual RibbonBar and manager setup times.
- [x] 1.2 Add `LogPerf` calls to `Student`, `Class`, and `Teacher` managers' `SetupPresentation` and `SetupDetailItems` methods.
- [x] 1.3 Run the application and analyze the generated `loading_perf_hs.txt` to identify the most significant bottlenecks.

## 2. Optimization Implementation

- [ ] 2.1 Refactor `SetupDetailItems` in `StudentRelated\Student.cs` to defer the instantiation of `IContentItem` objects until they are needed by the `ContentItemBulider`.
- [ ] 2.2 Refactor `SetupDetailItems` in `ClassRelated\Class.cs` to use the same lazy instantiation pattern.
- [ ] 2.3 Refactor `SetupDetailItems` in `TeacherRelated\Teacher.cs` to use the same lazy instantiation pattern.
- [ ] 2.4 Optimize the `Application.Idle` event handler in `Program.cs` by potentially splitting sync tasks or adjusting their priority.

## 3. Validation

- [ ] 3.1 Use the enhanced `LogPerf` telemetry to compare startup durations before and after the optimizations.
- [ ] 3.2 Perform manual UI responsiveness tests during the background loading phase to ensure no stutters occur.
- [ ] 3.3 Verify that all UI extensions (RibbonBar buttons and detailed info panels) function correctly and load data on demand.
