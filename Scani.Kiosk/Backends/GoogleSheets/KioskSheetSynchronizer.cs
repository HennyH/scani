namespace Scani.Kiosk.Backends.GoogleSheets;

public class KioskSheetSynchronizer : IHostedService, IDisposable
{
    private readonly ILogger<KioskSheetSynchronizer> _logger;
    private readonly TimeSpan _syncInterval;
    private readonly KioskSheetReaderWriter _kioskSheetReaderWriter;
    private readonly SynchronizedKioskState _kioskState;
    private readonly CancellationTokenSource _stoppingCts = new();
    private int _executionCount;
    private Task? _executingTask;
    private Timer? _timer;
    private bool _isDisposed;

    public KioskSheetSynchronizer(
            ILogger<KioskSheetSynchronizer> logger,
            IConfiguration configuration,
            KioskSheetReaderWriter kioskSheetReaderWriter,
            SynchronizedKioskState kioskState)
    {
        _logger = logger;
        _syncInterval = configuration.GetValue<TimeSpan>("GoogleSheet:SyncInterval");
        _kioskSheetReaderWriter = kioskSheetReaderWriter;
        _kioskState = kioskState;
    }

    public async Task PerformSynchronizationAsync()
    {
        _timer?.Change(Timeout.Infinite, 0);

#pragma warning disable CA1031 // Do not catch general exception types
        try
        {
            _logger.LogInformation("Performing synchronization #{} on thread {}", _executionCount++, Environment.CurrentManagedThreadId);

            var nextState = await _kioskSheetReaderWriter.ReadAsync();
            await _kioskState.ReduceStateAsync(prevState => Task.FromResult(new GoogleSheetKioskState
            {
                UsersSheet = nextState.UsersSheet,
                EquipmentSheet = nextState.EquipmentSheet,
                LoanSheet = nextState.LoanSheet
            }));
        }
        catch (Exception error)
        {
            _logger.LogError(error, "Unhandled exception when synchronizing students and equipment items from google sheet");
        }
#pragma warning restore CA1031 // Do not catch general exception types

        _timer?.Change((int)_syncInterval.TotalMilliseconds, Timeout.Infinite);
    }

    private void PerformSynchronization(object? _)
    {
        _timer?.Change(Timeout.Infinite, 0);
        _executingTask = PerformSynchronizationAsync();
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Starting syncrhonization of kiosk google sheet.");
        _timer = new Timer(PerformSynchronization, null, 0, Timeout.Infinite);
        return Task.CompletedTask;
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Stopping synchronization of kiosk google sheet.");
        _timer?.Change(Timeout.Infinite, 0);
        if (_executingTask != null)
        {
            try
            {
                _stoppingCts.Cancel();
            }
            finally
            {
                await Task.WhenAny(_executingTask, Task.Delay(Timeout.Infinite, cancellationToken)).ConfigureAwait(false);
            }
        }
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
            _stoppingCts?.Dispose();
            _timer?.Dispose();
        }

        _isDisposed = true;
    }
}
