namespace Search.Service.Models;

public class SearchResult
{
    public string Id { get; set; } = string.Empty;
    public double Score { get; set; }
    public object? Source { get; set; }
}
