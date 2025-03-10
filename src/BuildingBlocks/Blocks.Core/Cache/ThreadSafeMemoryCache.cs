using Microsoft.Extensions.Caching.Memory;

namespace Blocks.Core;

public class ThreadSafeMemoryCache(IMemoryCache _cache) : IThreadSafeMemoryCache
{
    private MemoryCacheEntryOptions _cacheOptions = new MemoryCacheEntryOptions().SetSlidingExpiration(TimeSpan.FromDays(1));
    
    //the extensions method provided for IMemoryCache are not thread safe
    private static readonly AsyncLock _mutex = new AsyncLock();

    public bool TryGet<T>(string cacheKey, out T value)
    {
        using (_mutex.Lock())
        {
            _cache.TryGetValue(cacheKey, out value);
            return value != null;
        }
    }
    public T GetOrCreate<T>(string cacheKey, T value)
    {
        using (_mutex.Lock())
        {
            return _cache.GetOrCreate(cacheKey, entry =>
            {
                entry.SetOptions(_cacheOptions);
                return value;
            });
        }
    }

		public T GetOrCreate<T>(T value)
		{
				using (_mutex.Lock())
				{
						return _cache.GetOrCreate(typeof(T).FullName, entry =>
						{
								entry.SetOptions(_cacheOptions);
								return value;
						});
				}
		}

		public async Task<T> GetOrCreateAsync<T>(string cacheKey, Func<ICacheEntry, Task<T>> factory)
    {
        using (_mutex.Lock())
        {
            return (await _cache.GetOrCreateAsync(cacheKey, factory));
        }
    }

    public T GetOrCreate<T>(string cacheKey, Func<ICacheEntry, T> factory)
    {
        using (_mutex.Lock())
        {
            return _cache.GetOrCreate(cacheKey, factory);
        }
    }
		public T GetOrCreate<T>(Func<ICacheEntry, T> factory)
		{
				using (_mutex.Lock())
				{
						return _cache.GetOrCreate(typeof(T).FullName, factory);
				}
		}

		public T Set<T>(string cacheKey, T value)
    {
        return _cache.Set(cacheKey, value, _cacheOptions);
    }
    public void Remove(string cacheKey)
    {
        _cache.Remove(cacheKey);
    }
}
