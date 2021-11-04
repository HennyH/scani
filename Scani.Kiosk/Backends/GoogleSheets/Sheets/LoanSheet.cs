using Google.Apis.Sheets.v4;
using Google.Apis.Sheets.v4.Data;
using Scani.Kiosk.Helpers;
using System.Linq;
using static Scani.Kiosk.Backends.GoogleSheet.SynchronizedGoogleSheetKioskState;

namespace Scani.Kiosk.Backends.GoogleSheet.Sheets
{
    public class LoanSheet
    {
        private const int FIRST_DATA_ROW = 1;
        private const int LAST_DATA_COL = 3;

        public static SheetReadResult<Loan> ReadLoans(ILogger logger, IList<IList<object>> cells)
        {
            var result = new SheetReadResult<Loan>();

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

                var id = Guid.Parse((cells[row][0] as string)!);
                var studentScancode = cells[row][1] as string;
                var equipmentScancode = cells[row][2] as string;
                var loanedDate = DateTime.Parse((cells[row][3] as string)!);
                var returnedDate = cells[row].Count >= 5 ? DateTime.Parse((cells[row][4] as string)!) : (DateTime?)null;
                result.Values.Add(new Loan(id, studentScancode, equipmentScancode, loanedDate, returnedDate, row + 1));
            }

            return result;
        }

        public static async Task<IEnumerable<SheetWriteResult<Loan>>> DeleteLoans(ILogger logger, LazyAsyncThrottledAccessor<SheetsService> sheetsAccessor, string sheetId, IEnumerable<Loan> loans)
        {
            var now = DateTime.Now;
            var result = await sheetsAccessor.AccessAsync(async s => await s.Spreadsheets.Values
                .BatchUpdate(new BatchUpdateValuesRequest
                {
                    ValueInputOption = "RAW",
                    Data = loans
                        .Select(lr => new ValueRange
                        {
                            Range = $"Loans!E{lr.RowNumber}",
                            Values = new List<IList<object>>
                            {
                                new List<object>
                                {
                                    now.ToString()
                                }
                            }
                        })
                        .ToList()
                }, sheetId)
                .ExecuteAsync());
            /* check response status */
            var updatedRanges = result.Responses
                .Select(r => r.UpdatedRange)
                .ToHashSet();
            var writeResults = new List<SheetWriteResult<Loan>>();
            foreach (var (l, i) in loans.Select((l, i) => (l, i)))
            {
                var sr = l as ISpreadsheetRow;
                var hasFailed = !updatedRanges.Contains($"Loans!E{(l as ISpreadsheetRow).RowNumber}");
                if (hasFailed)
                {
                    logger.LogError("Failed to delete loan {}", sr.CellReference);
                }
                else
                {
                    l.ReturnedDate = now;
                }
                writeResults.Add(new SheetWriteResult<Loan>
                {
                    Value = l,
                    Errors = hasFailed ? new List<string> { $"Failed to delete {(l as ISpreadsheetRow).CellReference}" } : new List<string>(),
                    Warnings = new List<string>()
                });
            }
            return writeResults;
        }

        public static async Task<IEnumerable<SheetWriteResult<Loan>>> AddLoans(ILogger logger, LazyAsyncThrottledAccessor<SheetsService> sheetsAccessor, int loanCount, string sheetId, IEnumerable<LoanRequest> loanRequests)
        {
            var now = DateTime.Now;
            var result = await sheetsAccessor.AccessAsync(s => s.Spreadsheets.Values
                .BatchUpdate(new BatchUpdateValuesRequest
                {
                    ValueInputOption = "RAW",
                    Data = loanRequests
                        .Select((lr, i) => new ValueRange
                        {
                            Range = $"Loans!A{loanCount + 2 + i}:E{loanCount + 2 + i}",
                            Values = new List<IList<object>>
                            {
                                new List<object>
                                {
                                    lr.Id,
                                    lr.StudentScancode,
                                    lr.EquipmentScancode,
                                    lr.RequestedDate.ToString(),
                                    (string?)null
                                }
                            }
                        })
                        .ToList()
                }, sheetId)
                .ExecuteAsync());
            /* check response status */
            var updatedRanges = result.Responses
                .Select(r => r.UpdatedRange)
                .ToHashSet();
            var writeResults = new List<SheetWriteResult<Loan>>();
            foreach (var (l, i) in loanRequests.Select((l, i) => (l, i)))
            {
                var hasFailed = !updatedRanges.Contains($"Loans!A{loanCount + 2 + i}:D{loanCount + 2 + i}");
                if (hasFailed)
                {
                    logger.LogError("Failed to add request of {} for {} at {}", l.StudentScancode, l.EquipmentScancode, l.RequestedDate);
                }
                writeResults.Add(new SheetWriteResult<Loan>
                {
                    Value = new Loan(l.Id, l.StudentScancode, l.EquipmentScancode, l.RequestedDate, null, loanCount + 2 + i),
                    Errors = hasFailed ? new List<string> { $"Failed to add request of {l.StudentScancode} for {l.EquipmentScancode} at {l.RequestedDate}" } : new List<string>(),
                    Warnings = new List<string>()
                });
            }
            return writeResults;
        }
    }
}
