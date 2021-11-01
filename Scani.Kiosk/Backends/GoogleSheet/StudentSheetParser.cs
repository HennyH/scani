using OfficeOpenXml;
using static Scani.Kiosk.Backends.GoogleSheet.GoogleSheetKioskState;

namespace Scani.Kiosk.Backends.GoogleSheet
{
    public static class StudentSheetParser
    {
        private const int CUSTOM_FIELD_HEADER_ROW = 2;
        private const int FIRST_CUSTOM_FIELD_HEADER_COL = 6;
        private const int FIRST_DATA_ROW = 2;
        private const int LAST_DATA_COL = 5;

        public static SheetParseResult<Student> ParseStudentsFromWorksheet(ILogger logger, ExcelWorksheet? worksheet)
        {
            var result = new SheetParseResult<Student>();

            if (worksheet == null) return result;

            var customFieldNames = new HashSet<string>();
            for (int col = FIRST_CUSTOM_FIELD_HEADER_COL; true; col++)
            {
                try
                {
                    var cell = worksheet.Cells[CUSTOM_FIELD_HEADER_ROW, col];
                    var name = cell.GetValue<string>();
                    if (string.IsNullOrWhiteSpace(name))
                    {
                        break;
                    }
                    customFieldNames.Add(name);
                }
                catch (Exception error)
                {
                    logger.LogError(error, "Error when detecting which custom fields were present in student spreadsheet");
                    result.Errors.Add($"Error when detecting which custom fields were present in student spreadsheet: {error.Message}");
                    break;
                }
            }

            for (int row = FIRST_DATA_ROW + 1; row < worksheet.Dimension.Rows; row++)
            {
                var stop = false;
                try
                {
                    var isRowEmpty = true;
                    for (int col = 1; isRowEmpty && col <= LAST_DATA_COL; col++)
                    {
                        isRowEmpty &= string.IsNullOrWhiteSpace(worksheet.Cells[row, col].GetValue<string?>());
                    }

                    if (isRowEmpty)
                    {
                        stop = true;
                        logger.LogTrace("Stopped looking for students due to row {} being empty", row);
                    }
                }
                catch (Exception error)
                {
                    logger.LogError(error, "Error occured when trying to determine if the row {} is empty in the student sheet", row);
                }

                if (stop)
                {
                    break;
                }

                try
                {
                    var fullName = worksheet.Cells[row, 2].GetValue<string>();
                    if (string.IsNullOrWhiteSpace(fullName))
                    {
                        result.Errors.Add($"No full name entered for student on row {row}");
                        logger.LogError("No full name entered for student on row {}", row);
                        continue;
                    }

                    var displayName = worksheet.Cells[row, 1].GetValue<string>();
                    if (string.IsNullOrWhiteSpace(displayName))
                    {
                        result.Warnings.Add($"No display name entered for student on row {row} defaulting to using full name");
                        logger.LogWarning("No display name entered for student on row {} defaulting to using full name", row);
                        displayName = fullName;
                    }

                    var email = worksheet.Cells[row, 3].GetValue<string>();
                    if (string.IsNullOrWhiteSpace(email))
                    {
                        result.Warnings.Add($"No email entered for student '{fullName}' on row {row}");
                        logger.LogWarning("No email entered for student on row {}", row);
                    }

                    var customScancode = worksheet.Cells[row, 4].GetValue<string?>();
                    var generatedScancode = worksheet.Cells[row, 5].GetValue<string>();
                    if (string.IsNullOrWhiteSpace(generatedScancode))
                    {
                        result.Errors.Add($"No generated scancode present for student '{fullName}' on row {row}");
                        logger.LogError("No generated scancode present for student '{}' on row {}", fullName, row);
                    }

                    if (string.IsNullOrWhiteSpace(customScancode) && string.IsNullOrWhiteSpace(generatedScancode))
                    {
                        result.Errors.Add($"No custom or generated scancode present for student '{fullName}' on row {row}");
                        logger.LogError("No custom or generated scancode present for student '{}' on row {}", fullName, row);
                        continue;
                    }

                    var customFields = customFieldNames.ToDictionary(n => n, _ => (string?)null);
                    foreach (var (i, fieldName) in customFieldNames.Select((n, i) => (i, n)))
                    {
                        try
                        {
                            customFields[fieldName] = worksheet.Cells[row, FIRST_CUSTOM_FIELD_HEADER_COL + i].GetValue<string>();
                        }
                        catch (Exception error)
                        {
                            result.Errors.Add($"Error attempting to retreive custom field '{fieldName}' for row {row} at column {FIRST_CUSTOM_FIELD_HEADER_COL + i}");
                            logger.LogError(error, "Error attempting to retreive custom field '{}' for row {} at column {}", fieldName, row, FIRST_CUSTOM_FIELD_HEADER_COL + i);
                            continue;
                        }
                    }

                    result.Values.Add(new Student(displayName, fullName, email, generatedScancode)
                    {
                        CustomScancode = customScancode,
                        CustomFields = customFields
                    });
                }
                catch (Exception error)
                {
                    logger.LogError(error, "Error when attempting to read data row from student spreadsheet on row {}", row);
                    result.Errors.Add($"Error when attempting to read data row from student spreadsheet on row {row}: {error.Message}");
                    continue;
                }
            }

            return result;
        }
    }
}
