using Google.Apis.Auth.OAuth2;
using Google.Apis.Services;
using Google.Apis.Sheets.v4;
using Scani.Kiosk.Helpers;

namespace Scani.Kiosk.Backends.GoogleSheet
{
    public sealed class ThrottledKioskSheetAccessorFactory
    {
        private static readonly string[] SHEET_SCOPES = new[]
        {
            SheetsService.Scope.Spreadsheets
        };
        private readonly string _credentialsFile;
        private readonly string _appName;
        private readonly CancellationToken _cancellationToken;

        public ThrottledKioskSheetAccessorFactory(
                IConfiguration configuration,
                CancellationToken cancellationToken = default)
        {
            this._credentialsFile = configuration.GetValue<string>("GoogleSheet:CredentialsFile");
            this._appName = configuration.GetValue<string>("GoogleSheet:AppName");
            this._cancellationToken = cancellationToken;
        }

        public LazyAsyncThrottledAccessor<SheetsService> CreateAccessor(int limit, TimeSpan limitPeriod)
        {
            return new LazyAsyncThrottledAccessor<SheetsService>(() => CreateSheetsService(), limit, limitPeriod);
        }

        private async Task<SheetsService> CreateSheetsService()
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
    }
}
