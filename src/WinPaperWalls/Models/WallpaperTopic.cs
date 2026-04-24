namespace WinPaperWalls.Models;

public class WallpaperTopic
{
    public string Name { get; set; } = string.Empty;
    public List<string> ImageFileNames { get; set; } = new();
}
