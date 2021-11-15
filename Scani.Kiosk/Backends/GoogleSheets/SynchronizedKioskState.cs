using Microsoft.VisualStudio.Threading;
using Scani.Kiosk.Backends.GoogleSheets.Sheets;
using Scani.Kiosk.Backends.GoogleSheets.Sheets.Models;
using Scani.Kiosk.Helpers;

namespace Scani.Kiosk.Backends.GoogleSheet
{
    public class GoogleSheetKioskState
    {
        public KioskSheetReadResult<UserRow>? StudentsSheet { get; set; }
        public KioskSheetReadResult<EquipmentRow>? EquipmentSheet { get; set; }
        public KioskSheetReadResult<LoanRow>? LoanSheet { get; set; }
        public DateTime? LastModified { get; set; }

        public bool HasLoaded => StudentsSheet != null && EquipmentSheet != null && LoanSheet != null;
        public IEnumerable<EquipmentRow> Equipment => EquipmentSheet?.Rows?.ToList() ?? new List<EquipmentRow>();
        public IEnumerable<UserRow> Students => StudentsSheet
            ?.Rows
            ?.Where(s => s.IsActiveUser)
            ?.ToList() ?? new List<UserRow>();
        public IEnumerable<LoanRow> Loans => LoanSheet?.Rows?.ToList() ?? new List<LoanRow>();
        public IEnumerable<LoanRow> ActiveLoans => Loans.Where(l => !l.ReturnedDate.HasValue).ToList();
        public IEnumerable<EquipmentRow> UnloanedEquipment =>
            Equipment
            ?.Where(e =>
                LoanSheet != null
                && !LoanSheet.Rows.Any(l => !l.ReturnedDate.HasValue && e.HasScancode(l.EquipmentScancode)))
            ?.ToList()
            ?? new List<EquipmentRow>();

        public IEnumerable<EquipmentRow> EquipmentLoanedToUser(ICollection<string> userScancodes) =>
            Equipment
            ?.Where(e =>
                LoanSheet != null
                && LoanSheet.Rows.Any(l =>
                    !l.ReturnedDate.HasValue
                    && e.HasScancode(l.EquipmentScancode)
                    && userScancodes.Contains(l.StudentScancode)))
            ?.ToList()
            ?? new List<EquipmentRow>();

        public EquipmentRow? EquipmentWithScancode(string equipmentScancode) => 
            Equipment
            ?.SingleOrDefault(e => e.HasScancode(equipmentScancode));

        public UserRow? StudentWithScancode(string studentScancode) =>
            Students
            ?.SingleOrDefault(s => s.Email == studentScancode || s.HasScancode(studentScancode));

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
        private bool _isDisposed;
        public event Func<Task>? StateChanged;

        public async Task<T> ReadStateAsync<T>(Func<GoogleSheetKioskState, Task<T>> action)
        {
            ArgumentNullException.ThrowIfNull(action);
            using var readLock = await this._stateLock.ReadLockAsync();
            return await action(_state).ConfigureAwait(false);
        }

        public async Task ReadStateAsync(Func<GoogleSheetKioskState, Task> action)
        {
            ArgumentNullException.ThrowIfNull(action);
            using var readLock = await this._stateLock.ReadLockAsync();
            await action(_state).ConfigureAwait(false);
        }

        public async Task ReduceStateAsync(Func<GoogleSheetKioskState, Task<GoogleSheetKioskState>> reducer)
        {
            ArgumentNullException.ThrowIfNull(reducer);

            {
                using var writeLock = await this._stateLock.WriteLockAsync();
                _state = await reducer(_state).ConfigureAwait(false);
                _state.LastModified = DateTime.Now;
            }

            await StateChanged.InvokeAllAsync().ConfigureAwait(false);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (_isDisposed) return;

            if (disposing)
            {
                _stateLock?.Dispose();
            }

            _isDisposed = true;
        }
    }
}
