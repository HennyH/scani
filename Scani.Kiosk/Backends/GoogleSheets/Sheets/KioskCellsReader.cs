using Scani.Kiosk.Backends.GoogleSheets.Sheets.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Reflection;
using System.Text.RegularExpressions;

namespace Scani.Kiosk.Backends.GoogleSheets.Sheets
{
    public static class KioskCellsReader
    {
        private const int MAX_FLEX_FIELDS = 20;

        public static KioskSheetReadResult<T> ParseCells<T>(ILogger logger, string sheetName, IList<IList<object>> cells, int numberOfHeaderRows, int dataHeaderRowNumber, (int RowNumber, int ColNumber)? firstFlexFieldCell = null)
            where T : ISheetRow
        {
            var expectedColumns = GetExpectedColumns<T>();
            var maxExpectedColumnNumber = expectedColumns.Max(ec => ec.ColumnNumber);
            var result = new KioskSheetReadResult<T>()
            {
                SheetName = sheetName,
                NextDataRowNumber = Math.Max(dataHeaderRowNumber + 1, cells.Count + 1),
                DataRowNumberToRange = rowNumber => $"{sheetName}!A{rowNumber}:{GetExcelColumnName(maxExpectedColumnNumber)}{rowNumber}"
            };

            if (cells.Count < numberOfHeaderRows)
            {
                result.Errors.Add(new MissingExpectedHeaderRows(sheetName, numberOfHeaderRows, cells.Count));
            }

            if (firstFlexFieldCell.HasValue && (firstFlexFieldCell.Value.RowNumber > cells.Count || firstFlexFieldCell.Value.ColNumber < cells[firstFlexFieldCell.Value.RowNumber - 1].Count))
            {
                result.Errors.Add(new MissingFlexFields(sheetName, firstFlexFieldCell.Value.RowNumber, firstFlexFieldCell.Value.ColNumber));
            }

            /* After checking for missing flex fields and header rows, if we are missing the expected
             * headers rows we're just going to bail out here.
             */
            if (result.Errors.Any(e => e is MissingExpectedHeaderRows))
            {
                return result;
            }

            /* If we expected to find flex fields, and they aren't missing lets try read them out */
            if (firstFlexFieldCell.HasValue && !result.Errors.Any(e => e is MissingFlexFields))
            {
                var (flexRowNumber, firstFlexColNumber) = firstFlexFieldCell.Value;
                var flexRow = cells[flexRowNumber - 1];

                if (flexRow.Count > MAX_FLEX_FIELDS)
                {
                    result.Errors.Add(new TooManyFlexFields(sheetName, MAX_FLEX_FIELDS, flexRow.Count));
                }

                var maxLastFlexColNumber = Math.Max(flexRow.Count, firstFlexColNumber + MAX_FLEX_FIELDS);
                for (int flexColNumber = firstFlexColNumber; flexColNumber <= maxLastFlexColNumber && flexColNumber < flexRow.Count; flexColNumber++)
                {
                    var flexHeader = flexRow[flexColNumber - 1] as string;
                    /* An empty flex header is considered fatal in terms of continuing to search for other flex fields */
                    if (string.IsNullOrWhiteSpace(flexHeader))
                    {
                        result.Errors.Add(new EmptyFlexFieldHeader(sheetName, flexRowNumber, flexColNumber));
                        break;
                    }

                    result.FlexFieldNames.Add(flexHeader);
                }
            }

            /* Here we read the data row headers and check they're all good */
            if (cells.Count > dataHeaderRowNumber)
            {
                var dataHeaderRow = cells[dataHeaderRowNumber - 1];

                foreach (var (expectedColumnNumber, expectedColumnName, isRequired, _) in expectedColumns)
                {
                    if (expectedColumnNumber > dataHeaderRow.Count)
                    {
                        if (isRequired)
                        {
                            result.Errors.Add(new MissingRequiredDataColumn(sheetName, expectedColumnName, dataHeaderRowNumber, expectedColumnNumber));
                        }
                        else
                        {
                            result.Errors.Add(new MissingExpectedDataColumn(sheetName, expectedColumnName, dataHeaderRowNumber, expectedColumnNumber));
                        }
                    }
                    else
                    {
                        var actualName = dataHeaderRow[expectedColumnNumber - 1] as string;

                        if (string.IsNullOrEmpty(actualName) || !string.Equals(actualName, expectedColumnName, StringComparison.OrdinalIgnoreCase))
                        {
                            if (isRequired)
                            {
                                result.Errors.Add(new MissingRequiredDataColumn(sheetName, expectedColumnName, dataHeaderRowNumber, expectedColumnNumber));
                            }
                            else
                            {
                                result.Errors.Add(new MissingExpectedDataColumn(sheetName, expectedColumnName, dataHeaderRowNumber, expectedColumnNumber));
                            }
                        }

                        if (actualName != null && !expectedColumns.Any(c => c.ColumnName.Equals(actualName, StringComparison.OrdinalIgnoreCase)))
                        {
                            result.Errors.Add(new UnrecognisedDataColumn(sheetName, actualName, expectedColumnNumber));
                        }
                    }
                }

                /* If the data headers are whacked we bail out */
                if (result.Errors.Any(e => e is MissingExpectedDataColumn || e is MissingRequiredDataColumn || e is UnrecognisedDataColumn))
                {
                    return result;
                }

                /* Finally it is time to read the data rows into objects */
                ulong? firstGeneratedScancode = null;
                HashSet<string> generatedScancodes = new HashSet<string>();
                HashSet<string> scancodes = new HashSet<string>();
                for (int dataRowNumber = dataHeaderRowNumber + 1; dataRowNumber <= cells.Count; dataRowNumber++)
                {
                    bool isValidRow = true;
                    var dataRow = cells[dataRowNumber - 1];

                    if (dataRow.Count < expectedColumns.Where(c => c.IsRequired).Count())
                    {
                        result.Errors.Add(new DataRowMissingValues(sheetName, dataRowNumber, maxExpectedColumnNumber, dataRow.Count));
                        isValidRow = false;
                    }
                    else
                    {
                        var item = (T)Activator.CreateInstance(typeof(T), nonPublic: true)!;
                        item.Range = result.DataRowNumberToRange(dataRowNumber);

                        foreach (var (columnNumber, columnName, isRequired, property) in expectedColumns)
                        {
                            if (columnNumber > dataRow.Count)
                            {
                                if (isRequired)
                                {
                                    result.Errors.Add(new DataRowExpectedValueMissing(sheetName, columnName, dataRowNumber, columnNumber));
                                    isValidRow = false;
                                }
                            }
                            else
                            {
                                var value = dataRow[columnNumber - 1] as string;
                                if (string.IsNullOrWhiteSpace(value) && isRequired)
                                {
                                    result.Errors.Add(new DataRowExpectedValueMissing(sheetName, columnName, dataRowNumber, columnNumber));
                                    isValidRow = false;
                                }
                                property.SetValue(item, value ?? string.Empty);
                            }
                        }

                        if (firstFlexFieldCell != null && result.FlexFieldNames.Any() && (item is IHaveFlexFields itemWithFlexFields))
                        {
                            var flexFields = new Dictionary<string, string?>();
                            var firstFlexColNumber = firstFlexFieldCell.Value.ColNumber;
                            for (int flexColumnOffset = 0; flexColumnOffset < result.FlexFieldNames.Count; flexColumnOffset++)
                            {
                                var flexFieldName = result.FlexFieldNames[flexColumnOffset];
                                var flexColumnNumber = firstFlexColNumber + flexColumnOffset;
                                if (flexColumnNumber <= dataRow.Count)
                                {
                                    flexFields.Add(flexFieldName, dataRow[flexColumnNumber - 1] as string);
                                }
                                else
                                {
                                    flexFields.Add(flexFieldName, null);
                                }
                            }

                            itemWithFlexFields.FlexFields = flexFields;
                        }

                        if (item is IHaveScancode itemWithScancode)
                        {
                            var generatedScancode = itemWithScancode.GeneratedScancode;
                            var customScancode = itemWithScancode.CustomScancode;
                            ulong generatedScancodeAsNumber = 0;

                            if (string.IsNullOrWhiteSpace(generatedScancode) || !Regex.IsMatch(generatedScancode, @"^\d+$"))
                            {
                                result.Errors.Add(new InvalidGeneratedScancode(sheetName, generatedScancode, dataRowNumber));
                                isValidRow = false;
                            }
                            else if (firstGeneratedScancode.HasValue
                                     && ulong.TryParse(generatedScancode, out generatedScancodeAsNumber)
                                     && generatedScancodeAsNumber != firstGeneratedScancode.Value + (ulong)(dataRowNumber - (dataHeaderRowNumber + 1)))
                            {
                                result.Errors.Add(new NonSequentialGeneratedScancodes(sheetName, dataRowNumber, firstGeneratedScancode.Value + (ulong)(dataRowNumber - dataHeaderRowNumber), generatedScancodeAsNumber));
                                isValidRow = false;
                            }
                            else if (generatedScancodes.Contains(generatedScancode))
                            {
                                result.Errors.Add(new DuplicateGeneratedScancode(sheetName, generatedScancode, dataRowNumber));
                                isValidRow = false;
                            }
                            else if (itemWithScancode.CustomScancode != null && scancodes.Contains(itemWithScancode.CustomScancode))
                            {
                                result.Errors.Add(new DuplicateCustomScancode(sheetName, itemWithScancode.CustomScancode, dataRowNumber));
                                isValidRow = false;
                            }

                            if (firstGeneratedScancode == null && ulong.TryParse(generatedScancode, out generatedScancodeAsNumber))
                            {
                                firstGeneratedScancode = generatedScancodeAsNumber;
                            }

                            if (!string.IsNullOrWhiteSpace(generatedScancode))
                            {
                                generatedScancodes.Add(generatedScancode);
                                scancodes.Add(generatedScancode);
                            }

                            if (!string.IsNullOrWhiteSpace(customScancode))
                            {
                                scancodes.Add(itemWithScancode.Scancode);
                            }
                        }

                        if (isValidRow)
                        {
                            result.Rows.Add(item);
                        }
                    }
                }
            }

            result.Ok = true;
            return result;
        }

        /// <summary>
        /// Converts a 1 based column number into an excel column name.
        /// </summary>
        /// <remarks>
        /// Taken from https://stackoverflow.com/a/182924
        /// </remarks>
        private static string GetExcelColumnName(int columnNumber)
        {
            string columnName = "";

            while (columnNumber > 0)
            {
                int modulo = (columnNumber - 1) % 26;
                columnName = Convert.ToChar('A' + modulo) + columnName;
                columnNumber = (columnNumber - modulo) / 26;
            }

            return columnName;
        }

        private static IEnumerable<(int ColumnNumber, string ColumnName, bool IsRequired, PropertyInfo Property)> GetExpectedColumns<T>()
        {
            var columns = new List<(int Order, string ColumnName, bool IsRequired, PropertyInfo Property)>();
            foreach (var property in typeof(T).GetProperties())
            {
                var attributes = property.GetCustomAttributes(true);
                var columnAttribute = attributes
                    .Where(a => a is ColumnAttribute)
                    .Cast<ColumnAttribute>()
                    .FirstOrDefault();
                if (columnAttribute == null)
                {
                    continue;
                }
                else if (string.IsNullOrWhiteSpace(columnAttribute.Name))
                {
                    throw new ArgumentException("Any [Column] declared on a sheet row must have the Name defined.", nameof(T));
                }
                var isRequired = attributes.Any(a => a is RequiredAttribute);
                columns.Add((columnAttribute.Order, columnAttribute.Name!, isRequired, property));
            }

            if (columns.Any(c => c.Order < 0)) throw new ArithmeticException($"{typeof(T)} had a [Column] with an Order < 0");

            return columns
                .OrderBy(c => c.Order)
                .Select((c, i) => (i + 1, c.ColumnName, c.IsRequired, c.Property)).ToList();
        }
    }
}
