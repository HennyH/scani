using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Scani.Kiosk.Backends.GoogleSheets.Sheets.Models
{
    [GoogleSheet("Users", HeaderCount: 2)]
    public class UserRow : ISheetRow, IHaveScancodes, IHaveFlexFields
    {
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        private UserRow()
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        { }

        public UserRow(string displayName, string fullName, string email, string generatedScancode, string range, IDictionary<string, string?> flexFields)
        {
            this.DisplayName = displayName;
            this.FullName = fullName;
            this.Email = email;
            this.GeneratedScancode = generatedScancode;
            this.Range = range;
            this.FlexFields = flexFields;
        }

        [SheetColumn("Display Name*", ColumnNumber: 1, IsRequired = true)]
        public string DisplayName { get; set; }

        [SheetColumn("Full Name*", ColumnNumber: 2, IsRequired = true)]
        public string FullName { get; set; }

        [SheetColumn("Email", ColumnNumber: 3)]
        public string? Email { get; set; }

        [SheetColumn("Custom Scancode", ColumnNumber: 4)]
        public string? CustomScancode { get; set; }

        [SheetColumn("Generated Scancode*", ColumnNumber: 5, IsRequired = true)]
        public string GeneratedScancode { get; set; }

        [SheetColumn("Deactive User? (Y/N - Default N)", ColumnNumber: 6)]
        public string? DeactiveUserText { get; set; }

        [SheetColumn("Is Admin? (Y/N - Default N)", ColumnNumber: 7)]
        public string? IsAdminText { get; set; }

        [FlexFieldSheetColumn(RowNumber: 2, ColumnNumber: 8)]
        public IDictionary<string, string?> FlexFields { get; } = new Dictionary<string, string?>();

        public bool IsActiveUser => string.IsNullOrWhiteSpace(DeactiveUserText) || !(DeactiveUserText == "Y" || DeactiveUserText == "Yes" || DeactiveUserText == "True" || DeactiveUserText == "true");

        public bool IsAdminUser => IsAdminText == "Y" || IsAdminText == "Yes" || IsAdminText == "True" || IsAdminText == "true";

        public string PrimaryScancode => string.IsNullOrWhiteSpace(CustomScancode) ? GeneratedScancode : CustomScancode;

        public HashSet<string> Scancodes => new[] { Email, CustomScancode, GeneratedScancode }
            .Where(sc => !string.IsNullOrWhiteSpace(sc))
            .Select(sc => sc!)
            .ToHashSet();

        public bool HasScancode(string scancode) => Scancodes.Contains(scancode);

        public string Range { get; set; }
    }
}
