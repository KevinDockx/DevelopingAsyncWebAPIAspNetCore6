using AutoMapper;
using Books.API.Entities;
using Books.API.Models;

namespace Books.API.Profiles;

public class BooksProfile : Profile
{
	public BooksProfile()
	{
        CreateMap<Book, BookDto>()
         .ForMember(dest => dest.AuthorName, opt => opt.MapFrom(src =>
             $"{src.Author.FirstName} {src.Author.LastName}"))
         .ConstructUsing(src => new BookDto(src.Id,
             string.Empty,
             src.Title,
             src.Description));

        CreateMap<BookForCreationDto, Book>()
         .ConstructUsing(src => new Book(Guid.NewGuid(),
                src.AuthorId,
                src.Title,
                src.Description));

        CreateMap<Book, BookWithCoversDto>()
           .ForMember(dest => dest.AuthorName, opt => opt.MapFrom(src =>
               $"{src.Author.FirstName} {src.Author.LastName}"))
           .ConstructUsing(src => new BookWithCoversDto(src.Id,
               string.Empty,
               src.Title,
               src.Description));

        CreateMap<Models.External.BookCoverDto, BookCoverDto>();

        CreateMap<IEnumerable<Models.External.BookCoverDto>, BookWithCoversDto>()
               .ForMember(dest => dest.BookCovers,
                   opt => opt.MapFrom(src => src));
    }
}
