using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using SistemaGVP.Application.Interfaces;

namespace SistemaGVP.Infrastructure.Caching;

public class MemoryCacheService : ICacheService
{
    private readonly IMemoryCache _cache;
    private readonly ILogger<MemoryCacheService> _logger;

    private static readonly Dictionary<string, HashSet<string>> PrefixIndex = new();
    private static readonly object IndexLock = new();

    public MemoryCacheService(IMemoryCache cache, ILogger<MemoryCacheService> logger)
    {
        _cache = cache;
        _logger = logger;
    }

    public T? GetOrCreate<T>(string key, Func<T> factory, TimeSpan expiration)
    {
        if (_cache.TryGetValue(key, out T? cached))
            return cached;

        var value = factory();

        if (value is not null)
        {
            IndexKey(key);
            _cache.Set(key, value, new MemoryCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = expiration,
                SlidingExpiration = expiration / 2
            });
        }

        return value;
    }

    public async Task<T?> GetOrCreateAsync<T>(string key, Func<Task<T>> factory, TimeSpan expiration)
    {
        if (_cache.TryGetValue(key, out T? cached))
            return cached;

        var value = await factory();

        if (value is not null)
        {
            IndexKey(key);
            _cache.Set(key, value, new MemoryCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = expiration,
                SlidingExpiration = expiration / 2
            });
        }

        return value;
    }

    public void Remove(string key)
    {
        _cache.Remove(key);
        lock (IndexLock)
        {
            foreach (var entry in PrefixIndex.Values)
                entry.Remove(key);
        }
    }

    public void RemoveByPrefix(string prefix)
    {
        lock (IndexLock)
        {
            if (!PrefixIndex.TryGetValue(prefix, out var keys)) return;

            foreach (var key in keys)
                _cache.Remove(key);

            keys.Clear();
        }
    }

    private static void IndexKey(string key)
    {
        var prefix = key[..key.IndexOf(':')] ?? key;
        lock (IndexLock)
        {
            if (!PrefixIndex.ContainsKey(prefix))
                PrefixIndex[prefix] = new HashSet<string>();
            PrefixIndex[prefix].Add(key);
        }
    }
}
