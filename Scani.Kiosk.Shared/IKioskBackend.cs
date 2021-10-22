namespace Scani.Kiosk.Shared;

using Scani.Kiosk.Shared.Models;

public interface IKioskBackend
{
    public Task<UserInfo?> GetUserByScancodeAsync(string scancode);
    public Task<EquipmentInfo?> GetEquipmentByScancodeAsync(string scancode);
    public Task<IEnumerable<EquipmentInfo>> GetAllEquipmentAsync();
    public Task<IEnumerable<EquipmentInfo>> GetEquipmentLoanedToUserAsync(int userId);
    public Task<IEnumerable<EquipmentInfo>> GetAllAvailableEquipmentAsync();
    public Task MarkLoanedEquipmentAsReturnedByUser(int userId, IEnumerable<int> equipmentIds);
}
