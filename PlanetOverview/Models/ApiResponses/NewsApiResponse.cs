namespace PlanetOverview.Models.ApiResponses;

public class NewsApiResponse
{
    public string Status { get; set; } = string.Empty;
    public int TotalResults { get; set; }
    public List<NewsArticle> Articles { get; set; } = new();
}

public class NewsArticle
{
    public NewsSource? Source { get; set; }
    public string? Author { get; set; }
    public string? Title { get; set; }
    public string? Description { get; set; }
    public string? Url { get; set; }
    public string? UrlToImage { get; set; }
    public string? PublishedAt { get; set; }
    public string? Content { get; set; }
}

public class NewsSource
{
    public string? Id { get; set; }
    public string? Name { get; set; }
}