using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace Scani.Database.Entities;

[Index(nameof(Name), IsUnique = true)]
public class Class
{
    public Class(string name)
    {
        this.Name = name;
    }

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
    protected Class()
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
    { }

    [Key]
    public int ClassId { get; set; }
    public string Name { get; set; }
    public virtual ICollection<ClassStudent> Students { get; set; } = new List<ClassStudent>();
    public virtual ICollection<ClassTeacher> Teachers { get; set; } = new List<ClassTeacher>();
    public virtual ICollection<ClassTime> ClassTimes { get; set; } = new List<ClassTime>();
    public virtual ICollection<ClassStudentGroup> StudentGroups { get; set; } = new List<ClassStudentGroup>();
}
