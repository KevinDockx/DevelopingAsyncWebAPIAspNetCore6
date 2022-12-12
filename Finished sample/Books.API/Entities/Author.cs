using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace Books.API.Entities;


[Table("Authors")]
public class Author
{
    [Key]
    public Guid Id { get; set; }

    [Required]
    [MaxLength(150)]
    public string FirstName { get; set; }

    [Required]
    [MaxLength(150)]
    public string LastName { get; set; }


    public Author(Guid id, string firstName, string lastName)
    {
        Id = id;
        FirstName = firstName;
        LastName = lastName;
    }
}