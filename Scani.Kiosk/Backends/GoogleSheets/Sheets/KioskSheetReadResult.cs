using Scani.Kiosk.Backends.GoogleSheets.Sheets.Models;

namespace Scani.Kiosk.Backends.GoogleSheets.Sheets
{
    public class KioskSheetReadResult<T>
        where T : ISheetRow
    {
        public KioskSheetReadResult(string sheetName, Func<int, string> dataRowNumberToRange)
        {
            this.SheetName = sheetName;
            this.DataRowNumberToRange = dataRowNumberToRange;
        }

        public string SheetName { get; set; }
        public bool Ok { get; set; }
        public ICollection<T> Rows { get; set; } = new List<T>();
        public ICollection<KioskSheetReadError> Errors { get; set; } = new List<KioskSheetReadError>();
        public IList<string> FlexFieldNames { get; set; } = new List<string>();
        public Func<int, string> DataRowNumberToRange { get; set; }
        public int NextDataRowNumber;
    }
}
