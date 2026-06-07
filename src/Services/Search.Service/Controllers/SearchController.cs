using Microsoft.AspNetCore.Mvc;
using Search.Service.Models;
using Search.Service.Services;

namespace Search.Service.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SearchController : ControllerBase
{
    private readonly ISearchService _searchService;

    public SearchController(ISearchService searchService)
    {
        _searchService = searchService;
    }

    [HttpPost]
    public async Task<IActionResult> Search([FromBody] SearchRequest request)
    {
        var results = await _searchService.SearchAsync(request.Query, request.Page, request.PageSize);
        return Ok(results);
    }

    [HttpPost("index")]
    public async Task<IActionResult> Index([FromBody] object document)
    {
        await _searchService.IndexDocumentAsync(document);
        return Ok();
    }
}
