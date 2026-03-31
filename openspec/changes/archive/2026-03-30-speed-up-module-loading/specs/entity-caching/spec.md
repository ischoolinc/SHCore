## ADDED Requirements

### Requirement: Non-blocking data property access during initialization
Classes that manage cached entities (Student, Teacher, Class) MUST NOT access the `Items` property synchronously during their constructor or setup phase before the initial data loading is complete.

#### Scenario: Module initialization during application startup
- **WHEN** the `Core_General_Program` initializes the module components by calling their `SetupPresentation` or constructor methods
- **THEN** the module initialization proceeds without invoking `_Loading.WaitOne()` (via the `Items` getter), bypassing synchronous blockage.

### Requirement: Cross-module view dependencies
When an entity's UI view depends on another entity's data (e.g., a Student's view referencing Class information), it MUST display a placeholder if the dependent module is not yet loaded (`Loaded == false`).

#### Scenario: Dependent module is still loading
- **WHEN** the `Student` module renders its list pane and the `Class` module has not yet fired `ItemLoaded`
- **THEN** the Class column in the Student list displays a "Loading..." placeholder or remains empty, and it dynamically refreshes once the `Class` module completes its load.