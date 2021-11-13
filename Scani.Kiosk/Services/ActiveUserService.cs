using Scani.Kiosk.Backends.GoogleSheets.Sheets.Models;
using Scani.Kiosk.Helpers;

namespace Scani.Kiosk.Services;

public record ActiveUserState
{
    public ActiveUserState(UserRow userInfo)
    {
        ArgumentNullException.ThrowIfNull(userInfo);
        IsAdmin = userInfo.IsAdmin;
        User = userInfo;
    }

    public ActiveUserState()
    {
        IsAdmin = false;
        User = null;
    }

    public bool HasActiveUser => User != null;
    public bool IsAdmin { get; }
    public UserRow? User { get; }
}

public class ActiveUserService
{
    public ActiveUserState ActiveUserState { get; private set; } = new ActiveUserState();
    public event Func<ActiveUserState, Task>? ActiveUserChanged;

    public async Task SetActiveUserAsync(UserRow userInfo)
    {
        ActiveUserState = new ActiveUserState(userInfo);
        await ActiveUserChanged.InvokeAllAsync(ActiveUserState).ConfigureAwait(false);
    }

    public async Task LogoutActiveUserAsync()
    {
        ActiveUserState = new ActiveUserState();
        await ActiveUserChanged.InvokeAllAsync(ActiveUserState).ConfigureAwait(false);
    }
}
