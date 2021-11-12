using Scani.Kiosk.Backends.GoogleSheets.Sheets.Models;

namespace Scani.Kiosk.Backends.GoogleSheets.Sheets
{
    public class KioskSheetReadResult<T>
        where T : ISheetRow
    {
        public KioskSheetReadResult(string sheetName, Func<int, string> dataRowNumberToRange, int maximumRowNumber)
        {
            this._maximumRowNumber = maximumRowNumber;
            this.SheetName = sheetName;
            this.DataRowNumberToRange = dataRowNumberToRange;
        }

        private int _maximumRowNumber;
        public string SheetName { get; set; }
        public bool Ok { get; set; }
        public ICollection<T> Rows { get; } = new List<T>();
        public ICollection<KioskSheetReadError> Errors { get; } = new List<KioskSheetReadError>();
        public IList<string> FlexFieldNames { get; } = new List<string>();
        public Func<int, string> DataRowNumberToRange { get; set; }
        public int MaxRowNumber => _maximumRowNumber;
        public int GetNextRowNumber()
        {
            return Interlocked.Increment(ref _maximumRowNumber);
        }
    }
}
