namespace Scani.Kiosk.Shared.Models;

public class FlexField
{
    public FlexField(string name, string? value)
    {
        this.Name = name;
        this.Value = value;
    }

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
    private FlexField()
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
    { }

    public string Name { get; set; }
    public string? Value { get; set; }
}
