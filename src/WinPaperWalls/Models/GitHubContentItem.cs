using System.Text.Json.Serialization;

namespace WinPaperWalls.Models;

/// <summary>
/// Represents an item from the GitHub Contents API.
/// </summary>
public sealed class GitHubContentItem
{
	[JsonPropertyName("name")]
	public string Name { get; set; } = string.Empty;

	[JsonPropertyName("type")]
	public string Type { get; set; } = string.Empty;

	[JsonPropertyName("path")]
	public string Path { get; set; } = string.Empty;

	[JsonPropertyName("download_url")]
	public string? DownloadUrl { get; set; }
}
