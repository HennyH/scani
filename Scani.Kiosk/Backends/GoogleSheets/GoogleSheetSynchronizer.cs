﻿using Google.Apis.Auth.OAuth2;
using Google.Apis.Services;
using Google.Apis.Sheets.v4;
using System.Collections.Concurrent;
using Scani.Kiosk.Backends.GoogleSheet.Sheets;
using static Scani.Kiosk.Backends.GoogleSheet.SynchronizedGoogleSheetKioskState;

namespace Scani.Kiosk.Backends.GoogleSheet
{
    public class GoogleSheetSynchronizer : IHostedService, IDisposable
    {
        private readonly ILogger<GoogleSheetSynchronizer> _logger;
        private readonly TimeSpan _syncInterval;
        private readonly KioskSheetReaderWriter _kioskSheetReaderWriter;
        private readonly SynchronizedGoogleSheetKioskState _kioskState;
        private int _executionCount = 0;
        private Timer? _timer;

        public GoogleSheetSynchronizer(
                ILogger<GoogleSheetSynchronizer> logger,
                IConfiguration configuration,
                KioskSheetReaderWriter kioskSheetReaderWriter,
                SynchronizedGoogleSheetKioskState kioskState)
        {
            this._logger = logger;
            this._syncInterval = configuration.GetValue<TimeSpan>("GoogleSheet:SyncInterval");
            this._kioskSheetReaderWriter = kioskSheetReaderWriter;
            this._kioskState = kioskState;
        }

        private async void PerformSynchronization(object? _)
        {
            try
            {
                _logger.LogInformation("Performing synchronization #{} on thread {}", _executionCount++, Thread.CurrentThread.ManagedThreadId);

                var nextState = await _kioskSheetReaderWriter.ReadAsync();
                await _kioskState.ReduceStateAsync(prevState => Task.FromResult(new GoogleSheetKioskState
                {
                    Students = nextState.Students,
                    EquipmentItems = nextState.EquipmentItems,
                    Loans = prevState?.Loans?.Any() == true ? prevState.Loans : nextState.Loans
                } as IGoogleSheetKioskState));
            }
            catch (Exception error)
            {
                _logger.LogError(error, "Unhandled exception when synchronizing students and equipment items from google sheet");
            }

            _timer?.Change((int)_syncInterval.TotalMilliseconds, Timeout.Infinite);
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Attempting to connect to google sheets API");
            

            _logger.LogInformation("Starting syncrhonization of kiosk google sheet.");
            _timer = new Timer(PerformSynchronization, null, 0, Timeout.Infinite);
            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Stopping synchronization of kiosk google sheet.");
            _timer?.Change(Timeout.Infinite, 0);
            return Task.CompletedTask;
        }

        public void Dispose()
        {
            _timer?.Dispose();
            _kioskSheetReaderWriter?.Dispose();
        }
    }
}
