using Blocks.Exceptions;

namespace Blocks.Linq;

public static class Extensions
{
		public static TSource SingleOrThrow<TSource>(this IEnumerable<TSource> source, Func<TSource, bool> predicate)
		{
				var item = source.SingleOrDefault(predicate);
				if (item is null) throw new NotFoundException($"{typeof(TSource).Name} not found");
				return item;
		}
}
