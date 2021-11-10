using Microsoft.VisualStudio.Threading;
using Scani.Kiosk.Backends.GoogleSheets.Sheets;
using Scani.Kiosk.Backends.GoogleSheets.Sheets.Models;
using Scani.Kiosk.Helpers;
using System.Collections.Concurrent;

namespace Scani.Kiosk.Backends.GoogleSheet
{
    public class GoogleSheetKioskState
    {
        public KioskSheetReadResult<StudentRow>? StudentsSheet { get; set; }
        public KioskSheetReadResult<EquipmentRow>? EquipmentSheet { get; set; }
        public KioskSheetReadResult<LoanRow>? LoanSheet { get; set; }
        public DateTime? LastModified { get; set; }

        public bool HasLoaded => StudentsSheet != null && EquipmentSheet != null && LoanSheet != null;
        public IEnumerable<EquipmentRow> Equipment => EquipmentSheet?.Rows?.ToList() ?? new List<EquipmentRow>();
        public IEnumerable<StudentRow> Students => StudentsSheet
            ?.Rows
            ?.Where(s => s.IsActiveUser)
            ?.ToList() ?? new List<StudentRow>();
        public IEnumerable<LoanRow> Loans => LoanSheet?.Rows?.ToList() ?? new List<LoanRow>();
        public IEnumerable<LoanRow> ActiveLoans => Loans.Where(l => !l.ReturnedDate.HasValue).ToList();
        public IEnumerable<EquipmentRow> UnloanedEquipment =>
            Equipment
            ?.Where(e =>
                LoanSheet != null
                && !LoanSheet.Rows.Any(l => !l.ReturnedDate.HasValue && l.EquipmentScancode == e.Scancode))
            ?.ToList()
            ?? new List<EquipmentRow>();

        public IEnumerable<EquipmentRow> EquipmentLoanedToUser(string userScancode) =>
            Equipment
            ?.Where(e =>
                LoanSheet != null
                && LoanSheet.Rows.Any(l =>
                    !l.ReturnedDate.HasValue
                    && l.EquipmentScancode == e.Scancode
                    && l.StudentScancode == userScancode))
            ?.ToList()
            ?? new List<EquipmentRow>();

        public EquipmentRow? EquipmentWithScancode(string equipmentScancode) => 
            Equipment
            ?.SingleOrDefault(e => e.Scancode == equipmentScancode);

        public StudentRow? StudentWithScancode(string studentScancode) =>
            Students
            ?.SingleOrDefault(s => s.Email == studentScancode || s.Scancode == studentScancode);

        public IEnumerable<LoanRow> ActiveLoansForUser(string userScancode) =>
            Loans
            ?.Where(l => !l.ReturnedDate.HasValue && l.StudentScancode == userScancode)
            ?.ToList()
            ?? new List<LoanRow>();
    } 

    public class SynchronizedKioskState : IDisposable
    {
        private GoogleSheetKioskState _state = new();
#pragma warning disable VSTHRD012 // Provide JoinableTaskFactory where allowed
        private readonly AsyncReaderWriterLock _stateLock = new AsyncReaderWriterLock();
#pragma warning restore VSTHRD012 // Provide JoinableTaskFactory where allowed

        public event Func<Task>? StateChanged;

        public async Task<R> ReadStateAsync<R>(Func<GoogleSheetKioskState, Task<R>> action)
        {
            var readLock = await this._stateLock.ReadLockAsync();
            try
            {
                return await action(_state);
            }
            finally
            {
                await readLock.ReleaseAsync();
            }
        }

        public async Task ReadStateAsync(Func<GoogleSheetKioskState, Task> action)
        {
            var readLock = await this._stateLock.ReadLockAsync();
            try
            {
                await action(_state);
            }
            finally
            {
                await readLock.ReleaseAsync();
            }
        }

        public async Task ReduceStateAsync(Func<GoogleSheetKioskState, Task<GoogleSheetKioskState>> reducer)
        {
            var writeLock = await this._stateLock.WriteLockAsync();
            try
            {
                _state = await reducer(_state);
                _state.LastModified = DateTime.Now;
            }
            finally
            {
                await writeLock.ReleaseAsync();
            }

            await StateChanged.InvokeAllAsync();
        }

        public void Dispose()
        {
            _stateLock.Dispose();
        }
    }
}
