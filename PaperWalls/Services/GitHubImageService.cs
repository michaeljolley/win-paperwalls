using Microsoft.Extensions.Logging;
using System.Net.Http.Json;
using System.Text.Json.Serialization;
using PaperWalls.Models;

namespace PaperWalls.Services;

internal sealed partial class GitHubImageService : IGitHubImageService
{
	private const string ApiBaseUrl = "https://api.github.com/repos/burkeholland/paper/contents/wallpapers";
	private const string UserAgent = "PaperWalls/1.0";
	private static readonly TimeSpan CacheExpiry = TimeSpan.FromHours(24);

	private readonly HttpClient _httpClient;
	private readonly ISettingsService _settingsService;
	private readonly ILogger<GitHubImageService> _logger;

	private DateTime _topicsCacheTime = DateTime.MinValue;
	private List<string>? _cachedTopics;
	private readonly Dictionary<string, (DateTime timestamp, List<WallpaperImage> images)> _imageCache = new();
	private readonly object _cacheLock = new();

	public GitHubImageService(
		IHttpClientFactory httpClientFactory,
		ISettingsService settingsService,
		ILogger<GitHubImageService> logger)
	{
		_httpClient = httpClientFactory.CreateClient("GitHub");
		_httpClient.DefaultRequestHeaders.Add("User-Agent", UserAgent);
		_settingsService = settingsService;
		_logger = logger;
	}

	public async Task<List<string>> GetTopicsAsync()
	{
		lock (_cacheLock)
		{
			if (_cachedTopics != null && DateTime.UtcNow - _topicsCacheTime < CacheExpiry)
			{
				LogReturningCachedTopics(_logger);
				return new List<string>(_cachedTopics);
			}
		}

		try
		{
			LogFetchingTopicsFromGitHub(_logger);

			var response = await _httpClient.GetAsync(ApiBaseUrl).ConfigureAwait(false);

			CheckRateLimit(response);

			response.EnsureSuccessStatusCode();

			var items = await response.Content.ReadFromJsonAsync<List<GitHubContentItem>>().ConfigureAwait(false);
			if (items == null)
			{
				LogGitHubApiReturnedNull(_logger);
				return new List<string>();
			}

			var topics = items
				.Where(i => i.Type == "dir")
				.Select(i => i.Name)
				.ToList();

			var settings = _settingsService.LoadSettings();
			var filteredTopics = topics
				.Where(t => !settings.ExcludedTopics.Contains(t, StringComparer.OrdinalIgnoreCase))
				.ToList();

			lock (_cacheLock)
			{
				_cachedTopics = filteredTopics;
				_topicsCacheTime = DateTime.UtcNow;
			}

			LogFetchedTopics(_logger, topics.Count, filteredTopics.Count);
			return filteredTopics;
		}
		catch (HttpRequestException ex)
		{
			LogFailedToFetchTopics(_logger, ex);

			// Return cached data if available
			lock (_cacheLock)
			{
				if (_cachedTopics != null)
				{
					LogReturningStaleCachedTopics(_logger);
					return new List<string>(_cachedTopics);
				}
			}

			throw;
		}
	}

	public async Task<List<WallpaperImage>> GetImagesAsync(string topic)
	{
		lock (_cacheLock)
		{
			if (_imageCache.TryGetValue(topic, out var cached) &&
				DateTime.UtcNow - cached.timestamp < CacheExpiry)
			{
				LogReturningCachedImages(_logger, topic);
				return new List<WallpaperImage>(cached.images);
			}
		}

		try
		{
			LogFetchingImagesForTopic(_logger, topic);

			var url = $"{ApiBaseUrl}/{topic}";
			var response = await _httpClient.GetAsync(url).ConfigureAwait(false);

			CheckRateLimit(response);

			response.EnsureSuccessStatusCode();

			var items = await response.Content.ReadFromJsonAsync<List<GitHubContentItem>>().ConfigureAwait(false);
			if (items == null)
			{
				LogGitHubApiReturnedNullForTopic(_logger, topic);
				return new List<WallpaperImage>();
			}

			var images = items
				.Where(i => i.Type == "file" && IsImageFile(i.Name))
				.Select(i => new WallpaperImage
				{
					FileName = i.Name,
					Url = i.DownloadUrl ?? string.Empty,
					Topic = topic
				})
				.ToList();

			lock (_cacheLock)
			{
				_imageCache[topic] = (DateTime.UtcNow, images);
			}

			LogFetchedImages(_logger, images.Count, topic);
			return images;
		}
		catch (HttpRequestException ex)
		{
			LogFailedToFetchImages(_logger, ex, topic);

			// Return cached data if available
			lock (_cacheLock)
			{
				if (_imageCache.TryGetValue(topic, out var cached))
				{
					LogReturningStaleCachedImages(_logger, topic);
					return new List<WallpaperImage>(cached.images);
				}
			}

			throw;
		}
	}

	private void CheckRateLimit(HttpResponseMessage response)
	{
		if (response.Headers.TryGetValues("X-RateLimit-Remaining", out var remainingValues))
		{
			if (int.TryParse(remainingValues.FirstOrDefault(), out var remaining))
			{
				LogGitHubApiRateLimit(_logger, remaining);

				if (remaining < 10)
				{
					LogGitHubApiRateLimitRunningLow(_logger, remaining);
				}
			}
		}

		if (response.StatusCode == System.Net.HttpStatusCode.Forbidden)
		{
			if (response.Headers.TryGetValues("X-RateLimit-Reset", out var resetValues))
			{
				if (long.TryParse(resetValues.FirstOrDefault(), out var resetTimestamp))
				{
					var resetTime = DateTimeOffset.FromUnixTimeSeconds(resetTimestamp);
					LogGitHubApiRateLimitExceeded(_logger, resetTime);
				}
			}
		}
	}

	private static bool IsImageFile(string fileName)
	{
		var extension = Path.GetExtension(fileName).ToLowerInvariant();
		return extension is ".jpg" or ".jpeg" or ".png" or ".bmp" or ".webp";
	}

	// LoggerMessage source-generated methods for Native AOT compatibility
	[LoggerMessage(EventId = 2000, Level = LogLevel.Debug, Message = "Returning cached topics")]
	private static partial void LogReturningCachedTopics(ILogger logger);

	[LoggerMessage(EventId = 2001, Level = LogLevel.Information, Message = "Fetching topics from GitHub")]
	private static partial void LogFetchingTopicsFromGitHub(ILogger logger);

	[LoggerMessage(EventId = 2002, Level = LogLevel.Warning, Message = "GitHub API returned null response")]
	private static partial void LogGitHubApiReturnedNull(ILogger logger);

	[LoggerMessage(EventId = 2003, Level = LogLevel.Information, Message = "Fetched {Count} topics (filtered to {FilteredCount})")]
	private static partial void LogFetchedTopics(ILogger logger, int count, int filteredCount);

	[LoggerMessage(EventId = 2004, Level = LogLevel.Error, Message = "Failed to fetch topics from GitHub")]
	private static partial void LogFailedToFetchTopics(ILogger logger, Exception ex);

	[LoggerMessage(EventId = 2005, Level = LogLevel.Warning, Message = "Returning stale cached topics due to error")]
	private static partial void LogReturningStaleCachedTopics(ILogger logger);

	[LoggerMessage(EventId = 2006, Level = LogLevel.Debug, Message = "Returning cached images for topic {Topic}")]
	private static partial void LogReturningCachedImages(ILogger logger, string topic);

	[LoggerMessage(EventId = 2007, Level = LogLevel.Information, Message = "Fetching images for topic {Topic} from GitHub")]
	private static partial void LogFetchingImagesForTopic(ILogger logger, string topic);

	[LoggerMessage(EventId = 2008, Level = LogLevel.Warning, Message = "GitHub API returned null response for topic {Topic}")]
	private static partial void LogGitHubApiReturnedNullForTopic(ILogger logger, string topic);

	[LoggerMessage(EventId = 2009, Level = LogLevel.Information, Message = "Fetched {Count} images for topic {Topic}")]
	private static partial void LogFetchedImages(ILogger logger, int count, string topic);

	[LoggerMessage(EventId = 2010, Level = LogLevel.Error, Message = "Failed to fetch images for topic {Topic} from GitHub")]
	private static partial void LogFailedToFetchImages(ILogger logger, Exception ex, string topic);

	[LoggerMessage(EventId = 2011, Level = LogLevel.Warning, Message = "Returning stale cached images for topic {Topic} due to error")]
	private static partial void LogReturningStaleCachedImages(ILogger logger, string topic);

	[LoggerMessage(EventId = 2012, Level = LogLevel.Debug, Message = "GitHub API rate limit remaining: {Remaining}")]
	private static partial void LogGitHubApiRateLimit(ILogger logger, int remaining);

	[LoggerMessage(EventId = 2013, Level = LogLevel.Warning, Message = "GitHub API rate limit running low: {Remaining} requests remaining")]
	private static partial void LogGitHubApiRateLimitRunningLow(ILogger logger, int remaining);

	[LoggerMessage(EventId = 2014, Level = LogLevel.Error, Message = "GitHub API rate limit exceeded. Resets at {ResetTime}")]
	private static partial void LogGitHubApiRateLimitExceeded(ILogger logger, DateTimeOffset resetTime);

	private class GitHubContentItem
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
}
