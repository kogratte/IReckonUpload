using IReckonUpload.Models.Business;
using Microsoft.EntityFrameworkCore;

namespace IReckonUpload.DAL.Products
{
    public class ProductRepository : GenericRepository<Product>
    {
        public ProductRepository(DbContext dbContext) : base(dbContext)
        {
        }
    }
}
