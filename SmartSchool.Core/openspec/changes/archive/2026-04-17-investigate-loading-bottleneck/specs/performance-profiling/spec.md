## ADDED Requirements

### Requirement: Granular Performance Logging
The system SHALL provide a mechanism to log execution time of specific code blocks during the startup sequence and form loading.

#### Scenario: Log execution time of Init_System
- **WHEN** the `Init_System` method starts and ends
- **THEN** the system SHALL record the start time, end time, and duration in the performance log file (`loading_perf_hs.txt`).

### Requirement: Performance Summary Reporting
The system SHALL be able to generate a summary of performance logs identifying the top 3 slowest operations.

#### Scenario: Identify slow operations
- **WHEN** the application finishes loading the main form
- **THEN** the system SHALL analyze the performance log and output a summary of the most time-consuming initialization steps.
