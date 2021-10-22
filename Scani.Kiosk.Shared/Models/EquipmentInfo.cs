namespace Scani.Kiosk.Shared.Models;

public class EquipmentInfo
{
    public EquipmentInfo(int id, string displayName)
    {
        this.Id = id;
        this.DisplayName = displayName;
    }

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
    private EquipmentInfo()
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
    { }

    public int Id { get; set; }
    public string DisplayName { get; set; }
    public byte[]? Base64ImageData { get; set; }
    public ICollection<FlexField> FlexFields { get; set; } = new List<FlexField>();
}
