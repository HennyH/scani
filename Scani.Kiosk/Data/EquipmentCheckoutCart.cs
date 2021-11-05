using Scani.Kiosk.Backends.GoogleSheets.Sheets.Models;

namespace Scani.Kiosk.Data;

public class EquipmentCheckoutCart
{
    public event Action<EquipmentCheckoutCart> OnCartChanged = null!;
    private readonly IDictionary<string, EquipmentRow> _idToReturnedEquipment = new Dictionary<string, EquipmentRow>();
    private readonly IDictionary<string, EquipmentRow> _idToRequestedEquipment = new Dictionary<string, EquipmentRow>();
    public IEnumerable<string> ReturnedEquipmentIds => _idToReturnedEquipment.Keys;
    public IEnumerable<EquipmentRow> ReturnedEquipment => _idToReturnedEquipment.Values;
    public IEnumerable<string> RequestedEquipmentIds => _idToRequestedEquipment.Keys;
    public IEnumerable<EquipmentRow> RequestedEquipment => _idToRequestedEquipment.Values;
    public bool IsEmpty => !(_idToRequestedEquipment.Any() || _idToReturnedEquipment.Any());

    public void AddItemToCart(EquipmentRow item)
    {
        if (_idToRequestedEquipment.ContainsKey(item.Scancode)) return;
        _idToRequestedEquipment[item.Scancode] = item;
        OnCartChanged.Invoke(this);
    }

    public void RemoveItemToCart(EquipmentRow item)
    {
        if (!_idToRequestedEquipment.ContainsKey(item.Scancode)) return;
        _idToRequestedEquipment.Remove(item.Scancode);
        OnCartChanged.Invoke(this);
    }

    public void ReturnItem(EquipmentRow item)
    {
        if (_idToReturnedEquipment.ContainsKey(item.Scancode)) return;
        _idToReturnedEquipment[item.Scancode] = item;
        OnCartChanged.Invoke(this);
    }

    public void CancelReturnItem(EquipmentRow item)
    {
        if (!_idToReturnedEquipment.ContainsKey(item.Scancode)) return;
        _idToReturnedEquipment.Remove(item.Scancode);
        OnCartChanged.Invoke(this);
    }

    public bool HasReturned(EquipmentRow item)
    {
        return _idToReturnedEquipment.ContainsKey(item.Scancode);
    }

    public bool IsInCart(EquipmentRow item)
    {
        return _idToRequestedEquipment.ContainsKey(item.Scancode);
    }
}
