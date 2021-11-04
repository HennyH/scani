using Google.Apis.Sheets.v4;
using Scani.Kiosk.Backends.GoogleSheet.Sheets;
using Scani.Kiosk.Helpers;
using Scani.Kiosk.Shared;
using Scani.Kiosk.Shared.Models;
using System.Linq;
using static Scani.Kiosk.Backends.GoogleSheet.SynchronizedGoogleSheetKioskState;

namespace Scani.Kiosk.Backends.GoogleSheet
{
    public class GoogleSheetKioskBackend : IKioskBackend, IDisposable
    {
        private readonly ILogger<GoogleSheetKioskBackend> _logger;
        private readonly TaskCompletionSource _initialStateLoadedTcs = new TaskCompletionSource();
        private SynchronizedGoogleSheetKioskState _kioskState;
        private readonly string _sheetId;
        private LazyAsyncThrottledAccessor<SheetsService> _sheetsAccessor;
        private Task _loaded => _initialStateLoadedTcs.Task;

        public GoogleSheetKioskBackend(
                ILogger<GoogleSheetKioskBackend> logger,
                IConfiguration configuration,
                SynchronizedGoogleSheetKioskState kioskState,
                LazyAsyncThrottledAccessor<SheetsService> sheetsAccessor)
        {
            this._logger = logger;
            this._kioskState = kioskState;
            this._sheetId = configuration.GetValue<string>("GoogleSheet:SheetId");
            this._sheetsAccessor = sheetsAccessor;
        }

        public async Task CheckoutEquipmentAsUserAsync(string userId, IEnumerable<string> equipmentIds)
        {
            var now = DateTime.Now;
            var loanRequests = equipmentIds.Select(equipmentId => new LoanRequest(userId, equipmentId, now));

            var loanRequestWriteResults = await _kioskState.ReadStateAsync(async currState =>
            {
                if (currState == null) throw new InvalidOperationException();

                return await LoanSheet.AddLoans(_logger, _sheetsAccessor, currState.Loans.Count, _sheetId, loanRequests);
            });

            await _kioskState.ReduceStateAsync(prevState => Task.FromResult(new GoogleSheetKioskState
            {
                Students = prevState?.Students ?? Enumerable.Empty<Student>().ToList(),
                EquipmentItems = prevState?.EquipmentItems ?? Enumerable.Empty<EquipmentItem>().ToList(),
                Loans = (prevState?.Loans ?? Enumerable.Empty<Loan>())
                    .Concat(loanRequestWriteResults.Where(r => r.Ok && r.Value != null).Select(r => r.Value!))
                    .ToList()
            } as IGoogleSheetKioskState));
        }

        public async Task<List<EquipmentInfo>> GetAllAvailableEquipmentAsync()
        {
            return await _kioskState.ReadStateAsync(state => Task.FromResult(state.EquipmentItems
                    .Where(e =>
                        !state.Loans.Any(l => !l.ReturnedDate.HasValue && l.EquipmentScancode == e.Scancode))
                    .Select(e => new EquipmentInfo(e.Scancode, e.Name))
                    .ToList()));
        }

        public Task<List<EquipmentInfo>> GetAllEquipmentAsync()
        {
            return _kioskState.ReadStateAsync(state => Task.FromResult(state.EquipmentItems
                    .Select(e => new EquipmentInfo(e.Scancode, e.Name))
                    .ToList()));
        }

        public Task<EquipmentInfo?> GetEquipmentByScancodeAsync(string scancode)
        {
            return _kioskState.ReadStateAsync(state => Task.FromResult(state.EquipmentItems
                    .Where(e => e.Scancode == scancode)
                    .Select(e => new EquipmentInfo(e.Scancode, e.Name))
                    .FirstOrDefault()));
        }

        public Task<List<EquipmentInfo>> GetEquipmentLoanedToUserAsync(string userId)
        {
            return _kioskState.ReadStateAsync(state => Task.FromResult(state.EquipmentItems
                    .Where(e => state.Loans
                        .Any(l => !l.ReturnedDate.HasValue && l.StudentScancode == userId && l.EquipmentScancode == e.Scancode))
                    .Select(e => new EquipmentInfo(e.Scancode, e.Name))
                    .ToList()));
        }

        public Task<UserInfo?> GetUserByScancodeAsync(string scancode)
        {
            return _kioskState.ReadStateAsync(state => Task.FromResult(state.Students
                    .Where(s => s.Scancode == scancode)
                    .Select(e => new UserInfo(e.Scancode, e.DisplayName, false))
                    .FirstOrDefault()));
        }

        public async Task MarkLoanedEquipmentAsReturnedByUserAsync(string userId, IEnumerable<string> equipmentIds)
        {
            var now = DateTime.Now;

            var deletedLoansWriteResult = await _kioskState.ReadStateAsync(async currState =>
            {
                if (currState == null) throw new InvalidOperationException();

                var loans = currState.Loans
                    .Where(l => !l.ReturnedDate.HasValue
                                && l.StudentScancode == userId
                                && equipmentIds.Contains(l.EquipmentScancode, StringComparer.OrdinalIgnoreCase))
                    .ToList();

                return await LoanSheet.DeleteLoans(_logger, _sheetsAccessor, _sheetId, loans);
            });

            await _kioskState.ReduceStateAsync(prevState => Task.FromResult(new GoogleSheetKioskState
            {
                Students = prevState?.Students ?? Enumerable.Empty<Student>().ToList(),
                EquipmentItems = prevState?.EquipmentItems ?? Enumerable.Empty<EquipmentItem>().ToList(),
                Loans = (prevState?.Loans ?? Enumerable.Empty<Loan>())
                    .Select(l =>
                    {
                        var matchingDeletedLoan = deletedLoansWriteResult
                            .SingleOrDefault(r => r.Ok && r.Value != null && r.Value.Id == l.Id);
                        return matchingDeletedLoan switch
                        {
                            null => l,
                            SheetWriteResult<Loan> r => r.Value!
                        };
                    })
                    .ToList()
            } as IGoogleSheetKioskState));
        }

        public void Dispose()
        {
            _sheetsAccessor?.Dispose();
        }
    }
}
