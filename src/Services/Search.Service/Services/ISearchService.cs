using Search.Service.Models;

namespace Search.Service.Services;

public interface ISearchService
{
    Task<List<SearchResult>> SearchAsync(string query, int page, int pageSize);
    Task IndexDocumentAsync(object document);
}
