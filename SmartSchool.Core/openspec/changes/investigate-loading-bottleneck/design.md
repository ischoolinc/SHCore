## Context

The application's startup process is serial and blocking. It involves reading multiple XML configurations, connecting to DSA services, and initializing a complex ribbon-based UI. As the number of plugins and data volume grows, the time to reach a ready state has increased significantly.

## Goals / Non-Goals

**Goals:**
- Identify exact methods and blocks causing delays.
- Reduce blocking of the UI thread during startup and data fetching.
- Minimize the initial memory and CPU footprint by deferring plugin loading.

**Non-Goals:**
- Complete rewrite of the UI framework.
- Replacing the DSA communication protocol.
- Optimizing database-side query performance (out of scope for this client-side investigation).

## Decisions

### 1. Unified Profiling Utility
**Decision**: Create a `PerformanceTracker` utility that wraps `Stopwatch` and integrates with the existing `Core_Program.LogPerf`.
**Rationale**: `DateTime.Now` in `LogPerf` lacks the resolution needed for micro-benchmarking. `Stopwatch` provides higher precision.
**Alternatives**: Using an external profiler (rejected due to the need for continuous monitoring in production environments).

### 2. Task-Based Asynchrony for DSA Calls
**Decision**: Wrap `FISCA.DSAClient` calls in `Task.Run` and use `Progress<T>` for UI updates.
**Rationale**: WinForms UI thread must remain responsive to handle paint events and user interaction. Tasks are the idiomatic way to handle background work in modern .NET.
**Alternatives**: `BackgroundWorker` (rejected as it is legacy), `BeginInvoke`/`EndInvoke` (rejected as Tasks are more composable).

### 3. Virtualized Plugin Manager
**Decision**: Modify `ButtonAdapterPlugInManager` to store plugin metadata without instantiating the plugin or its UI components until the containing ribbon tab is activated.
**Rationale**: Many plugins are rarely used in a single session. Loading them all at once is wasteful.
**Alternatives**: Loading plugins in a low-priority background thread (rejected due to potential race conditions with UI component creation).

## Risks / Trade-offs

- [Risk] Asynchronous operations may lead to race conditions if multiple components depend on the same shared state.
  - Mitigation: Use thread-safe collections and ensure state transitions are managed on the UI thread.
- [Risk] Lazy loading might cause a perceived "lag" when first switching to a tab.
  - Mitigation: Pre-fetch plugin metadata and use lightweight placeholders.
- [Risk] Performance logging may slightly increase startup time itself.
  - Mitigation: Ensure logging is lightweight and can be disabled via configuration.
