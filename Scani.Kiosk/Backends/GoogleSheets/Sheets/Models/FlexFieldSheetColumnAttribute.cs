using System.Diagnostics.CodeAnalysis;

namespace Scani.Kiosk.Backends.GoogleSheets.Sheets.Models
{
    [AttributeUsage(AttributeTargets.Property, Inherited = false, AllowMultiple = false)]
    sealed class FlexFieldSheetColumnAttribute : Attribute
    {
        private readonly int _rowNumber;
        private readonly int _columnNumber;

        public FlexFieldSheetColumnAttribute(int RowNumber, int ColumnNumber)
        {
            this._rowNumber = RowNumber;
            this._columnNumber = ColumnNumber;
        }

        public int RowNumber => _rowNumber;

        public int ColumnNumber => _columnNumber;

        public static bool TryGetFlexFlieldStartCellPosition<T>([NotNullWhen(true)] out (int RowNumber, int ColumnNumber)? firstFlexFieldCellPosition)
        {
            firstFlexFieldCellPosition = typeof(T)
                .GetProperties()
                .SelectMany(p => p.GetCustomAttributes(true).Where(a => a is FlexFieldSheetColumnAttribute))
                .Cast<FlexFieldSheetColumnAttribute>()
                .Select(ffc => ((int, int)?)(ffc.RowNumber, ffc.ColumnNumber))
                .SingleOrDefault();
            return firstFlexFieldCellPosition != null;
        }
    }
}
