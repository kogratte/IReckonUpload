using Hangfire;
using IReckonUpload.DAL;
using IReckonUpload.Models.Internal;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace IReckonUpload.Business.Jobs
{
    public interface IDeleteTemporaryFile
    {
        Task Execute();
    }

    public class DeleteTemporaryFile : IDeleteTemporaryFile
    {
        private readonly ITransactionService _transactionService;
        private readonly IJobStatusManagementService _jobStatusManagementService;

        public DeleteTemporaryFile(IJobStatusManagementService jobStatusManagementService, ITransactionService transactionService)
        {
            _transactionService = transactionService;
            _jobStatusManagementService = jobStatusManagementService;
        }

        [AutomaticRetry(Attempts = 0)]
        public async Task Execute()
        {
            var filesToDelete = new List<string>();
            var doneJobs = _jobStatusManagementService.GetDoneJobs();
            if (doneJobs.Any())
            {
                doneJobs.ToList().ForEach(u => filesToDelete.Add(string.Copy(u.TempFilePath)));

                await _transactionService.ExecuteAsync((dbCtx) => {
                    var dbSet = dbCtx.Set<UploadedFile>();
                    dbSet.RemoveRange(doneJobs);
                    return Task.CompletedTask;
                });
                filesToDelete.ForEach(file => File.Delete(file));
            }
        }
    }
}
