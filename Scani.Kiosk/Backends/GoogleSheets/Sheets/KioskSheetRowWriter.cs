using Google.Apis.Sheets.v4;
using Google.Apis.Sheets.v4.Data;
using Scani.Kiosk.Backends.GoogleSheet;
using Scani.Kiosk.Backends.GoogleSheets.Sheets.Models;
using Scani.Kiosk.Helpers;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Reflection;

namespace Scani.Kiosk.Backends.GoogleSheets.Sheets
{
    public static class KioskSheetRowWriter
    {
        public static async Task<IReadOnlyCollection<KioskSheetWriteResult<T>>> UpdateRowsAsync<T>(
                string sheetId,
                string sheetName,
                ThrottledKioskSheetAccessor sheetsAccessor,
                IEnumerable<T> rows)
            where T : ISheetRow
        {
            ArgumentNullException.ThrowIfNull(sheetsAccessor);
            ArgumentNullException.ThrowIfNull(rows);

            var columns = SheetColumnAttribute.GetExpectedColumns<T>();
            var response = await sheetsAccessor.AccessAsync(async s =>
            {
                var request = s.Spreadsheets.Values.BatchUpdate(new BatchUpdateValuesRequest
                {
                    ValueInputOption = "RAW",
                    Data = rows
                        .Select(row => new ValueRange
                        {
                            Range = row.Range,
                            Values = new List<IList<object>>
                            {
                                new List<object>(columns.OrderBy(c => c.ColumnNumber).Select(c => c.Property.GetValue(row) ?? string.Empty))
                            }
                        })
                        .ToList()
                }, sheetId);
                return await request.ExecuteAsync().ConfigureAwait(false);
            }).ConfigureAwait(false);

            var results = new List<KioskSheetWriteResult<T>>();
            foreach (var row in rows)
            {
                if (response.Responses.Any(r => r.UpdatedRange == row.Range))
                {
                    results.Add(new KioskSheetWriteResult<T>(row)
                    {
                        Ok = true
                    });
                }
                else
                {
                    var result = new KioskSheetWriteResult<T>(row) { Ok = false };
                    result.Errors.Add(new KioskSheetWriteError(0, sheetName, "Failed to update row"));
                    results.Add(result);
                }
            }

            return results;
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
    }
}
