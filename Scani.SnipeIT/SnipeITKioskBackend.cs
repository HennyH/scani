using Scani.Kiosk.Shared;
using Scani.Kiosk.Shared.Models;
using System.Dynamic;
using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace Scani.SnipeIT;
public class SnipeITKioskBackend : IKioskBackend
{
    private readonly HttpClient _httpClient;

    public SnipeITKioskBackend(HttpClient httpClient)
    {
        _httpClient = httpClient;
        _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", "eyJ0eXAiOiJKV1QiLCJhbGciOiJSUzI1NiJ9.eyJhdWQiOiIxIiwianRpIjoiYTY0NTY3ZGVlNjI0ZmU2NDM3YmExMmJlNGFhNmUzY2Y4MWE2MWQ1ZDk4YzA3MGE5ZDc4NDNkOTM1MTUzYWYxOGRmNDEyOWRmYjYzZDEzMzQiLCJpYXQiOjE2MzQ3ODgwMzgsIm5iZiI6MTYzNDc4ODAzOCwiZXhwIjoyODk3MDkyMDM4LCJzdWIiOiIxIiwic2NvcGVzIjpbXX0.khJjGS_WIYZ5-fZNx_Ryn7xqtYltNs8Lrru3tVa48TNypbYU22l-8hmgOgFgos68Vs5SvbhlDYaqk_CPX8cjJjVM3FXfOYwEv_M87bhx-g_0YjfEXSD8OFkzrBycbKSGAuTYre4AWtDYJ8TjraUTWJsdqgRYBWsRVkAMiuGg8IPYG7RdBCIUEQi7M8mBfkpPA-YrPICm5bOgqqCEziKEp2ny7E8UfX6j6-a5iR_wYsJkYRhUYUFUY53dSuFRRpqjQbCbRCRDB-3wPocfoMVEAG-_6VOvadefxzfq0yixapsPl5jMvrvtf9pDknJ3KuKFXCcFhbYckJTiCGBgGvqD_qDjH7w9vs9l68J227J0hMPUQciBsFsElHo6_7nlINIMYodAW8ITHsyblflpmgLW_4sARGMqn-1uVK_0cYXxT7qn9BAL1aAC3CkEhGV2w4TqvON3czYLU6sDl6M7FItUGBhZPTsxecZPyFGMFeW2HsfEuNNgCggG1raGLz-3NXhtCZTPIIjUoA_qCTMX6avHahE5_m0ntFaO692WrGTgXOYJuCNuZOqPX43AXfCDqAt4cm4XMKrBrW2_7snZKTBPvh4_cNGMhnMipmfO80UlXJq4ZNWE4T44etEpTv8dVJFeHBt3iyAnHSng8IsGFuR3-iwt81RCNWaKH_ofAdrKEwA");
    }

    public async Task CheckoutEquipmentAsUserAsync(int userId, IEnumerable<int> equipmentIds)
    {
        foreach (var equipmentId in equipmentIds)
        {
            var response = await _httpClient.PostAsync(
                $"http://localhost:8000/api/v1/hardware/{equipmentId}/checkout",
                new StringContent(
                    JsonSerializer.Serialize(new
                    {
                        checkout_to_type = "user",
                        assigned_user = userId
                    }),
                    Encoding.UTF8,
                    "application/json"));
            if (response.StatusCode != HttpStatusCode.OK) break;
        }
    }

    public async Task<IEnumerable<EquipmentInfo>> GetAllAvailableEquipmentAsync()
    {
        var results = new List<EquipmentInfo>();
        while (true)
        {
            var response = await _httpClient.GetAsync($"http://localhost:8000/api/v1/hardware?sort=assigned_to&order=asc&offset={results.Count}");
            if (response.StatusCode != HttpStatusCode.OK) break;
            var data = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
            var eqipments = data.RootElement.GetProperty("rows");

            foreach (var equipment in eqipments.EnumerateArray())
            {
                var assignedTo = equipment.GetProperty("assigned_to");
                if (assignedTo.ValueKind != JsonValueKind.Null)
                {
                    return results;
                }

                var id = equipment.GetProperty("id").GetInt32();
                var name = equipment.GetProperty("name").GetString() ?? string.Empty;
                var customFields = equipment.GetProperty("custom_fields");
                var flexFields = customFields.ValueKind != JsonValueKind.Object
                    ? new List<FlexField>()
                    : customFields
                        .EnumerateObject()
                        .Select(f => new FlexField(f.Name, f.Value.GetProperty("value").ToString()))
                        .ToList();
                results.Add(new EquipmentInfo(id, name) { FlexFields = flexFields });
            }
        }

        return results;
    }

    public async Task<IEnumerable<EquipmentInfo>> GetAllEquipmentAsync()
    {
        var results = new List<EquipmentInfo>();
        while (true)
        {
            var response = await _httpClient.GetAsync($"http://localhost:8000/api/v1/hardware?sort=assigned_to&order=asc&offset={results.Count}");
            if (response.StatusCode != HttpStatusCode.OK) break;
            var data = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
            var eqipments = data.RootElement.GetProperty("rows");

            if (!eqipments.EnumerateArray().Any())
            {
                break;
            }

            foreach (var equipment in eqipments.EnumerateArray())
            {
                var id = equipment.GetProperty("id").GetInt32();
                var name = equipment.GetProperty("name").GetString() ?? string.Empty;
                var customFields = equipment.GetProperty("custom_fields");
                var flexFields = customFields.ValueKind != JsonValueKind.Object
                    ? new List<FlexField>()
                    : customFields
                        .EnumerateObject()
                        .Select(f => new FlexField(f.Name, f.Value.GetProperty("value").ToString()))
                        .ToList();
                results.Add(new EquipmentInfo(id, name) { FlexFields = flexFields });
            }
        }

        return results;
    }

    public async Task<EquipmentInfo?> GetEquipmentByScancodeAsync(string scancode)
    {
        var equipment = await GetAllEquipmentAsync();
        return equipment
            .Where(e => e.FlexFields.Any(ff => ff.Name == "Scancode" && ff.Value == scancode))
            .FirstOrDefault();
    }

    public async Task<IEnumerable<EquipmentInfo>> GetEquipmentLoanedToUserAsync(int userId)
    {
        var results = new List<EquipmentInfo>();
        var response = await _httpClient.GetAsync($"http://localhost:8000/api/v1/users/{userId}/assets");
        if (response.StatusCode != HttpStatusCode.OK) return results;
        var data = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        var equipments = data.RootElement.GetProperty("rows");
        foreach (var equipment in equipments.EnumerateArray())
        {
            var id = equipment.GetProperty("id").GetInt32();
            var name = equipment.GetProperty("name").GetString() ?? string.Empty;
            results.Add(new EquipmentInfo(id, name));
        }
        return results;
    }

    public async Task<UserInfo?> GetUserByScancodeAsync(string scancode)
    {
        var response = await _httpClient.GetAsync("http://localhost:8000/api/v1/users?name=Henry%20Hollingworth");
        if (response.StatusCode != HttpStatusCode.OK) return null;
        var data = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        var users = data.RootElement.GetProperty("rows");
        foreach (var user in users.EnumerateArray())
        {
            var id = user.GetProperty("id").GetInt32();
            var name = user.GetProperty("name").GetString() ?? string.Empty;
            var notes = user.GetProperty("notes").GetString() ?? string.Empty;
            if (notes.Contains($"SN:{scancode}") || name.Equals(scancode, StringComparison.OrdinalIgnoreCase))
            {
                return new UserInfo(id, name, true);
            }
        }

        return null;
    }

    public async Task MarkLoanedEquipmentAsReturnedByUserAsync(int userId, IEnumerable<int> equipmentIds)
    {
        foreach (var equipmentId in equipmentIds)
        {
            var response = await _httpClient.PostAsync(
                $"http://localhost:8000/api/v1/hardware/{equipmentId}/checkin",
                new StringContent(
                    JsonSerializer.Serialize(new
                    {
                        note = $"Returned by user id = {userId}"
                    }),
                    Encoding.UTF8,
                    "application/json"));
            if (response.StatusCode != HttpStatusCode.OK) break;
        }
    }
}
