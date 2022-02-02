using Scani.Kiosk.Backends.GoogleSheets.Sheets.Models;
using Scani.Kiosk.Helpers;

namespace Scani.Kiosk.Services;

public record ActiveUserState
{
    public ActiveUserState(UserRow userInfo, TimeZoneInfo timeZone)
    {
        ArgumentNullException.ThrowIfNull(userInfo);
        this.IsAdmin = userInfo.IsAdminUser;
        this.User = userInfo;
        this.TimeZone = timeZone;
    }

    public ActiveUserState()
    {
        IsAdmin = false;
        User = null;
    }

    public bool HasActiveUser => User != null;
    public bool IsAdmin { get; }
    public UserRow? User { get; }
    public TimeZoneInfo TimeZone { get; init; } = TimeZoneInfo.Local;
}

public class ActiveUserService
{
    public ActiveUserState ActiveUserState { get; private set; }
    public event Func<ActiveUserState, Task>? ActiveUserChanged;
    private readonly TimeZoneInfo _userTimeZone;

    public ActiveUserService(IConfiguration configuration)
    {
        _userTimeZone = TimeZoneInfo.FindSystemTimeZoneById(configuration.GetValue<string>("TimeZoneIdentifier"));
        ActiveUserState = new ActiveUserState { TimeZone = _userTimeZone };
    }

    public async Task SetActiveUserAsync(UserRow userInfo)
    {
        ActiveUserState = new ActiveUserState(userInfo, _userTimeZone);
        await ActiveUserChanged.InvokeAllAsync(ActiveUserState);
    }

    public async Task LogoutActiveUserAsync()
    {
        ActiveUserState = new ActiveUserState();
        await ActiveUserChanged.InvokeAllAsync(ActiveUserState);
    }
}
