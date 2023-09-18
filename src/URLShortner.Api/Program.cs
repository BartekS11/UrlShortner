using Microsoft.EntityFrameworkCore;
using URLShortner.Api.Constants;
using URLShortner.Api.Database;
using URLShortner.Api.Models;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddDbContext<UrlShortnerDbContext>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.MapPost("/shorter", async (UrlDto url, UrlShortnerDbContext dbContext, HttpContext ctx, CancellationToken token) =>
{
    var random = new Random();
    if (!IsValidUrl(url.LongUrl))
    {
        throw new Exception($"Url is not valid {url}");
    }

    var randomString = new string(Enumerable.Repeat(ConfigConstants.Letters, 8)
        .Select(x => x[random.Next(x.Length)]).ToArray());

    var shortUrl = new UrlManagement()
    {
        Id = Guid.NewGuid(),
        LongUrl = url.LongUrl,
        ShortUrl = randomString.ToString(),
    };

    await dbContext.Url.AddAsync(shortUrl, cancellationToken: token);
    await dbContext.SaveChangesAsync(cancellationToken: token);

    var newShortUrl = $"{ctx.Request.Scheme}://{ctx.Request.Host}/{shortUrl.ShortUrl}";

    return Results.Ok(new ShortUrlResponseDto(newShortUrl)
    { 
        ShortUrl = newShortUrl,
    }
    );
}
);

app.MapFallback(async (UrlShortnerDbContext dbContext, HttpContext ctx, CancellationToken token) =>
{
    var path = ctx.Request.Path.ToUriComponent().Trim('/');

    var matchUrl = await dbContext.Url.FirstOrDefaultAsync(x => 
        x.ShortUrl.Trim() == path.Trim(), 
        cancellationToken: token);

    if(matchUrl == null)
    {
        return Results.BadRequest();
    }

    return Results.Redirect(matchUrl.LongUrl);
});

app.Run();

static bool IsValidUrl(string Url)
{
    return Uri.TryCreate(Url, UriKind.Absolute, out var uriResult);
}