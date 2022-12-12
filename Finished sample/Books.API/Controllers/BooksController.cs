using AutoMapper;
using Books.API.Filters;
using Books.API.Models;
using Books.API.Services;
using Microsoft.AspNetCore.Mvc;

namespace Books.API.Controllers;

[Route("api")]
[ApiController]
public class BooksController : ControllerBase
{
    private readonly IBooksRepository _booksRepository;
    private readonly IMapper _mapper;
    private readonly ILogger<BooksController> _logger;

    public BooksController(IBooksRepository booksRepository, 
        IMapper mapper,
        ILogger<BooksController> logger)
    { 
        _booksRepository = booksRepository ??
            throw new ArgumentNullException(nameof(booksRepository));
        _mapper = mapper ??
            throw new ArgumentNullException(nameof(mapper));
        _logger = logger ?? 
            throw new ArgumentNullException(nameof(logger));
    }


    [HttpGet("books")]
    [TypeFilter(typeof(BooksResultFilter))]
    public IActionResult GetBooks_BadCode()
    { 
        var bookEntities = _booksRepository.GetBooksAsync().Result;
        return Ok(bookEntities);
    }

    [HttpGet("booksstream")]
    public async IAsyncEnumerable<BookDto> StreamBooks()
    {
        await foreach (var bookFromRepository in
            _booksRepository.GetBooksAsAsyncEnumerable())
        {      
            // add a delay to visually see the effect
            await Task.Delay(500);
            yield return _mapper.Map<BookDto>(bookFromRepository); ;
        }
    }
    
    [HttpGet("books/{id}", Name = "GetBook")]
    [TypeFilter(typeof(BookWithCoversResultFilter))]
    public async Task<IActionResult> GetBook(Guid id,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation($"ThreadId when entering GetBook: " +
            $"{System.Threading.Thread.CurrentThread.ManagedThreadId}");

        var bookEntity = await _booksRepository.GetBookAsync(id);
        if (bookEntity == null)
        {
            return NotFound();
        } 

        var amountOfPages = await GetBookPages_BadCode(id);

        //var bookCover = await _booksRepository
        //    .GetBookCoverAsync("dummycover");

        var bookCovers = await _booksRepository.
            GetBookCoversProcessOneByOneAsync(id, cancellationToken);

        //var bookCovers = await _booksRepository.
        //    GetBookCoversProcessAfterWaitForAllAsync(id);

        return Ok((bookEntity, bookCovers));
    }

    private Task<int> GetBookPages_BadCode(Guid id)
    {
        return Task.Run(() =>
        {
            var pageCalculator = new Books.Legacy.ComplicatedPageCalculator();

            _logger.LogInformation($"ThreadId when calculating the amount of pages: " +
                $"{System.Threading.Thread.CurrentThread.ManagedThreadId}");

            return pageCalculator.CalculateBookPages(id);
        });
    }


    [HttpPost("books")]
    [TypeFilter(typeof(BookResultFilter))]
    public async Task<IActionResult> CreateBook(
        [FromBody] BookForCreationDto bookForCreation)
    {
        var bookEntity = _mapper.Map<Entities.Book>(bookForCreation);
        _booksRepository.AddBook(bookEntity);
        await _booksRepository.SaveChangesAsync();

        await _booksRepository.GetBookAsync(bookEntity.Id);

        return CreatedAtRoute("GetBook",
            new { id = bookEntity.Id },
            bookEntity);
    }
}
