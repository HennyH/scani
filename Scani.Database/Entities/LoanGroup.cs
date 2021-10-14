using System.ComponentModel.DataAnnotations;

namespace Scani.Database.Entities;

public class LoanGroup
{
    [Key]
    public int LoanGroupId { get; set; }
    public virtual Loan? Loan {  get; set; }
    public virtual ICollection<LoanGroupMember> GroupMembers { get; set; } = new List<LoanGroupMember>();
}
