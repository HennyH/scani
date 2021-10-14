using System.ComponentModel.DataAnnotations;

namespace Scani.Database.Entities;

public class Loan
{
    [Key]
    public int LoadId { get; set; }
    public int LoanGroupId { get; set; }
    public virtual LoanGroup? LoanGroup { get; set; }
    public virtual ICollection<LoanItemLine> ItemLines { get; set; } = new List<LoanItemLine>();
}
