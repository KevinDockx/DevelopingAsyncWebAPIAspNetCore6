using AutoMapper;
using Books.API.Models;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc;

namespace Books.API.Filters;

public class BooksResultFilter : IAsyncResultFilter
{
    private readonly IMapper _mapper;

    public BooksResultFilter(IMapper mapper)
    {
        _mapper = mapper;
    }

    public async Task OnResultExecutionAsync(
        ResultExecutingContext context,
        ResultExecutionDelegate next)
    {
        var resultFromAction = context.Result as ObjectResult;
        if (resultFromAction?.Value == null
            || resultFromAction.StatusCode < 200
            || resultFromAction.StatusCode >= 300)
        {
            await next();
            return;
        }

        resultFromAction.Value = _mapper.Map<IEnumerable<BookDto>>(
            resultFromAction.Value);

        await next();
    }
}
