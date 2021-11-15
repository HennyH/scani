namespace Scani.Kiosk.Backends.GoogleSheets.Sheets.Models
{
    [GoogleSheet("Equipment", HeaderCount: 2)]
    public class EquipmentRow : ISheetRow, IHaveScancodes, IHaveFlexFields
    {
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        private EquipmentRow()
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        { }

        public EquipmentRow(string name, string generatedScancode, string range, IDictionary<string, string?> flexFields)
        {
            this.Name = name;
            this.GeneratedScancode = generatedScancode;
            this.Range = range;
            this.FlexFields = flexFields;
        }

        [SheetColumn("Name*", ColumnNumber: 1, IsRequired = true)]
        public string Name { get; set; }

        [SheetColumn("Custom Scancode", ColumnNumber: 2)]
        public string? CustomScancode { get; set; }

        [SheetColumn("Generated Scancode*", ColumnNumber: 3, IsRequired = true)]
        public string GeneratedScancode { get; set; }

        public string Range { get; set; }

        [FlexFieldSheetColumn(RowNumber: 2, ColumnNumber: 4)]
        public IDictionary<string, string?> FlexFields { get; } = new Dictionary<string, string?>();

        public string DefaultScancode => CustomScancode ?? GeneratedScancode;
        public ICollection<string> Scancodes
        {
            get
            {
                var scancodes = new List<string> { GeneratedScancode };
                if (!string.IsNullOrWhiteSpace(CustomScancode))
                {
                    scancodes.Add(CustomScancode);
                }
                return scancodes;
            }
        }

        public bool HasScancode(string scancode) => Scancodes.Any(s => s.Equals(scancode, StringComparison.OrdinalIgnoreCase));
    }
}