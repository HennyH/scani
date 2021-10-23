using Scani.Kiosk.Shared.Models;

namespace Scani.Kiosk.Data;

public class EquipmentCheckoutCart
{
    public event Action<EquipmentCheckoutCart> OnCartChanged = null!;
    private readonly IDictionary<int, EquipmentInfo> _idToReturnedEquipment = new Dictionary<int, EquipmentInfo>();
    private readonly IDictionary<int, EquipmentInfo> _idToRequestedEquipment = new Dictionary<int, EquipmentInfo>();
    public IEnumerable<int> ReturnedEquipmentIds => _idToReturnedEquipment.Keys;
    public IEnumerable<EquipmentInfo> ReturnedEquipment => _idToReturnedEquipment.Values;
    public IEnumerable<int> RequestedEquipmentIds => _idToRequestedEquipment.Keys;
    public IEnumerable<EquipmentInfo> RequestedEquipment => _idToRequestedEquipment.Values;
    public bool IsEmpty => !(_idToRequestedEquipment.Any() || _idToReturnedEquipment.Any());

    public void AddItemToCart(EquipmentInfo item)
    {
        if (_idToRequestedEquipment.ContainsKey(item.Id)) return;
        _idToRequestedEquipment[item.Id] = item;
        OnCartChanged.Invoke(this);
    }

    public void RemoveItemToCart(EquipmentInfo item)
    {
        if (!_idToRequestedEquipment.ContainsKey(item.Id)) return;
        _idToRequestedEquipment.Remove(item.Id);
        OnCartChanged.Invoke(this);
    }

    public void ReturnItem(EquipmentInfo item)
    {
        if (_idToReturnedEquipment.ContainsKey(item.Id)) return;
        _idToReturnedEquipment[item.Id] = item;
        OnCartChanged.Invoke(this);
    }

    public void CancelReturnItem(EquipmentInfo item)
    {
        if (!_idToReturnedEquipment.ContainsKey(item.Id)) return;
        _idToReturnedEquipment.Remove(item.Id);
        OnCartChanged.Invoke(this);
    }

    public bool HasReturned(EquipmentInfo item)
    {
        return _idToReturnedEquipment.ContainsKey(item.Id);
    }

    public bool IsInCart(EquipmentInfo item)
    {
        return _idToRequestedEquipment.ContainsKey(item.Id);
    }
}
