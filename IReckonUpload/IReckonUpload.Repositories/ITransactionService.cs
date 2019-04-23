using System;
using Microsoft.EntityFrameworkCore;

namespace IReckonUpload.DAL
{
    public interface ITransactionService
    {
        void Execute(Action<DbContext> command);
        void Execute(Action<DbContext> command, Action continueWith);
        void Execute<T>(Func<DbContext, T> command, Action<T> continueWith);
    }
}