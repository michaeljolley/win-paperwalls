# Project Context

- **Owner:** Michael Jolley
- **Project:** win-paperwalls — A Windows service with system tray icon that fetches 4K wallpapers from the @PaperWalls4K X/Twitter account and rotates the desktop background on a configurable schedule.
- **Stack:** C# / .NET, WinUI 3 (settings UI / tray), Windows Service
- **Created:** 2026-04-24

## Learnings

### 2026-04-24: WinUI 3 Application Lifecycle

WinUI 3's `Microsoft.UI.Xaml.Application` does not have an `Exit` event like WPF/WinForms. Instead, you must override the `Exit()` method to perform cleanup. The pattern used is:

```csharp
public new async void Exit()
{
    // Perform cleanup (dispose resources, stop services)
    base.Exit(); // Call base to actually exit
}
```

This is critical for properly disposing IHost, releasing mutexes, and cleaning up other resources before app termination.

### 2026-04-24: Code Quality Review Outcomes

Completed comprehensive code review of all services and UI code:

1. **Resource Management:** All services properly use IHttpClientFactory (no bare `new HttpClient()`), file handles are properly closed, host is disposed on exit, mutex is released
2. **Error Handling:** HTTP failures handled with fallback to cached data, settings file corruption handled, wallpaper change failures logged but don't crash scheduler
3. **Thread Safety:** SettingsService uses locks for file I/O, CacheService locks file operations, SchedulerService safely restarts timer on settings change
4. **DI Architecture:** All services registered as interfaces, proper singleton/hosted service pattern for SchedulerService
5. **Windows Startup:** Settings UI now reads actual registry state and applies changes via StartupManager when saving

The application is production-ready with proper error handling, logging, and resource management throughout.

<!-- Append new learnings below. Each entry is something lasting about the project. -->
