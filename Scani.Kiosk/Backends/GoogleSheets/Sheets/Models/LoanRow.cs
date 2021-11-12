using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Globalization;

namespace Scani.Kiosk.Backends.GoogleSheets.Sheets.Models
{
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
            : this(studentScancode, equipmentScancode, loanDate.ToString(CultureInfo.InvariantCulture), loanSheet)
        { }

        [Column("ID", Order = 1)]
        [Required]
        public string IdText { get; set; }

        public Guid Id => Guid.Parse(IdText);

        [Column("Student Scancode", Order = 2)]
        [Required]
        public string StudentScancode { get; set; }

        [Column("Equipment Scancode", Order = 3)]
        [Required]
        public string EquipmentScancode { get; set; }

        [Column("Loaned Date", Order = 4)]
        [Required]
        public string LoanDateText { get; set; }

        public DateTime LoanedDate
        {
            get
            {
                return DateTime.Parse(LoanDateText, CultureInfo.InvariantCulture);
            }

            set
            {
                LoanDateText = value.ToString(CultureInfo.InvariantCulture);
            }
        }

        [Column("Returned Date", Order = 5)]
        public string? ReturnedDateText { get; set; }

        public DateTime? ReturnedDate
        {
            get
            {
                return ReturnedDateText == null ? null : DateTime.Parse(ReturnedDateText, CultureInfo.InvariantCulture);
            }

            set
            {
                ReturnedDateText = value.ToString();
            }
        }

        public string Range { get; set; }
    }
}
