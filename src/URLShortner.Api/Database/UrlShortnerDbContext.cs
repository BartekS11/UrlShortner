using Microsoft.EntityFrameworkCore;
using URLShortner.Api.Models;

namespace URLShortner.Api.Database
{
    public class UrlShortnerDbContext : DbContext
    {
        private readonly IConfiguration _configuration;

        public UrlShortnerDbContext(DbContextOptions options, IConfiguration configuration) : base(options)
        {
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        }

        public DbSet<UrlManagement> Url { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlite(_configuration.GetConnectionString("DefaultConnection"));
            base.OnConfiguring(optionsBuilder);
        }
    }
}
