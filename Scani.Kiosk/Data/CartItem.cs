using Scani.Kiosk.Backends.GoogleSheets.Sheets.Models;

namespace Scani.Kiosk.Data;

public record CartItem(CartItemType ItemType, EquipmentRow EquipmentRow);
