using Microsoft.Extensions.Caching.Memory;

namespace Blocks.Core;

public interface IThreadSafeMemoryCache //:IMemoryCache
{
    bool TryGet<T>(string cacheKey, out T value);
		Task<T> GetOrCreateAsync<T>(string cacheKey, Func<ICacheEntry, Task<T>> factory);
    T GetOrCreate<T>(string cacheKey, Func<ICacheEntry, T> factory);
    T Set<T>(string cacheKey, T value);
    void Remove(string cacheKey);
}
