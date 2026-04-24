using WinPaperWalls.Models;
using WinPaperWalls.Services;

namespace WinPaperWalls.Tests.Services;

public class SettingsServiceTests : IDisposable
{
    private readonly string _testSettingsPath;
    private readonly SettingsService _service;

    public SettingsServiceTests()
    {
        // Use a test-specific directory
        var localAppData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        _testSettingsPath = Path.Combine(localAppData, "WinPaperWalls_Test_" + Guid.NewGuid().ToString());
        Directory.CreateDirectory(_testSettingsPath);

        // Set environment for test
        _service = new SettingsService();
    }

    public void Dispose()
    {
        // Clean up test directory
        if (Directory.Exists(_testSettingsPath))
        {
            Directory.Delete(_testSettingsPath, true);
        }
    }

    [Fact]
    public void LoadSettings_WhenNoFileExists_CreatesDefaultSettings()
    {
        // Act
        var settings = _service.LoadSettings();

        // Assert
        Assert.NotNull(settings);
        Assert.Equal(1440, settings.IntervalMinutes);
        Assert.Equal("Fill", settings.WallpaperStyle);
        Assert.Equal(500, settings.CacheMaxMB);
        Assert.True(settings.StartWithWindows);
        Assert.Empty(settings.ExcludedTopics);
    }

    [Fact]
    public void SaveAndLoadSettings_RoundTrip_PreservesData()
    {
        // Arrange
        var settings = new AppSettings
        {
            IntervalMinutes = 60,
            WallpaperStyle = "Fit",
            CacheMaxMB = 1000,
            StartWithWindows = false,
            ExcludedTopics = new List<string> { "Nature", "Space" }
        };

        // Act
        _service.SaveSettings(settings);
        var loaded = _service.LoadSettings();

        // Assert
        Assert.Equal(60, loaded.IntervalMinutes);
        Assert.Equal("Fit", loaded.WallpaperStyle);
        Assert.Equal(1000, loaded.CacheMaxMB);
        Assert.False(loaded.StartWithWindows);
        Assert.Equal(2, loaded.ExcludedTopics.Count);
        Assert.Contains("Nature", loaded.ExcludedTopics);
        Assert.Contains("Space", loaded.ExcludedTopics);
    }

    [Fact]
    public void SaveSettings_FiresSettingsChangedEvent()
    {
        // Arrange
        var eventFired = false;
        _service.SettingsChanged += (sender, args) => eventFired = true;
        var settings = new AppSettings();

        // Act
        _service.SaveSettings(settings);

        // Assert
        Assert.True(eventFired);
    }
}
