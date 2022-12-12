using Books.API.DbContexts;
using Books.API.Entities;
using Books.API.Models.External;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace Books.API.Services;

public class BooksRepository : IBooksRepository
{
    private readonly BooksContext _context;
    private readonly IHttpClientFactory _httpClientFactory;

    public BooksRepository(BooksContext context,
        IHttpClientFactory httpClientFactory)
    {
        _context = context ??
            throw new ArgumentNullException(nameof(context));
        _httpClientFactory = httpClientFactory ??
            throw new ArgumentNullException(nameof(httpClientFactory));
    }


    public void AddBook(Book bookToAdd)
    {
        if (bookToAdd == null)
        {
            throw new ArgumentNullException(nameof(bookToAdd));
        }

        _context.Add(bookToAdd);
    }

    public async Task<Book?> GetBookAsync(Guid id)
    { 
        return await _context.Books
            .Include(b => b.Author)
            .FirstOrDefaultAsync(b => b.Id == id); 
    }

    public async Task<BookCoverDto?> GetBookCoverAsync(string id)
    {
        var httpClient = _httpClientFactory.CreateClient();

        // pass through a dummy name
        var response = await httpClient
               .GetAsync($"http://localhost:52644/api/bookcovers/{id}");
        if (response.IsSuccessStatusCode)
        {
            return JsonSerializer.Deserialize<Models.External.BookCoverDto>(
                await response.Content.ReadAsStringAsync(),
                new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true,
                });
        }

        return null;
    }

    public async Task<IEnumerable<Models.External.BookCoverDto>> GetBookCoversProcessOneByOneAsync(
        Guid bookId,
        CancellationToken cancellationToken)
    {
        var httpClient = _httpClientFactory.CreateClient();
        var bookCovers = new List<Models.External.BookCoverDto>();

        // create a list of fake bookcovers
        var bookCoverUrls = new[]
        {
                $"http://localhost:52644/api/bookcovers/{bookId}-dummycover1",
                $"http://localhost:52644/api/bookcovers/{bookId}-dummycover2?returnFault=true",
                $"http://localhost:52644/api/bookcovers/{bookId}-dummycover3",
                $"http://localhost:52644/api/bookcovers/{bookId}-dummycover4",
                $"http://localhost:52644/api/bookcovers/{bookId}-dummycover5"
            };

        using (var cancellationTokenSource = new CancellationTokenSource())
        {
            using (var linkedCancellationTokenSource =
              CancellationTokenSource.CreateLinkedTokenSource(
                  cancellationTokenSource.Token, cancellationToken))
            {
                // fire tasks & process them one by one
                foreach (var bookCoverUrl in bookCoverUrls)
                {
                    var response = await httpClient
                       .GetAsync(bookCoverUrl,
                       linkedCancellationTokenSource.Token);

                    if (response.IsSuccessStatusCode)
                    {
                        var bookCover = JsonSerializer.Deserialize<Models.External.BookCoverDto>(
                            await response.Content.ReadAsStringAsync(
                                linkedCancellationTokenSource.Token),
                                new JsonSerializerOptions
                                {
                                    PropertyNameCaseInsensitive = true,
                                });

                        if (bookCover != null)
                        {
                            bookCovers.Add(bookCover);
                        }
                    }
                    else
                    {
                        cancellationTokenSource.Cancel();
                    }
                }
            }
        }
        return bookCovers;
    }

    public async Task<IEnumerable<Models.External.BookCoverDto>> DownloadBookCoverAsync_BadCode(
        Guid bookId)
    {
        var httpClient = _httpClientFactory.CreateClient();

        var bookCoverUrls = new[]
        {
         $"http://localhost:52644/api/bookcovers/{bookId}-dummycover1",
         $"http://localhost:52644/api/bookcovers/{bookId}-dummycover2"
     };

        var bookCovers = new List<Models.External.BookCoverDto>();
        var downloadTask1 = DownloadBookCoverAsync(bookCoverUrls[0],
            bookCovers);
        var downloadTask2 = DownloadBookCoverAsync(bookCoverUrls[1],
            bookCovers);
        await Task.WhenAll(downloadTask1, downloadTask2);
        return bookCovers;
    }

    private async Task DownloadBookCoverAsync(string bookCoverUrl,
        List<Models.External.BookCoverDto> bookCovers)
    {
        var httpClient = _httpClientFactory.CreateClient();
        var response = await httpClient.GetAsync(bookCoverUrl);

        var bookCover = JsonSerializer.Deserialize<
            Models.External.BookCoverDto>(
                      await response.Content.ReadAsStringAsync(),
                      new JsonSerializerOptions
                      {
                          PropertyNameCaseInsensitive = true,
                      });

        if (bookCover != null)
        {
            bookCovers.Add(bookCover);
        }
    }

    public async Task<IEnumerable<Models.External.BookCoverDto>> GetBookCoversProcessAfterWaitForAllAsync(
        Guid bookId)
    {
        var httpClient = _httpClientFactory.CreateClient();
        var bookCovers = new List<Models.External.BookCoverDto>();

        // create a list of fake bookcovers
        var bookCoverUrls = new[]
        {
                $"http://localhost:52644/api/bookcovers/{bookId}-dummycover1",
                $"http://localhost:52644/api/bookcovers/{bookId}-dummycover2",
                $"http://localhost:52644/api/bookcovers/{bookId}-dummycover3",
                $"http://localhost:52644/api/bookcovers/{bookId}-dummycover4",
                $"http://localhost:52644/api/bookcovers/{bookId}-dummycover5"
            };

        var bookCoverTasks = new List<Task<HttpResponseMessage>>();
        foreach (var bookCoverUrl in bookCoverUrls)
        {
            bookCoverTasks.Add(httpClient.GetAsync(bookCoverUrl));
        };

        // wait for all tasks to be completed
        var bookCoverTasksResults = await Task.WhenAll(bookCoverTasks);
        // run through the results in reverse order 
        foreach (var bookCoverTaskResult in bookCoverTasksResults.Reverse())
        {
            var bookCover = JsonSerializer.Deserialize<Models.External.BookCoverDto>(
                await bookCoverTaskResult.Content.ReadAsStringAsync(),
                new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true,
                });

            if (bookCover != null)
            {
                bookCovers.Add(bookCover);
            }
        }
        return bookCovers;
    }

    public IEnumerable<Book> GetBooks()
    { 
        return _context.Books
            .Include(b => b.Author)
            .ToList();
    }

    public async Task<IEnumerable<Book>> GetBooksAsync()
    { 
        return await _context.Books
            .Include(b => b.Author)
            .ToListAsync();
    }

    public IAsyncEnumerable<Book> GetBooksAsAsyncEnumerable()
    {
        return _context.Books.AsAsyncEnumerable<Book>();
    }


    public async Task<IEnumerable<Book>> GetBooksAsync(IEnumerable<Guid> bookIds)
    {
        return await _context.Books
            .Where(b => bookIds.Contains(b.Id))
            .Include(b => b.Author)
            .ToListAsync();
    }

    public async Task<bool> SaveChangesAsync()
    {
        // return true if 1 or more entities were changed
        return (await _context.SaveChangesAsync() > 0);
    }   
}
