## ADDED Requirements

### Requirement: Non-blocking Data Synchronization
The system SHALL ensure that background data synchronization for students, classes, and teachers does not cause perceptible UI freezing or "stuttering" during the initial application load.

#### Scenario: Smooth UI during background sync
- **WHEN** `SyncAllBackground` is triggered on `Application.Idle`
- **THEN** the UI remains responsive to user input (e.g., switching tabs) while data is being fetched and processed in the background.

### Requirement: Lazy Initialization of UI Components
The system SHALL defer the instantiation of heavy UI components, such as detailed Palmerworm items, until they are explicitly required for display (e.g., when a specific student is selected).

#### Scenario: Reduced startup memory and time
- **WHEN** the application initializes RibbonBars and builders
- **THEN** only the minimal necessary metadata is loaded, and actual complex UserControls are not instantiated until their first use.
