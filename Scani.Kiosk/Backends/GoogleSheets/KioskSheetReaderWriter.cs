using Google.Apis.Sheets.v4;
using Microsoft.VisualStudio.Threading;
using Scani.Kiosk.Backends.GoogleSheets.Sheets;
using Scani.Kiosk.Backends.GoogleSheets.Sheets.Models;
using Google.Apis.Util;

namespace Scani.Kiosk.Backends.GoogleSheets;

public class KioskSheetReaderWriter
{
#pragma warning disable VSTHRD012 // Provide JoinableTaskFactory where allowed
    private readonly AsyncReaderWriterLock _sheetLock = new();
#pragma warning restore VSTHRD012 // Provide JoinableTaskFactory where allowed
    private readonly ILogger<KioskSheetReaderWriter> _logger;
    private readonly string _sheetId;
    private readonly ThrottledKioskSheetAccessor _sheetsAccessor;

    public KioskSheetReaderWriter(
            ILogger<KioskSheetReaderWriter> logger,
            IConfiguration configuration,
            ThrottledKioskSheetAccessor sheetsAccessor)
    {
        _logger = logger;
        _sheetId = configuration.GetValue<string>("GoogleSheet:SheetId");
        _sheetsAccessor = sheetsAccessor;
    }

    private static async Task<IList<IList<IList<object>>>> GetRowsAsync(ThrottledKioskSheetAccessor service, string sheetId, string[] sheetNames, int pageSize = 100)
    {
        var results = new List<IList<IList<object>>>();
        foreach (var _ in sheetNames)
        {
            results.Add(new List<IList<object>>());
        }

        for (var i = 1; true; i += pageSize + 1)
        {
            var response = await service.AccessAsync(async s =>
            {
                var request = s.Spreadsheets.Values.BatchGet(sheetId);
                request.Ranges = new Repeatable<string>(sheetNames.Select(n => $"{n}!A{i}:Z{i + pageSize - 1}").ToList());
                return await request.ExecuteAsync();
            });

            for (var s = 0; s < sheetNames.Length; s++)
            {
                foreach (var row in response.ValueRanges[s].Values)
                {
                    results[s].Add(row);
                }
            }

            if (response.ValueRanges.All(r => r.Values.Count < pageSize))
            {
                break;
            }
        }

        return results;
    }

    public async Task<GoogleSheetKioskState> ReadAsync()
    {
        _logger.LogInformation("Entering read lock on thread {}", Environment.CurrentManagedThreadId);
        var sheetNames = new string[]
        {
            GoogleSheetAttribute.GetSheetName<UserRow>(),
            GoogleSheetAttribute.GetSheetName<EquipmentRow>(),
            GoogleSheetAttribute.GetSheetName<LoanRow>(),
        };

        IList<IList<IList<object>>> sheetCells;
        {
            using var readLock = await _sheetLock.ReadLockAsync();
            sheetCells = await GetRowsAsync(_sheetsAccessor, _sheetId, sheetNames).ConfigureAwait(false);
        }

        var users = KioskCellsReader.ParseCells<UserRow>(_logger, sheetCells[0]);
        var equipment = KioskCellsReader.ParseCells<EquipmentRow>(_logger, sheetCells[1]);
        var loans = KioskCellsReader.ParseCells<LoanRow>(_logger, sheetCells[2]);

        return new GoogleSheetKioskState
        {
            UsersSheet = users,
            EquipmentSheet = equipment,
            LoanSheet = loans
        };
    }

    public async Task WriteAsync(Func<SheetsService, Task> action)
    {
        var writeLock = await _sheetLock.WriteLockAsync();
        try
        {
            await _sheetsAccessor.AccessAsync(action);
        }
        finally
        {
            await writeLock.ReleaseAsync();
        }
    }
}
