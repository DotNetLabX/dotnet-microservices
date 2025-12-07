namespace Blocks.Core;

public static class StringExtensions
{
    public static string FormatWith(this string @this, params object[] additionalArgs)
        => string.Format(@this, additionalArgs);

    public static string FormatWith(this string @this, object arg)
        => string.Format(@this, arg);

    public static bool IsNullOrEmpty(this string? value)
    {
        return string.IsNullOrEmpty(value);
    }

    public static bool EqualsIgnoreCase(this string me, string theOther)
    {
        return me.Equals(theOther, StringComparison.InvariantCultureIgnoreCase);
    }
    public static bool EqualsOrdinalIgnoreCase(this string first, string second)
    {
        return string.Compare(first, second, StringComparison.OrdinalIgnoreCase) == 0;
    }

    public static string ToCamelCase(this string value)
    {
        if (string.IsNullOrEmpty(value))
            return value;

        return char.ToLowerInvariant(value[0]) + value.Substring(1);
    }

    public static T ToEnum<T>(this string value) where T : struct, Enum
            => (T)Enum.Parse(typeof(T), value, true);

    public static int? ToInt(this string input)
    {
        int i;
        if (int.TryParse(input, out i)) return i;
        return null;
    }
}

