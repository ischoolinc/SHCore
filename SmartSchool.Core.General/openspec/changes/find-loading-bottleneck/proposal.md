## Why

The application's startup process is currently slow, leading to a suboptimal user experience. Users have reported significant delays when the application initializes, particularly during the loading of student, class, and teacher data. Finding and addressing these bottlenecks is critical for improving productivity and responsiveness.

## What Changes

- **Enhanced Performance Logging**: Add more granular telemetry to `Program.cs` and core managers to pinpoint exact time-consuming operations.
- **Initialization Sequence Profiling**: Analyze the `Init_Student_Class_Teacher` and `Init_Core_Others` methods to identify synchronous blockers.
- **Data Fetching Optimization**: Evaluate the efficiency of `GetAbstractList` calls and object instantiation in `Student`, `Class`, and `Teacher` managers.
- **UI Component Deferral**: Investigate if reflection-based loading of RibbonBars and Palmerworm items can be deferred or optimized.
- **Resource Competition Analysis**: Ensure background data synchronization does not excessively compete with the UI thread during startup.

## Capabilities

### New Capabilities
- `performance-profiling`: Tools and logs to measure and report execution time of critical startup paths.
- `startup-optimization`: A set of optimizations to reduce application initialization time, potentially through lazy loading or parallelization.

### Modified Capabilities
- None

## Impact

- **Affected Code**: `Program.cs`, `StudentRelated\Student.cs`, `ClassRelated\Class.cs`, `TeacherRelated\Teacher.cs`, and various UI initialization points.
- **APIs**: Potentially `SmartSchool.Feature.QueryStudent` and related service calls.
- **Dependencies**: No new external dependencies are expected, but internal library usage may be optimized.
