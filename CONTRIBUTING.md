# Contributing to WinPaperWalls

Thank you for your interest in contributing to WinPaperWalls! This guide will
help you get started.

## Code of Conduct

Please be respectful and constructive in all interactions. We're all here to
build something great together.

## Getting Started

### Prerequisites

- Windows 10 (build 1809) or later
- [.NET 9.0 SDK](https://dotnet.microsoft.com/download/dotnet/9.0)
- Visual Studio 2022 (version 17.8+) or later with the following workloads:
  - .NET desktop development
  - Windows App SDK
- [Windows App SDK 1.6+](https://learn.microsoft.com/en-us/windows/apps/windows-app-sdk/downloads-and-tools)

### Building and Testing

```bash
dotnet restore
dotnet build
dotnet test
```

To run the application:

```bash
dotnet run --project src/WinPaperWalls/WinPaperWalls.csproj
```

## How to Contribute

### Reporting Bugs

- Search [existing issues](https://github.com/michaeljolley/win-paperwalls/issues)
  before opening a new one
- Include steps to reproduce, expected behavior, and actual behavior
- Screenshots are very helpful for UI issues

### Suggesting Features

Open an issue describing the feature, why it would be useful, and any
implementation ideas you have.

### Submitting Changes

1. Fork the repository
2. Create a feature branch from `main` (`git checkout -b feature/my-change`)
3. Make your changes
4. Ensure the project builds and tests pass
5. Commit using [conventional commits](#commit-messages)
6. Push to your fork and open a pull request against `main`

### Commit Messages

This project uses [Conventional Commits](https://www.conventionalcommits.org/).
Please format your commit messages as:

```
type: short description

Optional longer description.
```

Common types:

| Type | Description |
|------|-------------|
| `feat` | New feature |
| `fix` | Bug fix |
| `docs` | Documentation changes |
| `ci` | CI/CD changes |
| `chore` | Maintenance tasks |
| `refactor` | Code changes that don't fix a bug or add a feature |
| `test` | Adding or updating tests |

### Pull Requests

- Keep PRs focused — one logical change per PR
- Ensure CI passes (build + tests run automatically)
- Provide a clear description of what changed and why
- Link any related issues (e.g., "Fixes #123")

## Project Structure

```
src/WinPaperWalls/
├── Assets/              # App icons (logo.png, logo.ico)
├── Interop/             # Win32 P/Invoke for wallpaper setting
├── Models/              # Data models (AppSettings, WallpaperImage)
├── Services/            # Business logic (Settings, GitHub, Cache, Wallpaper, Scheduler, DesktopWallpaper, Startup)
├── App.xaml.cs          # App entry, DI host, single-instance mutex
├── MainWindow.xaml      # Settings window
├── TrayIconView.xaml    # System tray icon and context menu
└── WinPaperWalls.csproj

tests/WinPaperWalls.Tests/  # Unit tests (xunit + NSubstitute + FluentAssertions)
```

## License

By contributing, you agree that your contributions will be licensed under the
[MIT License](LICENSE).
