using Google.Apis.Sheets.v4;
using Microsoft.VisualStudio.Threading;
using Scani.Kiosk.Backends.GoogleSheet.Sheets;
using Scani.Kiosk.Helpers;

namespace Scani.Kiosk.Backends.GoogleSheet
{
    public class KioskSheetReaderWriter : IDisposable
    {
        private readonly AsyncReaderWriterLock _sheetLock = new();
        private readonly ILogger<KioskSheetReaderWriter> _logger;
        private readonly string _sheetId;
        private readonly LazyAsyncThrottledAccessor<SheetsService> _sheetsAccessor;

        public KioskSheetReaderWriter(
                ILogger<KioskSheetReaderWriter> logger,
                IConfiguration configuration,
                LazyAsyncThrottledAccessor<SheetsService> sheetsAccessor)
        {
            this._logger = logger;
            this._sheetId = configuration.GetValue<string>("GoogleSheet:SheetId");
            this._sheetsAccessor = sheetsAccessor;
        }

        public async Task<IGoogleSheetKioskState> ReadAsync()
        {
            _logger.LogInformation("Entering read lock on thread {}", Thread.CurrentThread.ManagedThreadId);
            var readLock = await this._sheetLock.ReadLockAsync();
            IList<IList<IList<object>>> sheetCells;
            try
            {
                sheetCells = await _sheetsAccessor.GetRowsAsync(_sheetId, new[] { "Students", "Equipment", "Loans" });
            }
            finally
            {
                _logger.LogInformation("Exiting read lock on thread {}", Thread.CurrentThread.ManagedThreadId);
                await readLock.ReleaseAsync();
            }

            var studentCells = sheetCells[0];
            var equipmentCells = sheetCells[1];
            var loanCells = sheetCells[2];
            var students = StudentSheet.ReadStudents(_logger, studentCells);
            var equipment = EquipmentSheet.ReadEquipmentItems(_logger, equipmentCells);
            var loans = LoanSheet.ReadLoans(_logger, loanCells);
            return new GoogleSheetKioskState
            {
                Students = students.Values.ToList(),
                EquipmentItems = equipment.Values.ToList(),
                Loans = loans.Values.ToList(),
                ParseErrors = students.Errors.Concat(equipment.Errors).ToList(),
                ParseWarnings = students.Warnings.Concat(equipment.Warnings).ToList(),
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

        public void Dispose()
        {
            _sheetsAccessor.Dispose();
        }
    }
}
