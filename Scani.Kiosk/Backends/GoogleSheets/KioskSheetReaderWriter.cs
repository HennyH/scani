using Google.Apis.Sheets.v4;
using Microsoft.VisualStudio.Threading;
using Scani.Kiosk.Helpers;
using Scani.Kiosk.Backends.GoogleSheets.Sheets;
using Scani.Kiosk.Backends.GoogleSheets.Sheets.Models;
using Google.Apis.Util;

namespace Scani.Kiosk.Backends.GoogleSheet
{
    public class KioskSheetReaderWriter
    {
        private readonly AsyncReaderWriterLock _sheetLock = new();
        private readonly ILogger<KioskSheetReaderWriter> _logger;
        private readonly string _sheetId;
        private readonly ThrottledKioskSheetAccessor _sheetsAccessor;

        public KioskSheetReaderWriter(
                ILogger<KioskSheetReaderWriter> logger,
                IConfiguration configuration,
                ThrottledKioskSheetAccessor sheetsAccessor)
        {
            this._logger = logger;
            this._sheetId = configuration.GetValue<string>("GoogleSheet:SheetId");
            this._sheetsAccessor = sheetsAccessor;
        }

        private async Task<IList<IList<IList<object>>>> GetRowsAsync(ThrottledKioskSheetAccessor service, string sheetId, string[] sheetNames, int pageSize = 1000)
        {
            var results = new List<IList<IList<object>>>();
            foreach (var _ in sheetNames)
            {
                results.Add(new List<IList<object>>());
            }

            for (var i = 1; true; i += pageSize + 1)
            {
                var response = await service.AccessAsync(async s =>
                {
                    var request = s.Spreadsheets.Values.BatchGet(sheetId);
                    request.Ranges = new Repeatable<string>(sheetNames.Select(n => $"{n}!A1:Z{i + pageSize}").ToList());
                    return await request.ExecuteAsync();
                });

                for (var s = 0; s < sheetNames.Length; s++)
                {
                    foreach (var row in response.ValueRanges[s].Values)
                    {
                        results[s].Add(row);
                    }
                }

                if (response.ValueRanges.All(r => r.Values.Count < pageSize))
                {
                    break;
                }
            }

            return results;
        }

        public async Task<GoogleSheetKioskState> ReadAsync()
        {
            _logger.LogInformation("Entering read lock on thread {}", Thread.CurrentThread.ManagedThreadId);
            var readLock = await this._sheetLock.ReadLockAsync();
            IList<IList<IList<object>>> sheetCells;
            try
            {
                sheetCells = await GetRowsAsync(_sheetsAccessor, _sheetId, new[] { "Students", "Equipment", "Loans" });
            }
            finally
            {
                _logger.LogInformation("Exiting read lock on thread {}", Thread.CurrentThread.ManagedThreadId);
                await readLock.ReleaseAsync();
            }

            var students = KioskCellsReader.ParseCells<StudentRow>(
                _logger,
                "Students",
                sheetCells[0],
                2,
                2,
                (2, 7));
            var equipment = KioskCellsReader.ParseCells<EquipmentRow>(
                _logger,
                "Equipment",
                sheetCells[1],
                2,
                2,
                (2, 4));
            var loans = KioskCellsReader.ParseCells<LoanRow>(
                _logger,
                "Loans",
                sheetCells[2],
                1,
                1);

            return new GoogleSheetKioskState
            {
                StudentsSheet = students,
                EquipmentSheet = equipment,
                LoanSheet = loans
            };
        }

        public async Task WriteAsync(Func<SheetsService, Task> action)
        {
            var writeLock = await this._sheetLock.WriteLockAsync();
            try
            {
                await _sheetsAccessor.AccessAsync(action);
            }
            finally
            {
                await writeLock.ReleaseAsync();
            }
        }
    }
}
