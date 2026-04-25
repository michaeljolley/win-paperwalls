# Decisions

### 2026-04-24T03:02:00Z: Image source
**By:** Michael Jolley (via Doc)
**What:** Use burkeholland/paper GitHub repo instead of @PaperWalls4K X/Twitter feed as the image source. 31 topic folders with 4K JPEGs, public API, no auth required.
**Why:** X API requires authentication and has strict rate limits. GitHub API is free for public repos (60 req/hr unauthenticated), images are organized by topic, and new topics auto-appear.

### 2026-04-24T03:02:00Z: Architecture — single WinUI 3 app with BackgroundService
**By:** Doc
**What:** Build as a single WinUI 3 desktop app with BackgroundService, not a true Windows service. Windows services run in session 0 with no UI access — tray icons require the user's desktop session.
**Why:** Avoids IPC complexity between a service and a tray app. Single process handles both background rotation and tray UI.

### 2026-04-24T03:02:00Z: UI framework — WinUI 3
**By:** Michael Jolley
**What:** Use WinUI 3 for all UI (settings window, tray integration via H.NotifyIcon.WinUI). NOT WPF or WinForms.
**Why:** User preference. Modern Windows UI framework.

### 2026-07-14: Native AOT — LoggerMessage source generator pattern
**By:** Biff (Backend Dev)
**What:** All logging must use `[LoggerMessage]` source generator pattern. Classes must be `partial`. Event IDs assigned by service/component range (1000-1099: CacheService, 2000-2099: GitHubImageService, 3000-3099: SchedulerService, 4000-4099: WallpaperService, 5000-5099: StartupManager, 6000-6099: LogBundleService, 7000-7099: SettingsViewModel).
**Why:** Native AOT compilation (`PublishAot` enabled) is incompatible with reflection-based logging. LoggerMessage generates compile-time methods, enabling zero-allocation, high-performance logging with full Native AOT support.
**Status:** Implemented — all 7 services converted, 48 tests passing.

### 2026-07-14: MSIX publish fix in release workflow
**By:** Biff (Backend Dev)
**What:** Pass `/p:GenerateAppxPackageOnBuild=true` and `/p:AppxPackageDir=<per-platform-dir>` explicitly on every `dotnet publish` command. Use `/p:Platform=x64` consistently instead of RuntimeIdentifier `-r win-x64`. Separate sideload (`AppPackages/`) and Store (`AppPackages-Store/`) output directories per platform.
**Why:** `dotnet publish` ignores publish profiles — MSBuild properties must be passed via `/p:` flags. Per-platform output dirs prevent x64/ARM64 collision. Consistent `/p:Platform=` usage aligns build-and-test with package job.
**Status:** Implemented — commit 5034645, v1.0.0 release recreated.

### 2026-04-25T17:02:13Z: Namespace, .NET version, WindowsAppSDK stability
**By:** Michael Jolley (via Copilot)
**What:** 1. Namespace is `PaperWalls` (not `BaldBeardedBuilder.PaperWalls`). 2. Use .NET 10 for all projects. 3. Do not use preview versions of WindowsAppSDK.
**Why:** User preference — consistency, stability, and modern platform versions.
