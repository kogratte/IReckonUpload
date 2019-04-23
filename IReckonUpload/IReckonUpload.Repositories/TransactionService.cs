using Microsoft.EntityFrameworkCore;
using System;

namespace IReckonUpload.DAL
{
    public class TransactionService: ITransactionService
    {
        private readonly DbContext _dbContext;

        public TransactionService(DbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public void Execute<T>(Func<DbContext, T> command, Action<T> continueWith)
        {
            using (var transaction = _dbContext.Database.BeginTransaction())
            {
                try
                {
                    T commandResult = command(this._dbContext);
                    _dbContext.SaveChanges();

                    transaction.Commit();

                    continueWith(commandResult);
                }
                catch (Exception e)
                {
                    transaction.Rollback();

                    throw e;
                }
            }
        }

        public void Execute(Action<DbContext> command)
        {
            using (var transaction = _dbContext.Database.BeginTransaction())
            {
                try
                {
                    command(this._dbContext);
                    _dbContext.SaveChanges();

                    transaction.Commit();
                }
                catch (Exception e)
                {
                    transaction.Rollback();

                    throw e;
                }
            }
        }

        public void Execute(Action<DbContext> command, Action continueWith)
        {
            using (var transaction = _dbContext.Database.BeginTransaction())
            {
                try
                {
                    command(this._dbContext);
                    _dbContext.SaveChanges();

                    transaction.Commit();
                    continueWith();
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
