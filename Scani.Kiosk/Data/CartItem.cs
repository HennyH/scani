using Scani.Kiosk.Backends.GoogleSheets.Sheets.Models;

namespace Scani.Kiosk.Data
{
    public class CartItem
    {
        public CartItem(CartItemType itemType, EquipmentRow equipment)
        {
            this.ItemType = itemType;
            this.EquipmentRow = equipment;
        }

        public CartItemType ItemType { get; set; }
        public EquipmentRow EquipmentRow { get; set; }
    }
}
