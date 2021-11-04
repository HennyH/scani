using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Scani.Kiosk.Backends.GoogleSheets.Sheets.Models
{
    public class StudentRow : ISheetRow, IHaveScancode, IHaveFlexFields
    {
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        private StudentRow()
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        { }

        public StudentRow(string displayName, string fullName, string email, string generatedScancode, string range, IDictionary<string, string?> flexFields)
        {
            this.DisplayName = displayName;
            this.FullName = fullName;
            this.Email = email;
            this.GeneratedScancode = generatedScancode;
            this.Range = range;
            this.FlexFields = flexFields;
        }

        [Column("Display Name*", Order = 1)]
        [Required]
        public string DisplayName { get; set; }

        [Column("Full Name*", Order = 2)]
        [Required]
        public string FullName { get; set; }

        [Column("Email", Order = 3)]
        public string? Email { get; set; }

        [Column("Custom Scancode", Order = 4)]
        public string? CustomScancode { get; set; }

        [Column("Generated Scancode*", Order = 5)]
        [Required]
        public string GeneratedScancode { get; set; }

        public string Range { get; set; }

        public IDictionary<string, string?> FlexFields { get; set; } = new Dictionary<string, string>();
    }
}
