using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace Scani.Database.Entities;

[Index(nameof(ClassStudentGroupId), nameof(ClassStudentId), IsUnique = true)]
public class ClassStudentGroupMember
{
    [Key]
    public int ClassStudentGroupMemberId { get; set; }
    public int ClassStudentGroupId { get; set; }
    public virtual ClassStudentGroup? ClassStudentGroup { get; set; }
    public int ClassStudentId { get; set; }
    public virtual ClassStudent? ClassStudent { get; set; }
}
