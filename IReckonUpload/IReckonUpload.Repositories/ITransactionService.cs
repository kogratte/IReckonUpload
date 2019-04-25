using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace IReckonUpload.DAL
{
    public interface ITransaction: IDisposable
    {
        void Enqueue(Action<DbContext> todo);
        IEnumerable<Action<DbContext>> GetQueue();

    }

    public interface ITransactionService
    {
        Task ExecuteAsync(Func<DbContext, Task> command);
        ITransaction BeginTransaction();

        Task Finalize(ITransaction transaction);
    }
}