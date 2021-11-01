namespace Scani.Kiosk.Shared.Models;

public class UserInfo
{
    public UserInfo(string id, string displayName, bool isAdmin)
    {
        this.Id = id;
        this.DisplayName = displayName;
        this.IsAdmin = isAdmin;
    }

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
    private UserInfo()
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
    { }

    public string Id { get; set; }
    public string DisplayName { get; set; }
    public bool IsAdmin { get; set; }
    public bool IsStudent => !IsAdmin;
    public ICollection<FlexField> FlexFields { get; set; } = new List<FlexField>();
}
