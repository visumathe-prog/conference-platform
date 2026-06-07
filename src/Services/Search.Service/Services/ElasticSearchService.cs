using Elastic.Clients.Elasticsearch;
using Search.Service.Models;

namespace Search.Service.Services;

public class ElasticSearchService : ISearchService
{
    private readonly ElasticsearchClient _client;
    private readonly string _index;

    public ElasticSearchService(IConfiguration configuration)
    {
        var url = configuration["Elasticsearch:Url"] ?? "http://localhost:9200";
        _index = configuration["Elasticsearch:Index"] ?? "events";
        _client = new ElasticsearchClient(new Uri(url));
    }

    public async Task<List<SearchResult>> SearchAsync(string query, int page, int pageSize)
    {
        var response = await _client.SearchAsync<object>(s => s
            .Index(_index)
            .From((page - 1) * pageSize)
            .Size(pageSize)
            .Query(q => q
                .MultiMatch(m => m
                    .Fields(f => f.Field("title").Field("description"))
                    .Query(query)
                )
            )
        );

        return response.Hits.Select(h => new SearchResult
        {
            Id = h.Id,
            Score = h.Score ?? 0,
            Source = h.Source
        }).ToList();
    }

    public async Task IndexDocumentAsync(object document)
    {
        await _client.IndexAsync(document, idx => idx.Index(_index));
    }
}
