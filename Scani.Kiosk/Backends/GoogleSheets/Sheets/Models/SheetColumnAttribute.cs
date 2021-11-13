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
    }
}
