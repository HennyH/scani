using System.ComponentModel.DataAnnotations;

namespace Scani.Database.Entities;

public class LoanGroupMember
{
    [Key]
    public int LoanGroupMemberId {  get; set; }
    public int LoanGroupId { get; set; }
    public virtual LoanGroup? LoanGroup { get; set; }
    public int UserId { get; set; }
    public virtual User? User { get;set; }
}
