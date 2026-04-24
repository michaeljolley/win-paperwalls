# Project Context

- **Owner:** Michael Jolley
- **Project:** win-paperwalls — A Windows service with system tray icon that fetches 4K wallpapers from the @PaperWalls4K X/Twitter account and rotates the desktop background on a configurable schedule.
- **Stack:** C# / .NET, WinUI 3 (settings UI / tray), Windows Service
- **Created:** 2026-04-24

## Learnings

<!-- Append new learnings below. Each entry is something lasting about the project. -->

### 2026-04-24 - Phase 3: System Tray Icon Implementation

**What was built:**
- System tray icon using H.NotifyIcon.WinUI (v2.1.4) package
- Tray context menu with Next Wallpaper, Settings, and Exit options
- Modified app startup to launch minimized to tray (no visible window)
- MainWindow hides instead of closing when user clicks X
- P/Invoke for User32.ShowWindow to properly hide windows
- Placeholder tray-icon.ico created (16x16, minimal)

**Key decisions:**
- TrayIconView created as a UserControl (separate from App.xaml) for cleaner separation
- Used H.NotifyIcon.WinUI's TaskbarIcon component with ContextFlyout for menu
- ICommand implementation for double-click to open settings
- MainWindow registered as singleton in DI, created on-demand when Settings is clicked
- Exit properly calls Application.Current.Exit() to terminate the app completely

**Integration notes:**
- Biff had already created IWallpaperService, IGitHubImageService, ICacheService with full implementations
- Added Microsoft.Extensions.Http using statements to CacheService and GitHubImageService (they use IHttpClientFactory)
- App.xaml.cs already had HttpClient registration and all service DI setup from Biff's work
- WallpaperService.ChangeWallpaperAsync() is fully implemented and can be called from tray menu

**Cross-Agent Notes:**
- Biff (Phase 2) built GitHubImageService, CacheService, and WallpaperService with full implementations and proper DI registration
- All services exposed via well-defined interfaces: IGitHubImageService, ICacheService, IWallpaperService
- App.xaml.cs DI setup from Phase 2 already includes IHttpClientFactory and all service registrations
- Marty (Phase 3) did not need to modify any of Biff's services—tray menu integration was clean

**Next Steps:**
- Phase 4: Scheduler service for automatic wallpaper rotation based on `IntervalMinutes`
- Consider: Unit tests once test project architecture is resolved (see decisions.md)
- Consider: Toast notifications for wallpaper change feedback

