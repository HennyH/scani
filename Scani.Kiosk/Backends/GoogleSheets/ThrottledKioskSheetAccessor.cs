using Google.Apis.Auth.OAuth2;
using Google.Apis.Services;
using Google.Apis.Sheets.v4;
using Scani.Kiosk.Extensions;
using Scani.Kiosk.Helpers;

namespace Scani.Kiosk.Backends.GoogleSheet
{
    public sealed class ThrottledKioskSheetAccessor
    {
        private static readonly string[] SHEET_SCOPES = new[]
        {
            SheetsService.Scope.Spreadsheets
        };
        private readonly string _credentialsFile;
        private readonly string _appName;
        private readonly CancellationToken _cancellationToken;
        private readonly Lazy<Task<ThrottledAccessor<SheetsService>>> _lazyThrottledAccessor;

        public ThrottledKioskSheetAccessor(
                ILogger<ThrottledAccessor<SheetsService>> logger,
                IConfiguration configuration,
                CancellationToken cancellationToken = default)
        {
            this._credentialsFile = configuration.GetValue<string>("GoogleSheet:CredentialsFile");
            this._appName = configuration.GetValue<string>("GoogleSheet:AppName");
            this._cancellationToken = cancellationToken;
#pragma warning disable VSTHRD011 // Use AsyncLazy<T>
            this._lazyThrottledAccessor = new Lazy<Task<ThrottledAccessor<SheetsService>>>(async () => new ThrottledAccessor<SheetsService>(logger, await CreateSheetsServiceAsync(), 100, TimeSpan.FromMinutes(1)));
#pragma warning restore VSTHRD011 // Use AsyncLazy<T>
        }

        private async Task<SheetsService> CreateSheetsServiceAsync()
        {
            using var stream = new FileStream(_credentialsFile, FileMode.Open, FileAccess.Read);
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

        public async Task<R> AccessAsync<R>(Func<SheetsService, Task<R>> action, TimeSpan? interval = null)
        {
            return await (await _lazyThrottledAccessor).AccessAsync(action, interval);
        }
    }
}
