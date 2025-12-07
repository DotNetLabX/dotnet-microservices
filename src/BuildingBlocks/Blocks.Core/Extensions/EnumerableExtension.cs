using System.Text;

namespace Blocks.Core;

public static class EnumerableExtensions
{
    public static bool IsEmpty<T>(this IEnumerable<T> enumerable) 
        => !enumerable.Any();

    public static bool IsNullOrEmpty<T>(this IEnumerable<T>? enumerable)
        => enumerable == null || !enumerable.Any();

    public static bool IsNotNullOrEmpty<T>(this IEnumerable<T>? enumerable)
        => enumerable != null && enumerable.Any();

    public static bool IsNullOrEmpty<T>(this Dictionary<T, T>? dict) where T : notnull
        => dict is null || dict.Count == 0;


    public static bool In<T>(this T o, params T[] values)
    {
        if (values == null) return false;

        return values.Contains(o);
    }

    public static bool HasAny<TSource>(this IEnumerable<TSource> @this) => @this != null && @this.Any();
    public static bool HasMany<T>(this IEnumerable<T> @this)
    {
        using (var en = @this.GetEnumerator())
            return en.MoveNext() && en.MoveNext();
    }

    public static string ToString<T>(this IEnumerable<T> @this, string seperator) => ToString(@this, seperator, seperator);

    public static string ToString<T>(this IEnumerable<T> @this, string seperator, string lastSeperator)
    {
        var result = new StringBuilder();

        var items = @this.Cast<object>().ToArray();

        for (var i = 0; i < items.Length; i++)
        {
            var item = items[i];

            if (item == null) result.Append("{NULL}");
            else result.Append(item.ToString());

            if (i < items.Length - 2)
                result.Append(seperator);

            if (i == items.Length - 2)
                result.Append(lastSeperator);
        }

        return result.ToString();
    }
}
