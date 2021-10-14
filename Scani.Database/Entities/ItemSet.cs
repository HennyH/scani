using Microsoft.EntityFrameworkCore;

namespace Scani.Database.Entities;

[Index(nameof(Name), IsUnique = true)]
public class ItemSet
{
    public ItemSet(string name)
    {
        this.Name = name;
    }

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
    protected ItemSet()
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
    { }

    public int ItemSetId { get; set; }
    public string Name { get; set; }
}
