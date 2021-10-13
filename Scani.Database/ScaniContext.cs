using Microsoft.EntityFrameworkCore;
using System.Diagnostics.CodeAnalysis;
using Scani.Database.Entities;

namespace Scani.Database;
public class ScaniContext : DbContext
{
    public ScaniContext([NotNullAttribute] DbContextOptions options) : base(options)
    { }

    public DbSet<ExampleEntity> ExampleEntities { get; set; } = null!;
}
