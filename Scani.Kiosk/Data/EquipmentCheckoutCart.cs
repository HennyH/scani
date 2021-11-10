using Scani.Kiosk.Backends.GoogleSheets.Sheets.Models;
using Scani.Kiosk.Helpers;
using System.Diagnostics.CodeAnalysis;
using System.Text;

namespace Scani.Kiosk.Data;

public class EquipmentCheckoutCart
{
    private const string DEFAULT_CHECKOUT_BTN_TEXT = "Checkout Items";
    public event Func<EquipmentCheckoutCart, Task>? OnCartChanged = null!;
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

    public string CheckoutDisplayText
    {
        get
        {
            var sb = new StringBuilder();
            if (IsEmpty)
            {
                sb.Append(DEFAULT_CHECKOUT_BTN_TEXT);
            }
            else
            {
                if (Takes.Any())
                {
                    sb.Append($"Checkout {Takes.Count()} items");
                    if (Returns.Any())
                    {
                        sb.Append(", ");
                    }
                }
                if (Returns.Any())
                {
                    sb.Append($"Return {Returns.Count()} items");
                }
            }
            return sb.ToString();
        }
    }

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
        

    public async Task ToggleSelfReturnAsync(EquipmentRow equipment)
    {
        if (_scancodeToCartItem.ContainsKey(equipment.Scancode))
        {
            _scancodeToCartItem.Remove(equipment.Scancode);
        }
        else
        {
            _scancodeToCartItem.Add(equipment.Scancode, new CartItem(CartItemType.SelfReturn, equipment));
        }

        await OnCartChanged.InvokeAllAsync(this);
    }

    public async Task ToggleDelegatedReturnAsync(EquipmentRow equipment)
    {
        if (_scancodeToCartItem.ContainsKey(equipment.Scancode))
        {
            _scancodeToCartItem.Remove(equipment.Scancode);
        }
        else
        {
            _scancodeToCartItem.Add(equipment.Scancode, new CartItem(CartItemType.DelegatedReturn, equipment));
        }

        await OnCartChanged.InvokeAllAsync(this);
    }

    public async Task ToggleTakeAsync(EquipmentRow equipment)
    {
        if (_scancodeToCartItem.ContainsKey(equipment.Scancode))
        {
            _scancodeToCartItem.Remove(equipment.Scancode);
        }
        else
        {
            _scancodeToCartItem.Add(equipment.Scancode, new CartItem(CartItemType.Take, equipment));
        }

        await OnCartChanged.InvokeAllAsync(this);
    }
}
