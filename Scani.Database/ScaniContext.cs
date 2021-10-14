using Microsoft.EntityFrameworkCore;
using System.Diagnostics.CodeAnalysis;
using Scani.Database.Entities;
using System.Reflection;
using Scani.Database.Enums;

namespace Scani.Database;
public class ScaniContext : DbContext
{
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
    public ScaniContext([NotNull] DbContextOptions options) : base(options)
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
    { }

    public DbSet<ScanCode> ScanCodes { get; set; }
    public DbSet<User> Users { get; set; }
    public DbSet<UserRoleEnum> UserRoles { get; set; }
    public DbSet<Class> Class { get; set; }
    public DbSet<ClassStudent> ClassStudents { get; set; }
    public DbSet<ClassStudentGroup> ClassStudentGroups { get; set; }
    public DbSet<ClassStudentGroupMember> ClassStudentGroupMembers { get; set; }
    public DbSet<ClassTeacher> ClassTeachers { get; set; }
    public DbSet<ClassTime> ClassTimes { get; set; }
    public DbSet<Item> Items { get; set; }
    public DbSet<ItemSet> ItemSets { get; set; }
    public DbSet<ItemSetItem> ItemSetItems { get; set; }
    public DbSet<Loan> Loans { get; set; }
    public DbSet<LoanGroup> LoansGroups {  get; set; }
    public DbSet<LoanGroupMember> LoansGroupsMembers {  get; set; }
    public DbSet<LoanItemLine> LoanItemLines { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
        modelBuilder.ApplyEnumTableConfigurations();

    }
}
