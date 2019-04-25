using IReckonUpload.DAL;
using IReckonUpload.Models.Business;
using IReckonUpload.Models.Consumers;
using Microsoft.EntityFrameworkCore;

namespace IReckonUpload
{
    public class AppDbContext : DbContext, IAppDbContext
    {
        public AppDbContext(DbContextOptions options) : base(options)
        {
        }

        public DbSet<Consumer> Consumers { get; set; }

        public DbSet<Product> Products { get; set; }
    }
}
