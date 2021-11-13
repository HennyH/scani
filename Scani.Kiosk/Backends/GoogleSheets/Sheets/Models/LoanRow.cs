using Scani.Kiosk.Helpers;

namespace Scani.Kiosk.Backends.GoogleSheets.Sheets.Models
{
    [GoogleSheet("Loans", HeaderCount:1)]
    public class LoanRow : ISheetRow
    {
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        private LoanRow()
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        { }

        public LoanRow(string studentScancode, string equipmentScancode, DateTime loanDate, KioskSheetReadResult<LoanRow> loanSheet)
        {
            ArgumentNullException.ThrowIfNull(loanSheet);
            this.IdText = Guid.NewGuid().ToString();
            this.StudentScancode = studentScancode;
            this.EquipmentScancode = equipmentScancode;
            this.LoanedDate = loanDate;
            this.Range = loanSheet.DataRowNumberToRange(loanSheet.GetNextRowNumber());
        }

        [SheetColumn("ID", ColumnNumber: 1, IsRequired = true)]
        public string IdText { get; set; }

        public Guid Id => Guid.Parse(IdText);

        [SheetColumn("Student Scancode", ColumnNumber: 2, IsRequired = true)]
        public string StudentScancode { get; set; }

        [SheetColumn("Equipment Scancode", ColumnNumber: 3, IsRequired = true)]
        public string EquipmentScancode { get; set; }

        [SheetColumn("Loaned Date", ColumnNumber: 4, IsRequired = true)]
        public DateTime LoanedDate { get; set; }

        [SheetColumn("Returned Date", ColumnNumber: 5)]
        public DateTime? ReturnedDate { get; set; }

        public string Range { get; set; }
    }
}
