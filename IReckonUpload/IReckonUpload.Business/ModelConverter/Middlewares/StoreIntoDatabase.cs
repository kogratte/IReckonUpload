using IReckonUpload.DAL;
using IReckonUpload.Models.Business;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading.Tasks;

namespace IReckonUpload.Business.ModelConverter.Middlewares
{
    public interface IStoreIntoDatabase: IFileToModelOnRead, IFileToModelOnRun
    {
    }

    /// <summary>
    /// Using hangfire, there is an automatic retry.
    /// Each time the delay is increased.
    /// Default retry is 10.
    /// </summary>
    public class StoreIntoDatabase : IStoreIntoDatabase, IDisposable
    {
        private readonly ITransactionService _transactionService;
        private ITransaction _transaction;
        private readonly IServiceScope scope;

        public StoreIntoDatabase(IServiceProvider serviceProvider)
        {
            this.scope = serviceProvider.CreateScope();
            this._transactionService = this.scope.ServiceProvider.GetRequiredService<ITransactionService>();
        }

        public void Dispose()
        {
            this._transaction.Dispose();
        }

        public Task OnDone()
        {
            _transactionService.Finalize(_transaction);
            return Task.CompletedTask;
        }

        public Task OnRead(Product product)
        {
            _transaction.Enqueue((dbx) =>
            {
                dbx.Set<Product>().Add(product);
            });

            return Task.CompletedTask;
        }

        public Task OnRun(string sourceFile)
        {
            this._transaction = _transactionService.BeginTransaction();
            return Task.CompletedTask;
        }
    }
}
