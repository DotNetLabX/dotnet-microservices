using Microsoft.Extensions.Caching.Memory;

namespace Blocks.Core.Cache;

public static class MemoryCacheExtensions
{
    public static T GetOrCreateByType<T>(this IMemoryCache memoryCache, Func<ICacheEntry, T> factory)
        => memoryCache.GetOrCreate(typeof(T).FullName!, factory)!;

    public static T Get<T>(this IMemoryCache cache)
        => cache.Get<T>(typeof(T).FullName!)!;
}
