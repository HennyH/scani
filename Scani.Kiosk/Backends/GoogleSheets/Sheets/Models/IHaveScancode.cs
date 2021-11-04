namespace Scani.Kiosk.Backends.GoogleSheets.Sheets.Models
{
    public interface IHaveScancode
    {
        public string? CustomScancode { get; set; }
        public string GeneratedScancode { get; set; }
        public string Scancode => CustomScancode ?? GeneratedScancode;
    }
}
