namespace Scani.Kiosk.Backends.GoogleSheets.Sheets.Models
{
    public interface IHaveFlexFields
    {
        public IDictionary<string, string?> FlexFields { get; set; }
    }
}
