using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace IReckonUpload.DAL
{
    public class ConsumerRepository : IConsumerRepository
    {
        private readonly DbSet<IConsumer> _consumers;

        public ConsumerRepository(DbContext dbContext)
        {
            _consumers = dbContext.Set<IConsumer>();
        }

        public IConsumer Find(string username, string password)
        {
            return _consumers.SingleOrDefault(c => c.Username == username && c.Password == password);
        }
    }
}
