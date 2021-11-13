using System.Reflection;

namespace Scani.Kiosk.Backends.GoogleSheets.Sheets.Models
{
    [AttributeUsage(AttributeTargets.Property, Inherited = false, AllowMultiple = false)]
    sealed class SheetColumnAttribute : Attribute
    {
        private readonly string _columnName;
        private readonly int _columnNumber;

        public SheetColumnAttribute(string ColumnName, int ColumnNumber)
        {
            this._columnName = ColumnName;
            this._columnNumber = ColumnNumber;
        }

        public string ColumnName => _columnName;

        public int ColumnNumber => _columnNumber;

        public bool IsRequired { get; set; }

        public static IEnumerable<(int ColumnNumber, string ColumnName, bool IsRequired, PropertyInfo Property)> GetExpectedColumns<T>()
        {
            var columns = new List<(int ColumnNumber, string ColumnName, bool IsRequired, PropertyInfo Property)>();
            foreach (var property in typeof(T).GetProperties())
            {
                var attributes = property.GetCustomAttributes(true);
                var sheetColumnAttribute = attributes
                    .Where(a => a is SheetColumnAttribute)
                    .Cast<SheetColumnAttribute>()
                    .FirstOrDefault();
                if (sheetColumnAttribute == null)
                {
                    continue;
                }
                else if (string.IsNullOrWhiteSpace(sheetColumnAttribute.ColumnName))
                {
                    throw new ArgumentException("Any [Column] declared on a sheet row must have the Name defined.", nameof(T));
                }
                columns.Add((sheetColumnAttribute.ColumnNumber, sheetColumnAttribute.ColumnName, sheetColumnAttribute.IsRequired, property));
            }

            if (columns.Any(c => c.ColumnNumber < 0)) throw new ArithmeticException($"{typeof(T)} had a [SheetColumn] with an ColumnNumber < 0");

            return columns.OrderBy(c => c.ColumnNumber).ToList();
        }
    }
}
