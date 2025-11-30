using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using SchoolManagementSystem.Application.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace SchoolManagementSystem.Application.Services
{
    public class CacheService : ICacheService
    {
        private readonly IMemoryCache _memoryCache;
        private readonly ILogger<CacheService> _logger;

        public CacheService(IMemoryCache memoryCache, ILogger<CacheService> logger)
        {
            _memoryCache = memoryCache;
            _logger = logger;
        }

        public async Task<T> GetOrCreateAsync<T>(string key, Func<Task<T>> factory, TimeSpan expiration)
        {
            try
            {
                if (_memoryCache.TryGetValue(key, out T cachedValue))
                {
                    _logger.LogDebug("Cache hit for key: {Key}", key);
                    return cachedValue;
                }

                _logger.LogDebug("Cache miss for key: {Key}. Executing factory method.", key);
                var value = await factory();

                if (value != null)
                {
                    var cacheEntryOptions = new MemoryCacheEntryOptions()
                        .SetAbsoluteExpiration(expiration)
                        .SetSize(1);

                    _memoryCache.Set(key, value, cacheEntryOptions);
                    _logger.LogDebug("Cached value for key: {Key} with expiration: {Expiration}", key, expiration);
                }

                return value;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in GetOrCreateAsync for key: {Key}", key);
                // If caching fails, still return the result from factory
                return await factory();
            }
        }

        public async Task<T> GetOrCreateAsync<T>(string key, Func<Task<T>> factory, MemoryCacheEntryOptions options)
        {
            try
            {
                if (_memoryCache.TryGetValue(key, out T cachedValue))
                {
                    _logger.LogDebug("Cache hit for key: {Key}", key);
                    return cachedValue;
                }

                _logger.LogDebug("Cache miss for key: {Key}. Executing factory method.", key);
                var value = await factory();

                if (value != null)
                {
                    _memoryCache.Set(key, value, options);
                    _logger.LogDebug("Cached value for key: {Key} with custom options", key);
                }

                return value;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in GetOrCreateAsync for key: {Key}", key);
                return await factory();
            }
        }

        public Task<T> GetAsync<T>(string key)
        {
            try
            {
                if (_memoryCache.TryGetValue(key, out T value))
                {
                    _logger.LogDebug("Cache hit for key: {Key}", key);
                    return Task.FromResult(value);
                }

                _logger.LogDebug("Cache miss for key: {Key}", key);
                return Task.FromResult(default(T));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in GetAsync for key: {Key}", key);
                return Task.FromResult(default(T));
            }
        }

        public Task SetAsync<T>(string key, T value, TimeSpan expiration)
        {
            try
            {
                if (value != null)
                {
                    var cacheEntryOptions = new MemoryCacheEntryOptions()
                        .SetAbsoluteExpiration(expiration)
                        .SetSize(1);

                    _memoryCache.Set(key, value, cacheEntryOptions);
                    _logger.LogDebug("Cached value for key: {Key} with expiration: {Expiration}", key, expiration);
                }
                return Task.CompletedTask;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in SetAsync for key: {Key}", key);
                return Task.CompletedTask;
            }
        }

        public Task RemoveAsync(string key)
        {
            try
            {
                _memoryCache.Remove(key);
                _logger.LogDebug("Removed cache entry for key: {Key}", key);
                return Task.CompletedTask;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in RemoveAsync for key: {Key}", key);
                return Task.CompletedTask;
            }
        }

        public Task RemoveByPatternAsync(string pattern)
        {
            try
            {
                var keysToRemove = GetKeysByPattern(pattern);
                foreach (var key in keysToRemove)
                {
                    _memoryCache.Remove(key);
                }

                _logger.LogDebug("Removed {Count} cache entries matching pattern: {Pattern}", keysToRemove.Count, pattern);
                return Task.CompletedTask;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in RemoveByPatternAsync for pattern: {Pattern}", pattern);
                return Task.CompletedTask;
            }
        }

        public Task<bool> ExistsAsync(string key)
        {
            try
            {
                var exists = _memoryCache.TryGetValue(key, out _);
                return Task.FromResult(exists);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in ExistsAsync for key: {Key}", key);
                return Task.FromResult(false);
            }
        }

        public Task ClearAllAsync()
        {
            try
            {
                // For IMemoryCache, we can't easily clear all entries without knowing all keys
                // This is a limitation of the built-in MemoryCache
                _logger.LogWarning("ClearAllAsync is not fully supported with IMemoryCache. Consider using IDistributedCache for full cache management.");

                // We can try to clear known patterns or rely on expiration
                // In a real application, you might want to use a different cache implementation
                // or maintain a list of all keys
                return Task.CompletedTask;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in ClearAllAsync");
                return Task.CompletedTask;
            }
        }

        /// <summary>
        /// Gets all cache keys matching the specified pattern
        /// Note: This uses reflection to access internal fields and may not work in all environments
        /// </summary>
        private List<string> GetKeysByPattern(string pattern)
        {
            var keys = new List<string>();

            try
            {
                // Use reflection to access the internal entries collection
                var coherentState = typeof(MemoryCache).GetField("_coherentState", BindingFlags.NonPublic | BindingFlags.Instance);
                var memoryCache = _memoryCache as MemoryCache;

                if (memoryCache != null && coherentState != null)
                {
                    var coherentStateValue = coherentState.GetValue(memoryCache);
                    var entriesCollection = coherentStateValue?.GetType().GetProperty("EntriesCollection", BindingFlags.NonPublic | BindingFlags.Instance);
                    var entriesValue = entriesCollection?.GetValue(coherentStateValue);

                    if (entriesValue is ICollection<KeyValuePair<object, object>> entries)
                    {
                        foreach (var entry in entries)
                        {
                            var key = entry.Key.ToString();
                            if (key.Contains(pattern.Replace("*", "")))
                            {
                                keys.Add(key);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Unable to get cache keys by pattern using reflection. Pattern: {Pattern}", pattern);

                // Fallback: Since we can't reliably get all keys from MemoryCache,
                // we'll return an empty list and rely on individual key removal
                // In a production scenario, consider using a different cache implementation
                // that supports pattern-based removal natively
            }

            return keys;
        }
    }
}