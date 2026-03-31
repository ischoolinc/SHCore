## Why

The SHCore application currently experiences severe slow loading times during startup. This is caused by a race condition/deadlock pattern in the initialization phase where background synchronization threads (e.g., `Class.Instance.SyncAllBackground()`) are launched, but the main UI thread almost immediately attempts to access the data (`Items`) while constructing the initial views. Since the data access is protected by an `AutoResetEvent` (`_Loading`), the main thread blocks synchronously until the background XML parsing completes. This sequential blocking makes the startup process feel like a slow, synchronous data queue, delaying the appearance of the `MotherForm`.

## What Changes

- Decouple the UI initialization of modules (Student, Class, Teacher) from their initial data loading.
- Initialize the UI components (like `NLDPanels` and DataGridViews) with empty states rather than immediately requesting data that triggers a blocking wait.
- Refactor the startup sequence so that the `MotherForm` is rendered and displayed to the user before or while the background threads (`SyncAllBackground`) fetch and process the XML payloads.
- Update event handlers so that UI grids and panels are populated reactively via the `ItemLoaded` event rather than synchronously during form construction.

## Capabilities

### New Capabilities

- `startup-sequence`: Managing the asynchronous initialization and rendering of the main application shell and its plugins without blocking the main UI thread.

### Modified Capabilities

- `entity-caching`: The `CacheManager<T>` data retrieval behavior during early application lifecycle needs to be observed, specifically how modules access `.Items` during UI construction.

## Impact

- **Affected Code**: `SmartSchool.Core.General.Program.cs` (initialization routines), `SmartSchool.Core.General\StudentRelated\Student.cs`, `SmartSchool.Core.General\ClassRelated\Class.cs`, `SmartSchool.Core.General\TeacherRelated\Teacher.cs`.
- **UI Impact**: The main application shell will appear much faster, showing "Loading..." or empty states initially, and populating data dynamically as the background threads complete.
- **Risk**: Potential null reference exceptions or data inconsistency in UI components if they assume data is immediately available upon instantiation. Careful handling of the `ItemLoaded` events and empty states is required.