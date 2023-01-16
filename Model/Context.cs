using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace Overtime.Model
{
    public class Context : DbContext
    {
        private Configuration _configuration;

        public DbSet<UserInformation> UserInformation { get; set; } = null!;

        public Context(IOptions<Configuration> options)
        {
            _configuration = options.Value ??
                throw new ArgumentNullException(nameof(options));
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder) =>
            optionsBuilder.UseSqlite($"Data Source={_configuration.Database}");
    }
}