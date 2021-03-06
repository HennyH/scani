namespace Scani.Kiosk.Backends.GoogleSheets.Sheets.Models
{
    public interface IHaveScancodes
    {
        public string? CustomScancode { get; set; }
        public string GeneratedScancode { get; set; }
        public string PrimaryScancode { get; }
        public HashSet<string> Scancodes { get; }
        public bool HasScancode(string scancode);
    }
}
