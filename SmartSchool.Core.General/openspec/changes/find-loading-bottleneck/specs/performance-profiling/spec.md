## ADDED Requirements

### Requirement: Comprehensive Startup Telemetry
The system SHALL record high-resolution timestamps for each major stage of the application startup process. This includes the beginning and end of student, class, and teacher manager initialization, as well as the registration of each RibbonBar and Palmerworm builder.

#### Scenario: Verify timestamp recording
- **WHEN** the application starts up
- **THEN** a log file named `loading_perf_hs.txt` is created or appended with entries containing HH:mm:ss.fff format and a description of the stage completed.

### Requirement: Bottleneck Reporting
The system SHALL provide a mechanism to summarize the most time-consuming operations observed during startup to facilitate quick identification of bottlenecks.

#### Scenario: Identify slowest initialization stage
- **WHEN** the developers review the `loading_perf_hs.txt` log
- **THEN** the duration between consecutive log entries reveals which specific `SetupPresentation` or `SetupDetailItems` call took the longest.
