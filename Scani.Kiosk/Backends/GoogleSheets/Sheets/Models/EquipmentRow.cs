using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Scani.Kiosk.Backends.GoogleSheets.Sheets.Models
{
    public class EquipmentRow : ISheetRow, IHaveScancode, IHaveFlexFields
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

        [Column("Name*", Order = 1)]
        [Required]
        public string Name { get; set; }

        [Column("Custom Scancode", Order = 2)]
        public string? CustomScancode { get; set; }

        [Column("Generated Scancode*", Order = 3)]
        public string GeneratedScancode { get; set; }

        public string Scancode => CustomScancode ?? GeneratedScancode;

        public string Range { get; set; }

        public IDictionary<string, string?> FlexFields { get; set; } = new Dictionary<string, string>();
    }
}