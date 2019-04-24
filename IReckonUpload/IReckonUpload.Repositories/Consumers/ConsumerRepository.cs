using IReckonUpload.Models.Consumers;
using Microsoft.EntityFrameworkCore;

namespace IReckonUpload.DAL.Consumers
{
    public class ConsumerRepository : GenericRepository<Consumer>
    {
        public ConsumerRepository(DbContext dbContext): base(dbContext)
        {
        }
    }
}
