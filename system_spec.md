# SmartSchool System Specification

## 1. System Overview
SmartSchool is a modular school management system built on the **.NET Framework (v4.8)** using **Windows Forms** for the user interface. It leverages the **FISCA** framework for infrastructure concerns such as authentication, authorization, UI hosting (RibbonBars), and data connectivity. The system is designed to manage core school entities like Students, Classes, and Teachers, along with various administrative functions.

## 2. Architecture
The application follows a modular architecture where functionality is divided into loosely coupled libraries. It appears to utilize a layered approach:

-   **Presentation Layer**: Windows Forms controls, RibbonBars, and "Palmerworm" items (possibly a term for tabbed or detailed views).
-   **Business Logic Layer**: Managers and Entities (e.g., `Student.Instance`, `Class.Instance`) that encapsulate business rules and synchronization logic.
-   **Data Access Layer**: Providers (e.g., `API.Provider.StudentProvider`) and DAO patterns to interact with the backend (likely via DSA/FISCA services).

### Key Modules
-   **SmartSchool.Core**: The core shell of the application. It handles system initialization, main menu (RibbonBar) configuration, user authentication (`LoginForm`), and system-wide settings (School Info, Security).
-   **SmartSchool.Core.General**: Contains the general business logic and UI for the primary entities:
    -   **StudentRelated**: Management of student data, attendance, discipline, and reports.
    -   **ClassRelated**: Class management, grading, and reports.
    -   **TeacherRelated**: Teacher profiles and management.
    -   **Others**: Configuration and utility functions.

## 3. Technology Stack
-   **Language**: C#
-   **Framework**: .NET Framework 4.8
-   **UI Framework**: Windows Forms (WinForms), possibly using DevComponents DotNetBar for modern UI elements.
-   **Key Libraries**:
    -   **FISCA**: (Foundation for ISchool Cloud Application) Provides core services like Authentication, Permission, Presentation, and Data connectivity.
    -   **Aspose**: Used for report generation (Word/Excel/BarCode).
    -   **K12.Data**: Likely a shared data library for K12 domain objects.

## 4. Key Mechanisms

### Initialization
The system initialization is split across modules:
-   **SmartSchool.Core.Program.Init_System()**: Sets up system providers, basic menu items (School Info, Security), and registers features.
-   **SmartSchool.Core.General.Program**:
    -   `Init_Student_Class_Teacher_Load()`: Configures background synchronization for entities.
    -   `Init_Student_Class_Teacher()`: Sets up the UI presentation and providers for Student, Class, and Teacher entities.
    -   `Init_Core_Others()`: Configures general settings and lookup tables (e.g., Department, Degree, Morality mappings).

### Dependency Injection / Service Locator
-   The system seems to use a form of manual dependency injection or service locator pattern, evident in `Customization.Data.AccessHelper.SetStudentProvider(...)`.
-   `Singleton` pattern is heavily used for Managers (e.g., `Student.Instance`).

### Data Synchronization
-   Entities like `Student`, `Class`, and `Teacher` have `SyncAllBackground()` methods, suggesting an asynchronous data loading and caching mechanism to keep the client UI responsive.

## 5. Directory Structure Overview
-   `SmartSchool.Core/`: Core infrastructure and system-level features.
-   `SmartSchool.Core.General/`: Main business features (Student, Class, Teacher).
-   `SHSchoolLoader/`: Likely the bootstrapper or executable host.
-   `SmartSchool.Common/`: Shared utilities and common code.

## 6. External Dependencies
-   **FISCA.DSAClient**: Communication with Data Service Access (DSA) servers.
-   **FISCA.Permission**: Role-based access control (RBAC).
-   **DevComponents**: Third-party UI suite.
