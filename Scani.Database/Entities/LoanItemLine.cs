using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace Scani.Database.Entities;

[Index(nameof(LoanId), nameof(ItemId), IsUnique = true)]
public class LoanItemLine
{
    [Key]
    public int LoanItemLineId { get; set; }
    public int LoanId { get; set; }
    public virtual Loan? Loan {  get; set; }
    public int ItemId { get; set; }
    public virtual Item? Item { get; set; }
}
