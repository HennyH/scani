using System.Globalization;

namespace Scani.Kiosk.Helpers
{
    public static class DateTimeHelpers
    {
        public static string ToIsoString(this DateTime datetime)
            => datetime.ToString("O", CultureInfo.InvariantCulture);

        public static DateTime FromIsoString(this string isoDateTimeText)
            => DateTime.ParseExact(isoDateTimeText, "O", CultureInfo.InvariantCulture);
    }
}
