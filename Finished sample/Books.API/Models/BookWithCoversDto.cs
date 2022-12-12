namespace Books.API.Models;

public class BookWithCoversDto : BookDto
{
    public IEnumerable<BookCoverDto> BookCovers { get; set; }
        = new List<BookCoverDto>();

    public BookWithCoversDto(Guid id,
        string authorName,
        string title,
        string? description) : base(id, authorName, title, description)
    {
    }
}