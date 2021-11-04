namespace Scani.Kiosk.Backends.GoogleSheets.Sheets
{
    public enum KioskSheetReadErrorType
    {
        MissingFlexFields,
        TooManyFlexFields,
        MissingRequiredDataColumn,
        UnrecognisedDataColumn,
        DuplicateGeneratedScancode,
        InvalidGeneratedScancode,
        NonSequentialGeneratedScancode,
        MissingExpectedHeaderRows,
        EmptyFlexFieldHeader,
        DataRowMissingValues,
        DataRowExpectedValueMissing
    }

    public abstract class KioskSheetReadError
    {
        public string SheetName { get; }
        public KioskSheetReadErrorType ErrorType { get; }
        public string Message { get; }

        public KioskSheetReadError(KioskSheetReadErrorType errorType, string sheetName, string message)
        {
            this.SheetName = sheetName;
            this.ErrorType = errorType;
            this.Message = message;
        }
    }

    public class MissingFlexFields : KioskSheetReadError
    {
        public int RowNumber { get; init; }
        public int StartColumnNumber { get; init; }
        public MissingFlexFields(string sheetName, int row, int startCol)
            : base(KioskSheetReadErrorType.MissingFlexFields,
                   sheetName,
                   $"Expected flex fields in sheet {sheetName} but was unable to locate them on row {row} starting at column {startCol} in spreadsheet {sheetName}")
        {
            this.RowNumber = row;
            this.StartColumnNumber = startCol;
        }
    }

    public class TooManyFlexFields : KioskSheetReadError
    {
        public TooManyFlexFields(string sheetName, int flexFieldLimit, int foundFlexFieldCount)
            : base(KioskSheetReadErrorType.TooManyFlexFields,
                   sheetName,
                   $"The sheet {sheetName} has {foundFlexFieldCount} flex fields and thus exceeded the flex field limit of {flexFieldLimit} fields")
        { }
    }

    public class MissingRequiredDataColumn : KioskSheetReadError
    {
        public string Name { get; set; }
        public int RowNumber { get; init; }
        public int ColumnNumber { get; init; }
        public MissingRequiredDataColumn(string sheetName, string columnName, int row, int column)
            : base(KioskSheetReadErrorType.MissingRequiredDataColumn,
                   sheetName,
                   $"The sheet {sheetName} is missing the required colum {columnName} on row {row} and column {column}")
        {
            this.Name = columnName;
            this.ColumnNumber = column;
            this.RowNumber = row;
        }
    }

    public class MissingExpectedDataColumn : KioskSheetReadError
    {
        public string Name { get; set; }
        public int RowNumber { get; init; }
        public int ColumnNumber { get; init; }
        public MissingExpectedDataColumn(string sheetName, string columnName, int row, int column)
            : base(KioskSheetReadErrorType.MissingRequiredDataColumn,
                   sheetName,
                   $"The sheet {sheetName} is missing the colum {columnName} on row {row} and column {column}")
        {
            this.Name = columnName;
            this.ColumnNumber = column;
            this.RowNumber = row;
        }
    }

    public class NonSequentialGeneratedScancodes : KioskSheetReadError
    {
        public ulong ExpectedScancode { get; init; }
        public ulong ActualScancode { get; set; }
        public int RowNumber { get; init; }

        public NonSequentialGeneratedScancodes(string sheetName, int row, ulong expectedScancode, ulong actualScancode)
            : base(KioskSheetReadErrorType.NonSequentialGeneratedScancode,
                   sheetName,
                   $"The sheet {sheetName} on row {row} violated the expectation of sequential generated scancodes, expected row {row} to have a generated scancode of {expectedScancode} but found {actualScancode}")
        { }
    }

    public class MissingExpectedHeaderRows : KioskSheetReadError
    {
        public int ExpectedNumberOfHeaderRows { get; }

        public MissingExpectedHeaderRows(string sheetName, int expectedNumberOfHeaderRows, int foundHeaderRows)
            : base(KioskSheetReadErrorType.MissingExpectedHeaderRows,
                   sheetName,
                   $"Expected the sheet {sheetName} to have {expectedNumberOfHeaderRows} header rows but found {foundHeaderRows}")
        { }
    }

    public class EmptyFlexFieldHeader : KioskSheetReadError
    {
        public int RowNumber { get; init; }
        public int ColumnNumber { get; init; }
        public EmptyFlexFieldHeader(string sheetName, int emptyFlexHeaderRowNumber, int emptyFlexHeaderColumNumber)
            : base(KioskSheetReadErrorType.EmptyFlexFieldHeader,
                   sheetName,
                   $"The sheet {sheetName} had an empty flex header on row {emptyFlexHeaderRowNumber} and column {emptyFlexHeaderColumNumber} which results in ambiguous parsing and is thus disallowed")
        {
            this.RowNumber = emptyFlexHeaderRowNumber;
            this.ColumnNumber = emptyFlexHeaderColumNumber;
        }
    }

    public class UnrecognisedDataColumn : KioskSheetReadError
    {
        public string ColumnName { get; init; }
        public int ColumnNumber { get; init; }

        public UnrecognisedDataColumn(string sheetName, string columnName, int columnNumber)
            : base(KioskSheetReadErrorType.UnrecognisedDataColumn,
                   sheetName,
                   $"The sheet {sheetName} had an unrecognised column '{columnName}' in column {columnNumber}")
        {
            this.ColumnName = columnName;
            this.ColumnNumber = columnNumber;
        }
    }

    public class DataRowMissingValues : KioskSheetReadError
    {
        public int RowNumber { get; init; }
        public int ExpectedColumnCount { get; init; }
        public int ActualColumnCount { get; init; }

        public DataRowMissingValues(string sheetName, int rowNumber, int expectedColumnCount, int actualColumnCount)
            : base(KioskSheetReadErrorType.DataRowMissingValues,
                   sheetName,
                   $"The sheet {sheetName}'s data row on row {rowNumber} was expected to contain {expectedColumnCount} values but contained {actualColumnCount}")
        {
            this.RowNumber = rowNumber;
            this.ExpectedColumnCount = expectedColumnCount;
            this.ActualColumnCount = actualColumnCount;
        }
    }

    public class DataRowExpectedValueMissing : KioskSheetReadError
    {
        public string ColumnName { get; init; }
        public int RowNumber { get; init; }
        public int ColumnNumber { get; init; }
        
        public DataRowExpectedValueMissing(string sheetName, string columnName, int rowNumber, int columnNumber)
            : base(KioskSheetReadErrorType.DataRowExpectedValueMissing,
                   sheetName,
                   $"The sheet {sheetName} is missing a value for column '{columnName}' on row {rowNumber} at column {columnNumber}")
        {
            this.ColumnName = columnName;
            this.RowNumber = rowNumber;
            this.ColumnNumber = columnNumber;
        }
    }

    public class InvalidGeneratedScancode : KioskSheetReadError
    {
        public InvalidGeneratedScancode(string sheetName, string generatedScancode, int rowNumber)
            : base(KioskSheetReadErrorType.InvalidGeneratedScancode,
                   sheetName,
                   $"The sheet {sheetName} has an invalid generated scancode {generatedScancode} on row {rowNumber} - generated scancodes must be numeric and in ascending order")
        { }
    }

    public class DuplicateGeneratedScancode : KioskSheetReadError
    {
        public DuplicateGeneratedScancode(string sheetName, string generatedScancode, int rowNumber)
            : base(KioskSheetReadErrorType.InvalidGeneratedScancode,
                   sheetName,
                   $"The sheet {sheetName} has a generated scancode {generatedScancode} on row {rowNumber} which is already in use")
        { }
    }
}
