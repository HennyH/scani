using System.ComponentModel.DataAnnotations;

namespace Scani.Database.Entities
{
    public class ExampleEntity
    {
        public ExampleEntity(string name)
        {
            Name = name;
        }

        [Key]
        public int Id { get; set; }
        public string Name { get; set; }
    }
}
