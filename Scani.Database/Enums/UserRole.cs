using System.ComponentModel;

namespace Scani.Database.Enums;

public enum UserRole : byte
{
    [Description("student")]
    Student = 1,
    [Description("teacher")]
    Teacher = 2
}
