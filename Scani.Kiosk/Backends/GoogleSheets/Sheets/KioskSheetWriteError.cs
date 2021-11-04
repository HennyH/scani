namespace Scani.Kiosk.Backends.GoogleSheets.Sheets
{
    public class KioskSheetWriteError
    {
        public string SheetName { get; }
        public KioskSheetReadErrorType ErrorType { get; }
        public string Message { get; }

        public KioskSheetWriteError(KioskSheetReadErrorType errorType, string sheetName, string message)
        {
            this.SheetName = sheetName;
            this.ErrorType = errorType;
            this.Message = message;
        }
    }

    // requried field missing
}
