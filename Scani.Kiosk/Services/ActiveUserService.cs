using Scani.Kiosk.Shared.Models;

namespace Scani.Kiosk.Services;

public record ActiveUserState
{
    public ActiveUserState(UserInfo userInfo)
    {
        IsAdmin = userInfo.IsAdmin;
        IsStudent = userInfo.IsStudent;
        UserInfo = userInfo;
    }

    public ActiveUserState()
    {
        IsAdmin = false;
        IsStudent = false;
        UserInfo = null;
    }

    public bool HasActiveUser => UserInfo != null;
    public bool IsAdmin { get; }
    public bool IsStudent { get; }
    public UserInfo? UserInfo { get; }
}

public class ActiveUserService
{
    public ActiveUserState ActiveUserState { get; private set; } = new ActiveUserState();
    public event Action<ActiveUserState>? ActiveUserChanged;

    public void SetActiveUser(UserInfo userInfo)
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
