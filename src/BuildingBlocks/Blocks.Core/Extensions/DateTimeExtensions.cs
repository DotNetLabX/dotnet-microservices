namespace Blocks.Core;

public static class DateTimeExtensions
{
    /// Converts a DateTime to Unix epoch seconds.
    public static long ToUnixEpochDate(this DateTime date)
        => (long)Math.Round((date.ToUniversalTime() -
                             new DateTimeOffset(1970, 1, 1, 0, 0, 0, TimeSpan.Zero))
                            .TotalSeconds);
}
