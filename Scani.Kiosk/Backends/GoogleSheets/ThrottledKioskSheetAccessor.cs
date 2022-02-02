using Google.Apis.Auth.OAuth2;
using Google.Apis.Services;
using Google.Apis.Sheets.v4;
using Scani.Kiosk.Extensions;
using Scani.Kiosk.Helpers;
using System.Text;

namespace Scani.Kiosk.Backends.GoogleSheets;

public sealed class ThrottledKioskSheetAccessor
{
    private static readonly string[] SHEET_SCOPES = new[]
    {
        SheetsService.Scope.Spreadsheets
    };
    private readonly ILogger<ThrottledKioskSheetAccessor> _logger;
    private readonly string? _credentialsFile;
    private readonly string? _credentialsBase64;
    private readonly string _appName;
    private readonly CancellationToken _cancellationToken;
    private readonly Lazy<Task<ThrottledAccessor<SheetsService>>> _lazyThrottledAccessor;

    public ThrottledKioskSheetAccessor(
            ILogger<ThrottledKioskSheetAccessor> logger,
            ILoggerFactory loggerFactory,
            IConfiguration configuration,
            CancellationToken cancellationToken = default)
    {
            this._logger = logger;
            this._credentialsFile = configuration.GetValue<string?>("GoogleSheet:CredentialsFile");
            this._credentialsBase64 = configuration.GetValue<string?>("GoogleSheet:CredentialsBase64");
            this._appName = configuration.GetValue<string>("GoogleSheet:AppName");
            this._cancellationToken = cancellationToken;
#pragma warning disable VSTHRD011 // Use AsyncLazy<T>
            this._lazyThrottledAccessor = new Lazy<Task<ThrottledAccessor<SheetsService>>>(async () =>
                new ThrottledAccessor<SheetsService>(loggerFactory.CreateLogger<ThrottledAccessor<SheetsService>>(), await CreateSheetsServiceAsync(), 100, TimeSpan.FromMinutes(1)));
#pragma warning restore VSTHRD011 // Use AsyncLazy<T>
    }

    private async Task<SheetsService> CreateSheetsServiceAsync()
    {
            if (_credentialsBase64 == null && _credentialsFile == null)
            {
                throw new InvalidOperationException("You must configure EITHER 'GoogleSheet:CredentailsFile' or 'GoogleSheet:CredentialsBase64'.");
            }

            if (!string.IsNullOrWhiteSpace(_credentialsBase64))
            {
                _logger.LogInformation("Using google credentials set via GoogleSeet:CredentialsBase64");
            }

            using var stream = !string.IsNullOrWhiteSpace(_credentialsBase64)
                ? new MemoryStream(Convert.FromBase64String(_credentialsBase64)) as Stream
                : new FileStream(_credentialsFile!, FileMode.Open, FileAccess.Read);
            var credential = await GoogleCredential.FromStreamAsync(stream, _cancellationToken);
            var scopedCredential = credential.CreateScoped(SHEET_SCOPES);
            return new SheetsService(new BaseClientService.Initializer
            {
                HttpClientInitializer = credential,
                ApplicationName = _appName
            });
        }

    public async Task AccessAsync(Func<SheetsService, Task> action, TimeSpan? interval = null)
    {
        await (await _lazyThrottledAccessor).AccessAsync(action, interval);
    }

    public async Task<T> AccessAsync<T>(Func<SheetsService, Task<T>> action, TimeSpan? interval = null)
    {
        return await (await _lazyThrottledAccessor).AccessAsync(action, interval);
    }
}
