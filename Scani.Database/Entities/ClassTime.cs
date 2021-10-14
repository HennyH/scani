using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace Scani.Database.Entities;

[Index(nameof(ClassId), nameof(StartTime), IsUnique = true)]
public class ClassTime
{
    [Key]
    public int ClassTimeId { get; set; }
    public int ClassId { get; set; }
    public virtual Class? Class { get; set; }
    public DayOfWeek DayOfWeek { get; set; }
    public TimeOnly StartTime { get; set; }
    [Range(1, int.MaxValue)]
    public int DurationMinutes { get; set; }
}
