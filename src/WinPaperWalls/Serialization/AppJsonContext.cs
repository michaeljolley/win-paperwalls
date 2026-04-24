using System.Text.Json;
using System.Text.Json.Serialization;
using WinPaperWalls.Models;

namespace WinPaperWalls.Serialization;

[JsonSerializable(typeof(AppSettings))]
[JsonSerializable(typeof(List<GitHubContentItem>))]
[JsonSourceGenerationOptions(WriteIndented = true)]
internal partial class AppJsonContext : JsonSerializerContext;
