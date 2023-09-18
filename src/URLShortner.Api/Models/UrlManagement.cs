namespace URLShortner.Api.Models
{
    public record UrlManagement(Guid Id = default, string ShortUrl = "", string LongUrl = "");
}
