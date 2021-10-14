using System.ComponentModel.DataAnnotations;

namespace Scani.Database.Entities;

public class User
{
    public User(int roleId, string username, byte[] saltBytes, byte[] hashedPassword, ScanCode scanCode)
    {
        this.UserRoleId = roleId;
        this.Username = username;
        this.SaltBytes = saltBytes;
        this.HashedPassword = hashedPassword;
        this.ScanCode = scanCode;
    }

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
    protected User()
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
    { }

    [Key]
    public int UserId { get; set; }
    public int UserRoleId { get; set; }
    public virtual UserRoleEnum? UserRole { get; set; }
    public string Username { get; set; }
    public byte[] SaltBytes { get; set; }
    public byte[] HashedPassword { get; set; }
    public int ScanCodeId { get; set; }
    public virtual ScanCode? ScanCode { get; set; }
}
