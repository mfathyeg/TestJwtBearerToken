using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace TestJwtBearerToken.Data
{
    public class MyIdentityContext : IdentityDbContext<MyUser>
    {
        public MyIdentityContext(DbContextOptions<MyIdentityContext> options, IConfiguration configuration) : base(options)
        {
            this.configuration = configuration;
        }

        private readonly IConfiguration configuration;


        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                optionsBuilder.UseSqlServer(configuration.GetConnectionString("TestJwtDbConnectionSTring"));
            }
        }

        public DbSet<SystemRefreshToken> SystemRefreshTokens { get; set; }
    }
}
