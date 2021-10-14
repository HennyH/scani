using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace Scani.Database.Entities;

[Index(nameof(ClassId), nameof(UserId), IsUnique = true)]
public class ClassStudent
{
    [Key]
    public int ClassStudentId { get; set; }
    public int ClassId { get; set; }
    public virtual Class? Class { get; set; }
    public int UserId { get; set; }
    public virtual User? User { get; set; }
    public virtual ICollection<ClassStudentGroupMember> GroupMemberships { get; set; } = new List<ClassStudentGroupMember>();
}
