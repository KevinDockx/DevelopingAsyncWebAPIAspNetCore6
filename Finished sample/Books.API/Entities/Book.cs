using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace Books.API.Entities;

[Table("Books")]
public class Book
{
    [Key]
    public Guid Id { get; set; }

    [Required]
    [MaxLength(150)]
    public string Title { get; set; }

    [MaxLength(2500)]
    public string? Description { get; set; }

    public Guid AuthorId { get; set; }
    public Author Author { get; set; } = null!;

    public Book(Guid id, Guid authorId, string title, string? description)
    {
        Id = id;
        AuthorId = authorId;
        Title = title;
        Description = description;
    }
    
}