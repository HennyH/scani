using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Reflection;

namespace Scani.Database.Enums;

public abstract class EnumTable
{
    [Key]
    public int Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public int Ordinal { get; set; }

    protected EnumTable(string name, string description, int ordinal)
    {
        this.Name = name;
        this.Description = description;
        this.Ordinal = ordinal;
    }

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
    protected EnumTable()
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
    { }
}

[Index(nameof(Ordinal), IsUnique = true)]
[Index(nameof(Description), IsUnique = true)]
[Index(nameof(Name), IsUnique = true)]
public abstract class EnumTable<TEntity, TEnum> : EnumTable
    where TEntity : EnumTable
    where TEnum : struct, Enum
{
    [NotMapped]
    public TEnum Value => (TEnum)Enum.ToObject(typeof(TEnum), Id);

    protected EnumTable(string name, string description, int ordinal)
        : base(name, description, ordinal)
    {
        this.Name = name;
        this.Description = description;
        this.Ordinal = ordinal;
    }

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
    protected EnumTable()
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
    { }

    public class ConfigureEnum : IEntityTypeConfiguration<TEntity>
    {
        public void Configure(EntityTypeBuilder<TEntity> builder)
        {
            var entities = Enum.GetValues<TEnum>()
                .Select(v =>
                {
                    var item = (TEntity)Activator.CreateInstance(typeof(TEntity), nonPublic: true)!;
                    item.Id = Convert.ToInt32(v);
                    if (item.Id == 0) throw new Exception($"The enum {typeof(TEnum).Name} has a member {v} whose value is 0, this is not supported for database backed enums due to EF Core seed data constraints");
                    item.Name = Enum.GetName(v) ?? string.Empty;
                    item.Description = v.GetDescription(fallback: string.Empty);
                    item.Ordinal = Convert.ToInt32(v);
                    return item;
                })
                .ToArray();
            builder.HasData(entities);
            builder.ToTable(typeof(TEntity).Name);
        }
    }
}

public static class EnumTableModelBuilderExtensions
{
    /// <summary>
    /// Apply all EnumTable model builder configurations.
    /// </summary>
    /// <remarks>
    /// Based upon the code located here: https://github.com/dotnet/efcore/blob/main/src/EFCore/ModelBuilder.cs
    /// The default ApplyConfigurationsFromAssembly() doesn't pickup our internal `ConfigureEnum` classes
    /// because they're decalred within a generic type.
    /// </remarks>
    /// <param name="modelBuilder"></param>
    public static void ApplyEnumTableConfigurations(this ModelBuilder modelBuilder)
    {
        var applyEntityConfigurationMethod = typeof(ModelBuilder)
            .GetMethods()
            .Single(
                e => e.Name == nameof(ModelBuilder.ApplyConfiguration)
                    && e.ContainsGenericParameters
                    && e.GetParameters().SingleOrDefault()?.ParameterType.GetGenericTypeDefinition()
                    == typeof(IEntityTypeConfiguration<>));
        foreach (var type in Assembly.GetExecutingAssembly().DefinedTypes.OrderBy(t => t.FullName))
        {
            if (type.BaseType?.IsGenericType == false || type.BaseType?.GetGenericTypeDefinition() != typeof(EnumTable<,>))
            {
                continue;
            }

            var enumTableType = type.BaseType;
            foreach (var maybeConfigurationType in enumTableType.GetNestedTypes())
            {
                foreach (var @interface in maybeConfigurationType.GetInterfaces())
                {
                    if (!@interface.IsGenericType)
                    {
                        continue;
                    }

                    if (@interface.GetGenericTypeDefinition() == typeof(IEntityTypeConfiguration<>))
                    {
                        var target = applyEntityConfigurationMethod.MakeGenericMethod(enumTableType.GetGenericArguments()[0]);
                        var configurationType = maybeConfigurationType.MakeGenericType(enumTableType.GetGenericArguments());
                        target.Invoke(modelBuilder, new[] { Activator.CreateInstance(configurationType) });
                    }
                }
            }
        }
    }
}
