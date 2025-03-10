namespace Blocks.Core;

public static class DateTimeExtensions
{
		/// <summary>
		/// Converts a date to Unix epoch.
		/// </summary>
		/// <param name="date">The date to convert</param>
		/// <returns>Date converted to seconds since Unix epoch (Jan 1, 1970, midnight UTC).</returns>
		public static long ToUnixEpochDate(this DateTime date)
			=> (long)Math.Round((date.ToUniversalTime() -
													 new DateTimeOffset(1970, 1, 1, 0, 0, 0, TimeSpan.Zero))
													.TotalSeconds);
}
