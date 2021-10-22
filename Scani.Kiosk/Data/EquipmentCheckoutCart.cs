using Scani.Kiosk.Shared.Models;

namespace Scani.Kiosk.Data;

public class EquipmentCheckoutCart
{
    private readonly ISet<int> _returnedEquipmentIds = new HashSet<int>();
    private readonly ISet<int> _requestedEquipmentIds = new HashSet<int>();
    private readonly ICollection<EquipmentInfo> _returnedEquipment = new List<EquipmentInfo>();
    private readonly ICollection<EquipmentInfo> _requestedEquipment = new List<EquipmentInfo>();
    public IEnumerable<int> ReturnedEquipmentIds => _returnedEquipmentIds;
    public IEnumerable<int> RequestedEquipmentIds => _requestedEquipmentIds;
    public IEnumerable<EquipmentInfo> RequestedEquipment => _requestedEquipment;
    public IEnumerable<EquipmentInfo> ReturnedEquipment => _returnedEquipment;

    public void AddItemToCart(EquipmentInfo item)
    {
        if (_requestedEquipmentIds.Contains(item.Id)) return;
        _requestedEquipmentIds.Add(item.Id);
        _requestedEquipment.Add(item);
    }

    public void RemoveItemToCart(EquipmentInfo item)
    {
        if (!_requestedEquipmentIds.Contains(item.Id)) return;
        _requestedEquipmentIds.Remove(item.Id);
        _requestedEquipment.Remove(item);
    }

    public void ReturnItem(EquipmentInfo item)
    {
        if (_returnedEquipmentIds.Contains(item.Id)) return;
        _returnedEquipmentIds.Add(item.Id);
        _returnedEquipment.Add(item);
    }

    public void CancelReturnItem(EquipmentInfo item)
    {
        if (!_returnedEquipmentIds.Contains(item.Id)) return;
        _returnedEquipmentIds.Remove(item.Id);
        _returnedEquipment.Remove(item);
    }

    public bool HasReturned(EquipmentInfo item)
    {
        return _returnedEquipmentIds.Contains(item.Id);
    }

    public bool IsInCart(EquipmentInfo item)
    {
        return _requestedEquipmentIds.Contains(item.Id);
    }
}
