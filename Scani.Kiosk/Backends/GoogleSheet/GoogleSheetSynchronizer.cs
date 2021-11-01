using OfficeOpenXml;
using System.Net;

namespace Scani.Kiosk.Backends.GoogleSheet
{
    public class GoogleSheetSynchronizer : IHostedService, IDisposable
    {
        private readonly ILogger<GoogleSheetSynchronizer> _logger;
        private readonly HttpClient _httpClient;
        private readonly string _sheetId;
        private readonly TimeSpan _syncInterval;
        private long _executionCount = 1;
        private Timer? _timer;

        public event Action<GoogleSheetKioskState>? StateChanged;

        public GoogleSheetSynchronizer(ILogger<GoogleSheetSynchronizer> logger, string sheetId, TimeSpan syncInterval)
        {
            _logger = logger;
            _sheetId = sheetId;
            _syncInterval = syncInterval;

            var httpClientHandler = new HttpClientHandler();
            httpClientHandler.AllowAutoRedirect = true;
            _httpClient = new HttpClient(httpClientHandler);
        }

        private async void PerformSynchronization(object? _)
        {
            try
            {
                _logger.LogInformation("Performing synchronization #{}", _executionCount);
                var response = await _httpClient.GetAsync($"https://docs.google.com/spreadsheets/d/{_sheetId}/export?format=xlsx");
                if (response.StatusCode != HttpStatusCode.OK)
                {
                    _logger.LogWarning("Expected OK response from /export but recieved {}", response.StatusCode);
                    return;
                }

                using var stream = await response.Content.ReadAsStreamAsync();
                using var package = new ExcelPackage(stream);

                var students = StudentSheetParser.ParseStudentsFromWorksheet(_logger, package.Workbook.Worksheets["Students"]);
                var equipment = EquipmentSheetParser.ParseEquipmentItemsFromWorksheet(_logger, package.Workbook.Worksheets["Equipment"]);
                var state = new GoogleSheetKioskState
                {
                    Students = students.Values,
                    EquipmentItems = equipment.Values,
                    ParseErrors = students.Errors.Concat(equipment.Errors).ToList(),
                    ParseWarnings = students.Warnings.Concat(equipment.Warnings).ToList(),
                };

                StateChanged?.Invoke(state);
            }
            catch (Exception error)
            {
                _logger.LogError(error, "Unhandled exception when synchronizing google sheet {}", _sheetId);
            }

            _executionCount += 1;
            _timer?.Change(_syncInterval.Milliseconds, Timeout.Infinite);
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Starting syncrhonization of google sheet {}", _sheetId);
            _timer = new Timer(PerformSynchronization, null, 0, Timeout.Infinite);
            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Stopping synchronization of google sheet {}", _sheetId);
            _timer?.Change(Timeout.Infinite, 0);
            return Task.CompletedTask;
        }

        public void Dispose()
        {
            _timer?.Dispose();
        }
    }
}
