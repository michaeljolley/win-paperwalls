using WinPaperWalls.Models;

namespace WinPaperWalls.Services;

public interface ISettingsService
{
    AppSettings LoadSettings();
    void SaveSettings(AppSettings settings);
    event EventHandler? SettingsChanged;
}
