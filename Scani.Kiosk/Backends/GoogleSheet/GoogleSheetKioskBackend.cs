using Scani.Kiosk.Shared;
using Scani.Kiosk.Shared.Models;

namespace Scani.Kiosk.Backends.GoogleSheet
{
    public class GoogleSheetKioskBackend : IKioskBackend, IDisposable
    {
        private readonly ILogger<GoogleSheetKioskBackend> _logger;
        private readonly TaskCompletionSource _initialStateLoadedTcs = new TaskCompletionSource();
        private readonly GoogleSheetSynchronizer _sheetSynchronizer;
        private GoogleSheetKioskState? _state;
        private Task _loaded => _initialStateLoadedTcs.Task;

        public GoogleSheetKioskBackend(ILogger<GoogleSheetKioskBackend> logger, GoogleSheetSynchronizer sheetSyncronizer)
        {
            _logger = logger;
            _sheetSynchronizer = sheetSyncronizer;
            _sheetSynchronizer.StateChanged += HandleStateChanged;
        }

        private void HandleStateChanged(GoogleSheetKioskState state)
        {
            _logger.LogTrace("Kiosk backend updated with new google sheet state");
            _state = state;

            if (!_initialStateLoadedTcs.Task.IsCompleted)
            {
                _initialStateLoadedTcs.SetResult();
            }
        }

        public Task CheckoutEquipmentAsUserAsync(string userId, IEnumerable<string> equipmentIds)
        {
            throw new NotImplementedException();
        }

        public async Task<IEnumerable<EquipmentInfo>> GetAllAvailableEquipmentAsync()
        {
            await _loaded;
            var state = _state;
            return state!.EquipmentItems.Select(e => new EquipmentInfo(e.GeneratedScancode, e.Name)).ToList();
        }

        public async Task<IEnumerable<EquipmentInfo>> GetAllEquipmentAsync()
        {
            await _loaded;
            var state = _state;
            return state!.EquipmentItems.Select(e => new EquipmentInfo(e.GeneratedScancode, e.Name)).ToList();
        }

        public async Task<EquipmentInfo?> GetEquipmentByScancodeAsync(string scancode)
        {
            await _loaded;
            var state = _state;
            return state!.EquipmentItems
                .Where(e => e.CustomScancode == scancode || e.GeneratedScancode == scancode)
                .Select(e => new EquipmentInfo(e.GeneratedScancode, e.Name))
                .FirstOrDefault();
        }

        public async Task<IEnumerable<EquipmentInfo>> GetEquipmentLoanedToUserAsync(string userId)
        {
            await _loaded;
            var state = _state;
            return state!.EquipmentItems.Select(e => new EquipmentInfo(e.GeneratedScancode, e.Name)).ToList();
        }

        public async Task<UserInfo?> GetUserByScancodeAsync(string scancode)
        {
            await _loaded;
            var state = _state;
            return state!.Students
                .Where(e => e.CustomScancode == scancode || e.GeneratedScancode == scancode)
                .Select(e => new UserInfo(e.GeneratedScancode, e.DisplayName, false))
                .FirstOrDefault();
        }

        public async Task MarkLoanedEquipmentAsReturnedByUserAsync(string userId, IEnumerable<string> equipmentIds)
        {
            throw new NotImplementedException();
        }

        public void Dispose()
        {
            _sheetSynchronizer.StateChanged -= HandleStateChanged;
        }
    }
}
