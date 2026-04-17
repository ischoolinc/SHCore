## Context

The `SmartSchool.Core.General` module is a core part of the 1Campus desktop application. It manages the primary entities: Students, Classes, and Teachers. During startup, it performs heavy initialization, including fetching abstract data lists for all these entities and setting up complex UI components (RibbonBars and Palmerworm items). The current implementation uses synchronous-style calls wrapped in background tasks, which can still block or stutter the UI thread. Reflection is heavily used to discover and load extension items.

## Goals / Non-Goals

**Goals:**
- Identify the exact methods and calls responsible for slow loading.
- Reduce the perceived and actual startup time of the module.
- Ensure the UI remains responsive while background data synchronization is in progress.
- Provide a clear performance log for future maintenance.

**Non-Goals:**
- Refactoring the entire `K12.Data` synchronization architecture.
- Replacing the underlying network protocol or communication library.
- Modifying the visual design of the UI.

## Decisions

### 1. Granular Telemetry in Startup Path
- **Decision**: Inject `LogPerf` calls into every major step of `Init_Student_Class_Teacher` and `Init_Core_Others`.
- **Rationale**: Existing logs are too coarse. We need to know which specific RibbonBar or which specific manager initialization is the slowest.
- **Alternative**: Using a commercial profiler, but `LogPerf` provides a lightweight, built-in solution that works in the user's environment.

### 2. Lazy Instantiation of Palmerworm Items
- **Decision**: Refactor `SetupDetailItems` to avoid instantiating `IContentItem` objects during the initial setup. Instead, pass the `Type` or a factory to `ContentItemBulider`.
- **Rationale**: Currently, `SetupDetailItems` calls constructors for all registered items. If these constructors perform UI initialization or data fetching, it multiplies the startup delay.
- **Alternative**: Keep existing logic but optimize individual constructors. This is less scalable as more plugins are added.

### 3. Review SyncAllBackground Impact
- **Decision**: Analyze if `SyncAllBackground` calls can be prioritized or if their completion events trigger heavy UI updates that block the main thread.
- **Rationale**: Moving these to `Application.Idle` was a good first step, but they might still be competing for resources or causing massive UI invalidations.
- **Alternative**: Completely remove `SyncAllBackground` from startup, but this would lead to "empty" panels until the user manually refreshes.

## Risks / Trade-offs

- **[Risk]** Refactoring `SetupDetailItems` might break existing plugins that expect to be instantiated at startup.  **Mitigation**: Perform a thorough audit of common `IContentItem` implementations to ensure they don't rely on startup-time instantiation.
- **[Risk]** Increased logging might slightly increase startup time itself.  **Mitigation**: Ensure `LogPerf` is efficient (it already uses a simple `File.AppendAllText` but should be careful about file locks).
- **[Risk]** Background synchronization might finish *after* the user starts interacting, leading to visible data pops.  **Mitigation**: This is an acceptable trade-off for a faster initial UI appearance.
