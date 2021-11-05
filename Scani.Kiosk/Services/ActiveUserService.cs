using Scani.Kiosk.Backends.GoogleSheets.Sheets.Models;

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
    public event Action<ActiveUserState>? ActiveUserChanged;

    public void SetActiveUser(StudentRow userInfo)
    {
        ActiveUserState = new ActiveUserState(userInfo);
        ActiveUserChanged?.Invoke(ActiveUserState);
    }

    public void LogoutActiveUser()
    {
        ActiveUserState = new ActiveUserState();
        ActiveUserChanged?.Invoke(ActiveUserState);
    }
}
