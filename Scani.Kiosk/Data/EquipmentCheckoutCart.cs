using Scani.Kiosk.Backends.GoogleSheets.Sheets.Models;
using System.Diagnostics.CodeAnalysis;

namespace Scani.Kiosk.Data;

public class EquipmentCheckoutCart
{
    public event Action<EquipmentCheckoutCart> OnCartChanged = null!;
    private readonly IDictionary<string, CartItem> _scancodeToCartItem = new Dictionary<string, CartItem>();
    public IEnumerable<string> ScancodesInCart => _scancodeToCartItem.Keys;
    public IEnumerable<EquipmentRow> Takes => _scancodeToCartItem.Values
        .Where(ci => ci.ItemType == CartItemType.Take)
        .Select(ci => ci.EquipmentRow)
        .ToList();
    public IEnumerable<EquipmentRow> SelfReturns => _scancodeToCartItem.Values
        .Where(ci => ci.ItemType == CartItemType.SelfReturn)
        .Select(ci => ci.EquipmentRow)
        .ToList();
    public IEnumerable<EquipmentRow> DelegatedReturns => _scancodeToCartItem.Values
        .Where(ci => ci.ItemType == CartItemType.DelegatedReturn)
        .Select(ci => ci.EquipmentRow)
        .ToList();
    public IEnumerable<EquipmentRow> Returns => SelfReturns.Concat(DelegatedReturns).ToList();
    public bool IsEmpty => !_scancodeToCartItem.Keys.Any();

    public bool TryGetCartItemTypeForEquipment(EquipmentRow equipment, [NotNullWhen(true)] out CartItemType? itemType)
    {
        itemType = null;

        if (_scancodeToCartItem.TryGetValue(equipment.Scancode, out var cartItem))
        {
            itemType = cartItem.ItemType;
            return true;
        }

        return false;
    }
        

    public void ToggleSelfReturn(EquipmentRow equipment)
    {
        if (_scancodeToCartItem.ContainsKey(equipment.Scancode))
        {
            _scancodeToCartItem.Remove(equipment.Scancode);
        }
        else
        {
            _scancodeToCartItem.Add(equipment.Scancode, new CartItem(CartItemType.SelfReturn, equipment));
        }

        OnCartChanged?.Invoke(this);
    }

    public void ToggleDelegatedReturn(EquipmentRow equipment)
    {
        if (_scancodeToCartItem.ContainsKey(equipment.Scancode))
        {
            _scancodeToCartItem.Remove(equipment.Scancode);
        }
        else
        {
            _scancodeToCartItem.Add(equipment.Scancode, new CartItem(CartItemType.DelegatedReturn, equipment));
        }

        OnCartChanged?.Invoke(this);
    }

    public void ToggleTake(EquipmentRow equipment)
    {
        if (_scancodeToCartItem.ContainsKey(equipment.Scancode))
        {
            _scancodeToCartItem.Remove(equipment.Scancode);
        }
        else
        {
            _scancodeToCartItem.Add(equipment.Scancode, new CartItem(CartItemType.Take, equipment));
        }

        OnCartChanged?.Invoke(this);
    }
}
