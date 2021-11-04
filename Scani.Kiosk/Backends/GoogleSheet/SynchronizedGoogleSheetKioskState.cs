using Microsoft.VisualStudio.Threading;
using System.Collections.Concurrent;

namespace Scani.Kiosk.Backends.GoogleSheet
{
    public interface IGoogleSheetKioskState
    {
        public IReadOnlyCollection<Student> Students { get; }
        public IReadOnlyCollection<EquipmentItem> EquipmentItems { get; }
        public IReadOnlyCollection<Loan> Loans { get; }
        public IReadOnlyCollection<string> ParseErrors { get; }
        public IReadOnlyCollection<string> ParseWarnings { get; }
    }

    public class GoogleSheetKioskState : IGoogleSheetKioskState
    {
        public IReadOnlyCollection<Student> Students { get; init; } = new List<Student>();
        public IReadOnlyCollection<EquipmentItem> EquipmentItems { get; init; } = new List<EquipmentItem>();
        public IReadOnlyCollection<Loan> Loans { get; init; } = new List<Loan>();
        public IReadOnlyCollection<string> ParseErrors { get; init; } = new List<string>();
        public IReadOnlyCollection<string> ParseWarnings { get; init; } = new List<string>();
    }

    public class SynchronizedGoogleSheetKioskState : IDisposable
    {
        private IGoogleSheetKioskState? _state;
        private readonly AsyncReaderWriterLock _stateLock = new AsyncReaderWriterLock();

        public event Action? StateChanged;

        public async Task<R> ReadStateAsync<R>(Func<IGoogleSheetKioskState?, Task<R>> action)
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

        public async Task ReadStateAsync(Func<IGoogleSheetKioskState?, Task> action)
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

        public async Task ReduceStateAsync(Func<IGoogleSheetKioskState?, Task<IGoogleSheetKioskState>> reducer)
        {
            var writeLock = await this._stateLock.WriteLockAsync();
            try
            {
                _state = await reducer(_state);
                StateChanged?.Invoke();
            }
            finally
            {
                await writeLock.ReleaseAsync();
            }
        }

        public void Dispose()
        {
            _stateLock.Dispose();
        }
    }

    public interface ISpreadsheetRow
    {
        public int RowNumber { get; }
        public string SheetName { get; }
        public string CellReference => $"{SheetName}!A{RowNumber}:Z{RowNumber}";
    }

    public record Student : ISpreadsheetRow
    {
        public Student(string displayName, string fullName, string email, string generatedScancode, int rowNumber)
        {
            DisplayName = displayName;
            FullName = fullName;
            Email = email;
            GeneratedScancode = generatedScancode;
            RowNumber = rowNumber;
        }

        public string DisplayName { get; init; }
        public string FullName { get; init; }
        public string Email { get; init; }
        public string? CustomScancode { get; init; }
        public string GeneratedScancode { get; init; }
        public string Scancode => CustomScancode ?? GeneratedScancode;
        public IDictionary<string, string?> CustomFields { get; set; } = new Dictionary<string, string?>();
        public int RowNumber { get; init; }
        public string SheetName => "Students";
    }

    public record EquipmentItem : ISpreadsheetRow
    {
        public EquipmentItem(string name, string generatedScancode, int rowNumber)
        {
            Name = name;
            GeneratedScancode = generatedScancode;
            RowNumber = rowNumber;
        }

        public string Name { get; init; }
        public string? CustomScancode { get; init; }
        public string GeneratedScancode { get; init; }
        public string Scancode => CustomScancode ?? GeneratedScancode;
        public IDictionary<string, string?> CustomFields { get; set; } = new Dictionary<string, string?>();
        public int RowNumber { get; init; }
        public string SheetName => "Equipment";
    }

    public record Loan : ISpreadsheetRow
    {
        public Loan(Guid id, string studentScancode, string equipmentScancode, DateTime loanedDate, DateTime? returnedDate, int rowNumber)
        {
            this.Id = id;
            this.StudentScancode = studentScancode;
            this.EquipmentScancode = equipmentScancode;
            this.LoanedDate = loanedDate;
            this.ReturnedDate = returnedDate;
            this.RowNumber = rowNumber;
        }

        public Guid Id { get; set; }
        public string StudentScancode { get; set; }
        public string EquipmentScancode { get; set; }
        public DateTime LoanedDate { get; set; }
        public DateTime? ReturnedDate { get; set; }
        public int RowNumber { get; init; }
        public string SheetName => "Loans";
    }

    public record LoanRequest
    {
        public LoanRequest(string studentScancode, string equipmentScancode, DateTime requestedDate)
        {
            this.StudentScancode = studentScancode;
            this.EquipmentScancode = equipmentScancode;
            this.RequestedDate = requestedDate;
        }

        public Guid Id { get; } = Guid.NewGuid();
        public string StudentScancode { get; set; }
        public string EquipmentScancode { get; set; }
        public DateTime RequestedDate { get; set; }
    }
}
