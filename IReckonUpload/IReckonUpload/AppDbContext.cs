using IReckonUpload.DAL;
using IReckonUpload.Models.Consumers;
using Microsoft.EntityFrameworkCore;

namespace IReckonUpload
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions options) : base(options)
        {
        }

        public DbSet<Consumer> Consumers { get; set; }
    }
}
