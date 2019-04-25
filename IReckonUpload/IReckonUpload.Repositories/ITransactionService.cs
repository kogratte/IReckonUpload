using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace IReckonUpload.DAL
{
    public interface ITransactionService
    {
        Task ExecuteAsync(Func<DbContext, Task> command);
    }
}