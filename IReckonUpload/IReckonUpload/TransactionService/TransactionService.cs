using IReckonUpload.DAL;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading.Tasks;

namespace IReckonUpload.TransactionService
{
    public class TransactionService : ITransactionService
    {
        private readonly DbContext _dbContext;

        public TransactionService(AppDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task ExecuteAsync(Func<DbContext, Task> command)
        {
            using (var transaction = _dbContext.Database.BeginTransaction())
            {
                try
                {
                    await command(_dbContext);
                    await _dbContext.SaveChangesAsync();

                    transaction.Commit();
                }
                catch (Exception e)
                {
                    transaction.Rollback();

                    throw e;
                }
            }
        }
    }
}
