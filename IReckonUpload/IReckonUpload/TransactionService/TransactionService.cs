using IReckonUpload.DAL;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IReckonUpload.TransactionService
{
    public class Transaction : ITransaction
    {
        private readonly IList<Action<DbContext>> Commands;

        public Transaction()
        {
            Commands = new List<Action<DbContext>>();
        }

        public void Dispose()
        {
            throw new NotImplementedException();
        }

        public void Enqueue(Action<DbContext> todo)
        {
            this.Commands.Add(todo);
        }

        public IEnumerable<Action<DbContext>> GetQueue()
        {
            return this.Commands;
        }
    }

    public class TransactionService : ITransactionService
    {
        private readonly DbContext _dbContext;

        public TransactionService(AppDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public ITransaction BeginTransaction()
        {
            return new Transaction();
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

        public async Task Finalize(ITransaction transaction)
        {
            using (var _dbTransaction = _dbContext.Database.BeginTransaction())
            {
                try
                {
                    transaction.GetQueue().ToList().ForEach(task =>
                    {
                        task.Invoke(_dbContext);
                    });

                    await _dbContext.SaveChangesAsync();

                    _dbTransaction.Commit();
                }
                catch (Exception e)
                {
                    _dbTransaction.Rollback();

                    throw e;
                }
            }
        }
    }
}
