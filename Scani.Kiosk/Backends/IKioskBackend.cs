namespace Scani.Kiosk.Shared;

using Scani.Kiosk.Shared.Models;

public interface IKioskBackend
{
    public Task<UserInfo?> GetUserByScancodeAsync(string scancode);
    public Task<EquipmentInfo?> GetEquipmentByScancodeAsync(string scancode);
    public Task<List<EquipmentInfo>> GetAllEquipmentAsync();
    public Task<List<EquipmentInfo>> GetEquipmentLoanedToUserAsync(string userId);
    public Task<List<EquipmentInfo>> GetAllAvailableEquipmentAsync();
    public Task CheckoutEquipmentAsUserAsync(string userId, IEnumerable<string> equipmentIds);
    public Task MarkLoanedEquipmentAsReturnedByUserAsync(string userId, IEnumerable<string> equipmentIds);
}
