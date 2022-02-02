using Microsoft.VisualStudio.Threading;
using Scani.Kiosk.Backends.GoogleSheets.Sheets;
using Scani.Kiosk.Backends.GoogleSheets.Sheets.Models;
using Scani.Kiosk.Helpers;

namespace Scani.Kiosk.Backends.GoogleSheets;

public class GoogleSheetKioskState
{
    public KioskSheetReadResult<UserRow>? UsersSheet { get; set; }
    public KioskSheetReadResult<EquipmentRow>? EquipmentSheet { get; set; }
    public KioskSheetReadResult<LoanRow>? LoanSheet { get; set; }
    public DateTime? LastModified { get; set; }

    public bool HasLoaded => UsersSheet != null && EquipmentSheet != null && LoanSheet != null;
    public IEnumerable<EquipmentRow> Equipment => EquipmentSheet?.Rows?.ToList() ?? new List<EquipmentRow>();
    public IEnumerable<UserRow> Users => UsersSheet
        ?.Rows
        ?.Where(s => s.IsActiveUser)
        ?.ToList() ?? new List<UserRow>();
    public IEnumerable<LoanRow> Loans => LoanSheet?.Rows?.ToList() ?? new List<LoanRow>();
    public IEnumerable<LoanRow> ActiveLoans => Loans.Where(l => !l.ReturnedDate.HasValue).ToList();
    public IEnumerable<EquipmentRow> UnloanedEquipment =>
        Equipment
        ?.Where(e =>
            LoanSheet != null
            && !LoanSheet.Rows.Any(l =>
                !l.ReturnedDate.HasValue
                && e.HasScancode(l.EquipmentScancode)))
        ?.ToList()
        ?? new List<EquipmentRow>();

    public IEnumerable<EquipmentRow> EquipmentLoanedToUser(ICollection<string> userScancodes) =>
        Equipment
        ?.Where(e =>
            LoanSheet != null
            && LoanSheet.Rows.Any(l =>
                !l.ReturnedDate.HasValue
                && e.HasScancode(l.EquipmentScancode)
                && userScancodes.Contains(l.UserScancode)))
        ?.ToList()
        ?? new List<EquipmentRow>();

    public EquipmentRow? EquipmentWithScancode(string equipmentScancode) =>
        Equipment
            ?.SingleOrDefault(e => e.HasScancode(equipmentScancode));

    public UserRow? UserWithScancode(string userScancode) =>
        Users
        ?.SingleOrDefault(s => s.HasScancode(userScancode));

    public IEnumerable<LoanRow> ActiveLoansForUser(string userScancode) =>
        Loans
        ?.Where(l =>
            !l.ReturnedDate.HasValue
            && UserWithScancode(userScancode)?.Scancodes?.Contains(l.UserScancode) == true)
        ?.ToList()
        ?? new List<LoanRow>();
}

public class SynchronizedKioskState : IDisposable
{
    private GoogleSheetKioskState _state = new();
#pragma warning disable VSTHRD012 // Provide JoinableTaskFactory where allowed
    private readonly AsyncReaderWriterLock _stateLock = new();
#pragma warning restore VSTHRD012 // Provide JoinableTaskFactory where allowed
    private bool _isDisposed;
    public event Func<Task>? StateChanged;

    public async Task<T> ReadStateAsync<T>(Func<GoogleSheetKioskState, Task<T>> action)
    {
        ArgumentNullException.ThrowIfNull(action);
        using var readLock = await _stateLock.ReadLockAsync();
        return await action(_state);
    }

    public async Task ReadStateAsync(Func<GoogleSheetKioskState, Task> action)
    {
        ArgumentNullException.ThrowIfNull(action);
        using var readLock = await _stateLock.ReadLockAsync();
        await action(_state);
    }

    public async Task ReduceStateAsync(Func<GoogleSheetKioskState, Task<GoogleSheetKioskState>> reducer)
    {
        ArgumentNullException.ThrowIfNull(reducer);

        {
            using var writeLock = await _stateLock.WriteLockAsync();
            _state = await reducer(_state);
            _state.LastModified = DateTime.Now;
        }

        await StateChanged.InvokeAllAsync();
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
