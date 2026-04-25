using Microsoft.Extensions.Logging;

namespace PaperWalls.Services;

internal sealed partial class CacheService : ICacheService
{
	private readonly string _cacheDirectory;
	private readonly HttpClient _httpClient;
	private readonly ILogger<CacheService> _logger;
	private readonly object _cacheLock = new();

	public CacheService(IHttpClientFactory httpClientFactory, ILogger<CacheService> logger, string? cacheDirectory = null)
	{
		if (cacheDirectory != null)
		{
			_cacheDirectory = cacheDirectory;
		}
		else
		{
			var localAppData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
			_cacheDirectory = Path.Combine(localAppData, "PaperWalls", "cache");
		}

		_httpClient = httpClientFactory.CreateClient();
		_logger = logger;

		EnsureCacheDirectoryExists();
	}

	public async Task<string> DownloadImageAsync(string url, string fileName)
	{
		var filePath = Path.Combine(_cacheDirectory, fileName);

		// If already cached, return immediately
		if (File.Exists(filePath))
		{
			LogImageAlreadyCached(_logger, fileName);

			// Update last access time for LRU tracking
			File.SetLastAccessTime(filePath, DateTime.UtcNow);
			return filePath;
		}

		LogDownloadingImage(_logger, fileName, url);

		try
		{
			var response = await _httpClient.GetAsync(url).ConfigureAwait(false);
			response.EnsureSuccessStatusCode();

			var imageBytes = await response.Content.ReadAsByteArrayAsync().ConfigureAwait(false);

			lock (_cacheLock)
			{
				// Double-check in case another thread downloaded it
				if (!File.Exists(filePath))
				{
					File.WriteAllBytes(filePath, imageBytes);
					LogDownloadedAndCached(_logger, fileName, imageBytes.Length);
				}
			}

			return filePath;
		}
		catch (Exception ex)
		{
			LogFailedToDownloadImage(_logger, ex, fileName, url);
			throw;
		}
	}

	public string? GetCachedImagePath(string fileName)
	{
		var filePath = Path.Combine(_cacheDirectory, fileName);

		if (File.Exists(filePath))
		{
			// Update last access time
			File.SetLastAccessTime(filePath, DateTime.UtcNow);
			return filePath;
		}

		return null;
	}

	public long GetCacheSizeBytes()
	{
		if (!Directory.Exists(_cacheDirectory))
		{
			return 0;
		}

		try
		{
			var files = Directory.GetFiles(_cacheDirectory);
			return files.Sum(f => new FileInfo(f).Length);
		}
		catch (Exception ex)
		{
			LogFailedToCalculateCacheSize(_logger, ex);
			return 0;
		}
	}

	public async Task EvictOldestAsync(long targetBytes)
	{
		LogStartingCacheEviction(_logger, targetBytes / 1024 / 1024);

		if (!Directory.Exists(_cacheDirectory))
		{
			return;
		}

		await Task.Run(() =>
		{
			lock (_cacheLock)
			{
				var files = Directory.GetFiles(_cacheDirectory)
					.Select(f => new FileInfo(f))
					.OrderBy(fi => fi.LastAccessTime)
					.ToList();

				var currentSize = files.Sum(f => f.Length);
				var deletedCount = 0;
				var freedBytes = 0L;

				foreach (var file in files)
				{
					if (currentSize <= targetBytes)
					{
						break;
					}

					try
					{
						var fileSize = file.Length;
						file.Delete();
						currentSize -= fileSize;
						freedBytes += fileSize;
						deletedCount++;

						LogEvictedFile(_logger, file.Name, fileSize);
					}
					catch (Exception ex)
					{
						LogFailedToDeleteCachedFile(_logger, ex, file.Name);
					}
				}

				LogCacheEvictionComplete(_logger, deletedCount, freedBytes / 1024 / 1024);
			}
		});
	}

	public async Task ClearCacheAsync()
	{
		LogClearingAllCachedImages(_logger);

		if (!Directory.Exists(_cacheDirectory))
		{
			return;
		}

		await Task.Run(() =>
		{
			lock (_cacheLock)
			{
				try
				{
					var files = Directory.GetFiles(_cacheDirectory);
					foreach (var file in files)
					{
						try
						{
							File.Delete(file);
						}
						catch (Exception ex)
						{
							LogFailedToDeleteFile(_logger, ex, Path.GetFileName(file));
						}
					}

					LogClearedCachedImages(_logger, files.Length);
				}
				catch (Exception ex)
				{
					LogFailedToClearCache(_logger, ex);
					throw;
				}
			}
		});
	}

	private void EnsureCacheDirectoryExists()
	{
		if (!Directory.Exists(_cacheDirectory))
		{
			Directory.CreateDirectory(_cacheDirectory);
			LogCreatedCacheDirectory(_logger, _cacheDirectory);
		}
	}

	// LoggerMessage source-generated methods for Native AOT compatibility
	[LoggerMessage(EventId = 1000, Level = LogLevel.Debug, Message = "Image {FileName} already cached")]
	private static partial void LogImageAlreadyCached(ILogger logger, string fileName);

	[LoggerMessage(EventId = 1001, Level = LogLevel.Information, Message = "Downloading image {FileName} from {Url}")]
	private static partial void LogDownloadingImage(ILogger logger, string fileName, string url);

	[LoggerMessage(EventId = 1002, Level = LogLevel.Information, Message = "Downloaded and cached {FileName} ({Size} bytes)")]
	private static partial void LogDownloadedAndCached(ILogger logger, string fileName, long size);

	[LoggerMessage(EventId = 1003, Level = LogLevel.Error, Message = "Failed to download image {FileName} from {Url}")]
	private static partial void LogFailedToDownloadImage(ILogger logger, Exception ex, string fileName, string url);

	[LoggerMessage(EventId = 1004, Level = LogLevel.Error, Message = "Failed to calculate cache size")]
	private static partial void LogFailedToCalculateCacheSize(ILogger logger, Exception ex);

	[LoggerMessage(EventId = 1005, Level = LogLevel.Information, Message = "Starting cache eviction to reach target size of {TargetMB} MB")]
	private static partial void LogStartingCacheEviction(ILogger logger, long targetMB);

	[LoggerMessage(EventId = 1006, Level = LogLevel.Debug, Message = "Evicted {FileName} ({Size} bytes)")]
	private static partial void LogEvictedFile(ILogger logger, string fileName, long size);

	[LoggerMessage(EventId = 1007, Level = LogLevel.Error, Message = "Failed to delete cached file {FileName}")]
	private static partial void LogFailedToDeleteCachedFile(ILogger logger, Exception ex, string fileName);

	[LoggerMessage(EventId = 1008, Level = LogLevel.Information, Message = "Cache eviction complete: deleted {Count} files, freed {FreedMB} MB")]
	private static partial void LogCacheEvictionComplete(ILogger logger, int count, long freedMB);

	[LoggerMessage(EventId = 1009, Level = LogLevel.Information, Message = "Clearing all cached images")]
	private static partial void LogClearingAllCachedImages(ILogger logger);

	[LoggerMessage(EventId = 1010, Level = LogLevel.Error, Message = "Failed to delete file {FileName}")]
	private static partial void LogFailedToDeleteFile(ILogger logger, Exception ex, string fileName);

	[LoggerMessage(EventId = 1011, Level = LogLevel.Information, Message = "Cleared {Count} cached images")]
	private static partial void LogClearedCachedImages(ILogger logger, int count);

	[LoggerMessage(EventId = 1012, Level = LogLevel.Error, Message = "Failed to clear cache")]
	private static partial void LogFailedToClearCache(ILogger logger, Exception ex);

	[LoggerMessage(EventId = 1013, Level = LogLevel.Information, Message = "Created cache directory at {Path}")]
	private static partial void LogCreatedCacheDirectory(ILogger logger, string path);
}
