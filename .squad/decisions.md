# Squad Decisions

## Active Decisions

### 2026-04-24: Test Project Architecture

**Status:** Under Review  
**Author:** Biff  

The test project (`tests/WinPaperWalls.Tests/`) encounters build conflicts when referencing the main WinUI 3 project. The WindowsAppSDK build targets expect Visual Studio build tools that aren't available in the .NET SDK alone (specifically MrtCore.PriGen.targets for PRI resource generation).

**Options:**
1. **Recommended:** Extract services into a separate class library (`src/WinPaperWalls.Core/`) for models and services; main WinUI app references Core, test project references Core only
2. **Alternative:** Keep test project excluded from solution build; manual testing only until refactoring

**Decision pending:** Should we refactor to Option 1 now or defer until more services are implemented?

### 2026-04-24: Image Source

**Status:** Decided  
**Author:** Michael Jolley (via Doc)

Use `burkeholland/paper` GitHub repo instead of @PaperWalls4K X/Twitter feed. 31 topic folders with 4K JPEGs, public API, no auth required. GitHub API is free for public repos (60 req/hr unauthenticated), images are organized by topic, new topics auto-appear.

### 2026-04-24: Architecture — Single WinUI 3 App with BackgroundService

**Status:** Decided  
**Author:** Doc

Build as a single WinUI 3 desktop app with BackgroundService (not a true Windows service). Windows services run in session 0 with no UI access—tray icons require the user's desktop session. This avoids IPC complexity between service and tray app.

### 2026-04-24: UI Framework

**Status:** Decided  
**Author:** Michael Jolley

Use WinUI 3 for all UI (settings window, tray integration via H.NotifyIcon.WinUI). Not WPF or WinForms.

## Governance

- All meaningful changes require team consensus
- Document architectural decisions here
- Keep history focused on work, decisions focused on direction
