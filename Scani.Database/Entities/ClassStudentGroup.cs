using System.ComponentModel.DataAnnotations;

namespace Scani.Database.Entities;

public class ClassStudentGroup
{
    [Key]
    public int ClassStudentGroupId { get; set; }
    public int ClassId { get; set; }
    public virtual Class? Class { get; set; }
    public virtual ICollection<ClassStudentGroupMember> GroupMembers { get; set; } = new List<ClassStudentGroupMember>();
}
