using System.Globalization;

namespace Scani.Kiosk.Backends.GoogleSheets.Sheets.Models
{
    [GoogleSheet("Loans", HeaderCount: 1)]
    public class LoanRow : ISheetRow
    {
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        private LoanRow()
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        { }

        public LoanRow(string idText, string studentScancode, string equipmentScancode, string loanDateText, KioskSheetReadResult<LoanRow> loanSheet)
        {
            ArgumentNullException.ThrowIfNull(loanSheet);
            this.IdText = idText;
            this.StudentScancode = studentScancode;
            this.EquipmentScancode = equipmentScancode;
            this.LoanDateText = loanDateText;
            this.Range = loanSheet.DataRowNumberToRange(loanSheet.GetNextRowNumber());
        }

        public LoanRow(string studentScancode, string equipmentScancode, string loanDateText, KioskSheetReadResult<LoanRow> loanSheet)
        {
            ArgumentNullException.ThrowIfNull(loanSheet);
            this.IdText = Guid.NewGuid().ToString();
            this.StudentScancode = studentScancode;
            this.EquipmentScancode = equipmentScancode;
            this.LoanDateText = loanDateText;
            this.Range = loanSheet.DataRowNumberToRange(loanSheet.GetNextRowNumber());
        }

        public LoanRow(string studentScancode, string equipmentScancode, DateTime loanDate, KioskSheetReadResult<LoanRow> loanSheet)
            : this(studentScancode, equipmentScancode, loanDate.ToString(CultureInfo.DefaultThreadCurrentCulture), loanSheet)
        { }

        [SheetColumn("ID", ColumnNumber: 1, IsRequired = true)]
        public string IdText { get; set; }

        public Guid Id => Guid.Parse(IdText);

        [SheetColumn("Student Scancode", ColumnNumber: 2, IsRequired = true)]
        public string StudentScancode { get; set; }

        [SheetColumn("Equipment Scancode", ColumnNumber: 3, IsRequired = true)]
        public string EquipmentScancode { get; set; }

        [SheetColumn("Loaned Date", ColumnNumber: 4, IsRequired = true)]
        public string LoanDateText { get; set; }

        public DateTime LoanedDate
        {
            get
            {
                return DateTime.Parse(LoanDateText, CultureInfo.DefaultThreadCurrentCulture);
            }

            set
            {
                LoanDateText = value.ToString(CultureInfo.DefaultThreadCurrentCulture);
            }
        }

        [SheetColumn("Returned Date", ColumnNumber: 5)]
        public string? ReturnedDateText { get; set; }

        public DateTime? ReturnedDate
        {
            get
            {
                return ReturnedDateText == null ? null : DateTime.Parse(ReturnedDateText, CultureInfo.DefaultThreadCurrentCulture);
            }

            set
            {
                ReturnedDateText = value.ToString();
            }
        }

        public string Range { get; set; }
    }
}
