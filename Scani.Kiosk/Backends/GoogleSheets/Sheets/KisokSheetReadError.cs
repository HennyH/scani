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

    public abstract record KioskSheetReadError(KioskSheetReadErrorType ErrorType, string SheetName, string Message);

    public record MissingFlexFields(string SheetName, int Row, int StartCol)
        : KioskSheetReadError(KioskSheetReadErrorType.MissingFlexFields, SheetName, $"Expected flex fields in sheet {SheetName} but was unable to locate them on row {Row} starting at column {StartCol} in spreadsheet {SheetName}");

    public record TooManyFlexFields(string SheetName, int FlexFieldLimit, int FoundFlexFieldCount)
        : KioskSheetReadError(KioskSheetReadErrorType.TooManyFlexFields, SheetName, $"The sheet {SheetName} has {FoundFlexFieldCount} flex fields and thus exceeded the flex field limit of {FlexFieldLimit} fields");

    public record MissingRequiredDataColumn(string SheetName, string ColumnName, int Row, int Column)
        : KioskSheetReadError(KioskSheetReadErrorType.MissingRequiredDataColumn, SheetName, $"The sheet {SheetName} is missing the required colum {ColumnName} on row {Row} and column {Column}");

    public record MissingExpectedDataColumn(string SheetName, string ColumnName, int Row, int Column)
        : KioskSheetReadError(KioskSheetReadErrorType.MissingRequiredDataColumn, SheetName, $"The sheet {SheetName} is missing the colum {ColumnName} on row {Row} and column {Column}");

    public record NonSequentialGeneratedScancodes(string SheetName, int Row, ulong ExpectedScancode, ulong ActualScancode)
        : KioskSheetReadError(KioskSheetReadErrorType.NonSequentialGeneratedScancode, SheetName, $"The sheet {SheetName} on row {Row} violated the expectation of sequential generated scancodes, expected row {Row} to have a generated scancode of {ExpectedScancode} but found {ActualScancode}");

    public record MissingExpectedHeaderRows(string SheetName, int ExpectedNumberOfHeaderRows, int FoundHeaderRows)
        : KioskSheetReadError(KioskSheetReadErrorType.MissingExpectedHeaderRows, SheetName, $"Expected the sheet {SheetName} to have {ExpectedNumberOfHeaderRows} header rows but found {FoundHeaderRows}");

    public record EmptyFlexFieldHeader(string SheetName, int EmptyFlexHeaderRowNumber, int EmptyFlexHeaderColumNumber)
        : KioskSheetReadError(KioskSheetReadErrorType.EmptyFlexFieldHeader, SheetName, $"The sheet {SheetName} had an empty flex header on row {EmptyFlexHeaderRowNumber} and column {EmptyFlexHeaderColumNumber} which results in ambiguous parsing and is thus disallowed");

    public record UnrecognisedDataColumn(string SheetName, string ColumnName, int ColumnNumber)
        : KioskSheetReadError(KioskSheetReadErrorType.UnrecognisedDataColumn, SheetName, $"The sheet {SheetName} had an unrecognised column '{ColumnName}' in column {ColumnNumber}");

    public record DataRowMissingValues(string SheetName, int RowNumber, int ExpectedColumnCount, int ActualColumnCount)
        : KioskSheetReadError(KioskSheetReadErrorType.DataRowMissingValues, SheetName, $"The sheet {SheetName}'s data row on row {RowNumber} was expected to contain {ExpectedColumnCount} values but contained {ActualColumnCount}");

    public record DataRowExpectedValueMissing(string SheetName, string ColumnName, int RowNumber, int ColumnNumber)
        : KioskSheetReadError(KioskSheetReadErrorType.DataRowExpectedValueMissing, SheetName, $"The sheet {SheetName} is missing a value for column '{ColumnName}' on row {RowNumber} at column {ColumnNumber}");

    public record InvalidGeneratedScancode(string SheetName, string GeneratedScancode, int RowNumber)
        : KioskSheetReadError(KioskSheetReadErrorType.InvalidGeneratedScancode, SheetName, $"The sheet {SheetName} has an invalid generated scancode {GeneratedScancode} on row {RowNumber} - generated scancodes must be numeric and in ascending order");

    public record DuplicateGeneratedScancode(string SheetName, string GeneratedScancode, int RowNumber)
        : KioskSheetReadError(KioskSheetReadErrorType.InvalidGeneratedScancode, SheetName, $"The sheet {SheetName} has a generated scancode {GeneratedScancode} on row {RowNumber} which is already in use");

    public record DuplicateCustomScancode(string SheetName, string CustomScancode, int RowNumber)
        : KioskSheetReadError(KioskSheetReadErrorType.InvalidGeneratedScancode, SheetName, $"The sheet {SheetName} has a custom scancode {CustomScancode} on row {RowNumber} which is already in use");

    public record DataRowInvalidValue(string SheetName, string ColumnName, string Value, int RowNumber, int ColumnNumber)
        : KioskSheetReadError(KioskSheetReadErrorType.DataRowExpectedValueMissing, SheetName, $"The sheet {SheetName} was not able to understand the value in column '{ColumnName}' on row {RowNumber} at column {ColumnNumber}, that value was {Value}");
}
