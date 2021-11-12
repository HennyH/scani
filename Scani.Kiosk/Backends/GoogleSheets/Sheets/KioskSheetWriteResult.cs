using Scani.Kiosk.Backends.GoogleSheets.Sheets.Models;

namespace Scani.Kiosk.Backends.GoogleSheets.Sheets
{
    public class KioskSheetWriteResult<T>
        where T : ISheetRow
    {
        public KioskSheetWriteResult(T row)
        {
            this.Row = row;
        }

        public T Row { get; set; }
        public bool Ok { get; set; }
        public ICollection<KioskSheetWriteError> Errors { get; } = new List<KioskSheetWriteError>();
    }
}
