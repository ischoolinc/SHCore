## ADDED Requirements

### Requirement: Asynchronous Login
The system SHALL perform the login operation (authentication and initial configuration fetching) asynchronously to prevent the UI from freezing.

#### Scenario: Login progress feedback
- **WHEN** the user initiates a login
- **THEN** the system SHALL display a progress indicator and remain responsive while waiting for the DSA service response.

### Requirement: Non-blocking School Information Retrieval
The system SHALL fetch school information and configurations in the background.

#### Scenario: Background data loading
- **WHEN** the main form or school info management form is opened
- **THEN** the system SHALL start data retrieval in a background thread and update the UI once the data is available.
