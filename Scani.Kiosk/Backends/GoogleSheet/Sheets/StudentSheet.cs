using OfficeOpenXml;
using static Scani.Kiosk.Backends.GoogleSheet.SynchronizedGoogleSheetKioskState;

namespace Scani.Kiosk.Backends.GoogleSheet
{
    public static class StudentSheet
    {
        private const int CUSTOM_FIELD_HEADER_ROW = 1;
        private const int FIRST_CUSTOM_FIELD_HEADER_COL = 5;
        private const int FIRST_DATA_ROW = 2;
        private const int LAST_DATA_COL = 4;

        public static SheetReadResult<Student> ReadStudents(ILogger logger, IList<IList<object>> cells)
        {
            var result = new SheetReadResult<Student>();

            var customFieldNames = new HashSet<string>();
            for (int col = FIRST_CUSTOM_FIELD_HEADER_COL; col < cells[CUSTOM_FIELD_HEADER_ROW].Count; col++)
            {
                try
                {
                    var name = cells[CUSTOM_FIELD_HEADER_ROW][col] as string;
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

            for (int row = FIRST_DATA_ROW; row < cells.Count; row++)
            {
                var stop = false;
                try
                {
                    var isRowEmpty = true;
                    for (int col = 0; isRowEmpty && col <= LAST_DATA_COL; col++)
                    {
                        isRowEmpty &= string.IsNullOrWhiteSpace(cells[row][col] as string);
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
                    var fullName = cells[row][0] as string;
                    if (string.IsNullOrWhiteSpace(fullName))
                    {
                        result.Errors.Add($"No full name entered for student on row {row}");
                        logger.LogError("No full name entered for student on row {}", row);
                        continue;
                    }

                    var displayName = cells[row][1] as string;
                    if (string.IsNullOrWhiteSpace(displayName))
                    {
                        result.Warnings.Add($"No display name entered for student on row {row} defaulting to using full name");
                        logger.LogWarning("No display name entered for student on row {} defaulting to using full name", row);
                        displayName = fullName;
                    }

                    var email = cells[row][2] as string;
                    if (string.IsNullOrWhiteSpace(email))
                    {
                        result.Warnings.Add($"No email entered for student '{fullName}' on row {row}");
                        logger.LogWarning("No email entered for student on row {}", row);
                    }

                    var customScancode = cells[row][3] as string;
                    var generatedScancode = cells[row][4] as string;
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
                        if (cells[row].Count - 1 <= FIRST_CUSTOM_FIELD_HEADER_COL + i)
                        {
                            break;
                        }

                        try
                        {
                            customFields[fieldName] = cells[row][FIRST_CUSTOM_FIELD_HEADER_COL + i] as string;
                        }
                        catch (Exception error)
                        {
                            result.Errors.Add($"Error attempting to retreive custom field '{fieldName}' for row {row} at column {FIRST_CUSTOM_FIELD_HEADER_COL + i}");
                            logger.LogError(error, "Error attempting to retreive custom field '{}' for row {} at column {}", fieldName, row, FIRST_CUSTOM_FIELD_HEADER_COL + i);
                            continue;
                        }
                    }

                    result.Values.Add(new Student(displayName, fullName, email!, (customScancode ?? generatedScancode)!, row + 1)
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
