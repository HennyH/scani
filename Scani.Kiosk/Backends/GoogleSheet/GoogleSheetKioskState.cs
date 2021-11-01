namespace Scani.Kiosk.Backends.GoogleSheet
{
    public class GoogleSheetKioskState
    {
        public ICollection<Student> Students { get; set; } = new List<Student>();
        public ICollection<EquipmentItem> EquipmentItems { get; set; } = new List<EquipmentItem>();
        public ICollection<Setting> Settings { get; set; } = new List<Setting>();

        public ICollection<string> ParseErrors { get; set; } = new List<string>();
        public ICollection<string> ParseWarnings { get; set; } = new List<string>();

        public record Student
        {
            public Student(string displayName, string fullName, string email, string generatedScancode)
            {
                DisplayName = displayName;
                FullName = fullName;
                Email = email;
                GeneratedScancode = generatedScancode;
            }

            public string DisplayName { get; set; }
            public string FullName { get; set; }
            public string Email { get; set; }
            public string? CustomScancode { get; set; }
            public string GeneratedScancode { get; set; }
            public IDictionary<string, string?> CustomFields { get; set; } = new Dictionary<string, string?>();
        }

        public record EquipmentItem
        {
            public EquipmentItem(string name, string generatedScancode)
            {
                Name = name;
                GeneratedScancode = generatedScancode;
            }

            public string Name { get; set; }
            public string? CustomScancode { get; set; }
            public string GeneratedScancode { get; set; }
            public IDictionary<string, string?> CustomFields { get; set; } = new Dictionary<string, string?>();
        }

        public record Setting
        {
            public Setting(string name, string description, string value)
            {
                Name = name;
                Description = description; 
                Value = value;
            }

            public string Name { get; set; }
            public string Description { get; set; }
            public string Value { get; set; }
        }
    }
}
