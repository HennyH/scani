namespace Scani.Kiosk.Backends.GoogleSheets.Sheets.Models
{
    public static class CellTextValue
    {
        private readonly static string[] TRUTHY_TEXTS = new[] { "y", "yes", "true" };
        private readonly static string[] FALSEY_TEXTS = new[] { "n", "no", "false" };
        public static bool IsTruthy(string? text) => !string.IsNullOrWhiteSpace(text) && TRUTHY_TEXTS.Any(tt => string.Equals(tt, text, StringComparison.OrdinalIgnoreCase));
        public static bool IsFalsy(string? text) => !string.IsNullOrWhiteSpace(text) && FALSEY_TEXTS.Any(ft => string.Equals(ft, text, StringComparison.OrdinalIgnoreCase));
    }
}
