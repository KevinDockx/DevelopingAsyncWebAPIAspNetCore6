using AutoMapper;
using Books.API.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Books.API.Filters;

public class BookResultFilter : IAsyncResultFilter
{
    private readonly IMapper _mapper;

    public BookResultFilter(IMapper mapper)
    {
        _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
    }

    public async Task OnResultExecutionAsync(
        ResultExecutingContext context, ResultExecutionDelegate next)
    {
        var resultFromAction = context.Result as ObjectResult;
        if (resultFromAction?.Value == null
          || resultFromAction.StatusCode < 200
          || resultFromAction.StatusCode >= 300)
        {
            await next();
            return;
        } 

        resultFromAction.Value = _mapper.Map<BookDto>(resultFromAction.Value);

        await next();
    }
}
