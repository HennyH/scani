using Microsoft.EntityFrameworkCore;

namespace Scani.Database.Entities;

[Index(nameof(ItemSetId), nameof(ItemId), IsUnique = true)]
public class ItemSetItem
{
    public int ItemSetItemId { get; set; }
    public int ItemSetId { get; set; }
    public virtual ItemSet? ItemSet { get; set; }
    public int ItemId { get; set; }
    public virtual Item? Item { get; set; }
}
