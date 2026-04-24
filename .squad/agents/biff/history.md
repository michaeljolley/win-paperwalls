# Project Context

- **Owner:** Michael Jolley
- **Project:** win-paperwalls — A Windows service with system tray icon that fetches 4K wallpapers from the @PaperWalls4K X/Twitter account and rotates the desktop background on a configurable schedule.
- **Stack:** C# / .NET, WinUI 3 (settings UI / tray), Windows Service
- **Created:** 2026-04-24

## Learnings

<!-- Append new learnings below. Each entry is something lasting about the project. -->

### 2026-04-24: Phase 1 Foundation Complete

**Solution Structure:**
- `win-paperwalls.sln` - Main solution file at repo root
- `src/WinPaperWalls/` - Unpackaged WinUI 3 app (net9.0-windows10.0.19041.0)
- `tests/WinPaperWalls.Tests/` - xUnit test project (created but not in solution yet)

**Core Components Built:**
1. **App.xaml.cs** - Generic host pattern with `Microsoft.Extensions.Hosting`, single-instance mutex (`WinPaperWalls_SingleInstance`)
2. **Models/** - `AppSettings`, `WallpaperTopic`, `WallpaperImage`
3. **Services/SettingsService** - Thread-safe JSON persistence to `%LOCALAPPDATA%\WinPaperWalls\settings.json`, fires `SettingsChanged` event
4. **Interop/DesktopWallpaper** - P/Invoke wrapper for `SystemParametersInfo`, supports Fill/Fit/Stretch/Tile/Center/Span via registry keys

**DI Container:**
- All services registered via `Host.CreateDefaultBuilder()` in `App.xaml.cs`
- Services: `ISettingsService`, `MainWindow`

**Key Patterns:**
- Generic host for DI and lifetime management
- Lock-based thread safety in SettingsService
- Registry-based wallpaper style configuration before `SPI_SETDESKWALLPAPER` call

**NuGet Packages:**
- `Microsoft.WindowsAppSDK` 1.6.240923002
- `Microsoft.Extensions.Hosting` 9.0.0
- `Microsoft.Extensions.DependencyInjection` 9.0.0
- `H.NotifyIcon.WinUI` 2.1.4 (referenced, not yet wired up)

**Test Project Note:**
- Test project created but excluded from solution due to transitive `WindowsAppSDK` build target conflicts
- TODO: Refactor services into separate class library to enable proper unit testing
- Alternative: Use `Directory.Build.props` to fully exclude WindowsAppSDK targets from test projects

**Build Command:** `dotnet build` at repo root succeeds.

