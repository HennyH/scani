using OfficeOpenXml;
using static Scani.Kiosk.Backends.GoogleSheet.SynchronizedGoogleSheetKioskState;

namespace Scani.Kiosk.Backends.GoogleSheet
{
    public static class EquipmentSheet
    {
        private const int CUSTOM_FIELD_HEADER_ROW = 1;
        private const int FIRST_CUSTOM_FIELD_HEADER_COL = 3;
        private const int FIRST_DATA_ROW = 2;
        private const int LAST_DATA_COL = 2;

        public static SheetReadResult<EquipmentItem> ReadEquipmentItems(ILogger logger, IList<IList<object>> cells)
        {
            var result = new SheetReadResult<EquipmentItem>();

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
                    logger.LogError(error, "Error when detecting which custom fields were present in equipment spreadsheet");
                    result.Errors.Add($"Error when detecting which custom fields were present in equipment spreadsheet: {error.Message}");
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
                        logger.LogTrace("Stopped looking for equipment items due to row {} being empty", row);
                    }
                }
                catch (Exception error)
                {
                    logger.LogError(error, "Error occured when trying to determine if the row {} is empty in the equipment sheet", row);
                }

                if (stop)
                {
                    break;
                }

                try
                {
                    var name = cells[row][0] as string;
                    if (string.IsNullOrWhiteSpace(name))
                    {
                        result.Errors.Add($"No name entered for equipment on row {row}");
                        logger.LogError("No name entered for equipment on row {}", row);
                        continue;
                    }

                    var customScancode = cells[row][1] as string;
                    var generatedScancode = cells[row][2] as string;
                    if (string.IsNullOrWhiteSpace(generatedScancode))
                    {
                        result.Errors.Add($"No generated scancode present for equipment '{name}' on row {row}");
                        logger.LogError("No generated scancode present for equipment '{}' on row {}", name, row);
                    }

                    if (string.IsNullOrWhiteSpace(customScancode) && string.IsNullOrWhiteSpace(generatedScancode))
                    {
                        result.Errors.Add($"No custom or generated scancode present for equipment '{name}' on row {row}");
                        logger.LogError("No custom or generated scancode present for equipment '{}' on row {}", name, row);
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

                    result.Values.Add(new EquipmentItem(name, (customScancode ?? generatedScancode)!, row + 1)
                    {
                        CustomScancode = customScancode,
                        CustomFields = customFields
                    });
                }
                catch (Exception error)
                {
                    logger.LogError(error, "Error when attempting to read data row from equipment spreadsheet on row {}", row);
                    result.Errors.Add($"Error when attempting to read data row from equipment spreadsheet on row {row}: {error.Message}");
                    continue;
                }
            }

            return result;
        }
    }
}
