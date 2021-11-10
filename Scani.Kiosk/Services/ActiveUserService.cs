using Scani.Kiosk.Backends.GoogleSheets.Sheets.Models;
using Scani.Kiosk.Helpers;

namespace Scani.Kiosk.Services;

public record ActiveUserState
{
    public ActiveUserState(StudentRow userInfo)
    {
        IsAdmin = false;
        IsStudent = true;
        User = userInfo;
    }

    public ActiveUserState()
    {
        IsAdmin = false;
        IsStudent = false;
        User = null;
    }

    public bool HasActiveUser => User != null;
    public bool IsAdmin { get; }
    public bool IsStudent { get; }
    public StudentRow? User { get; }
}

public class ActiveUserService
{
    public ActiveUserState ActiveUserState { get; private set; } = new ActiveUserState();
    public event Func<ActiveUserState, Task>? ActiveUserChanged;

    public async Task SetActiveUserAsync(StudentRow userInfo)
    {
        ActiveUserState = new ActiveUserState(userInfo);
        await ActiveUserChanged.InvokeAllAsync(ActiveUserState);
    }

    public async Task LogoutActiveUserAsync()
    {
        ActiveUserState = new ActiveUserState();
        await ActiveUserChanged.InvokeAllAsync(ActiveUserState);
    }
}
