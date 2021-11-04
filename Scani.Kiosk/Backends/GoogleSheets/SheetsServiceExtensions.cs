using Google.Apis.Sheets.v4;
using Google.Apis.Util;
using Scani.Kiosk.Helpers;

namespace Scani.Kiosk.Backends.GoogleSheet
{
    public static class SheetsServiceExtensions
    {
        public static async Task<IList<IList<IList<object>>>> GetRowsAsync(this LazyAsyncThrottledAccessor<SheetsService> service, string sheetId, string[] sheetNames, int pageSize = 1000)
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
                    request.Ranges = new Repeatable<string>(sheetNames.Select(n => $"{n}!A1:Z{i + pageSize}").ToList());
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
    }
}
