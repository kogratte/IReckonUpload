using IReckonUpload.DAL;
using Microsoft.EntityFrameworkCore;

namespace IReckonUpload
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions options) : base(options)
        {
        }

        public DbSet<IConsumer> Consumers { get; set; }
    }
}
