using IReckonUpload.DAL;
using IReckonUpload.Models.Internal;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IReckonUpload.Business
{
    public class JobStatusManagementService : IJobStatusManagementService
    {
        private readonly ITransactionService _transactionService;
        private readonly IRepository<UploadedFile> _uploadedFileRepository;

        public JobStatusManagementService(ITransactionService transactionService, IRepository<UploadedFile> uploadedFileRepository)
        {
            _transactionService = transactionService;
            _uploadedFileRepository = uploadedFileRepository;
        }

        public IEnumerable<UploadedFile> GetDoneJobs()
        {
            return _uploadedFileRepository.FindMany(x => x.StoreTasks.All(t => t.IsDone));
        }

        public async Task MarkJobAsDone(string jobId)
        {
            await _transactionService.ExecuteAsync(dbCtx =>
            {
                var dbSet = dbCtx.Set<StoreTask>();
                var currentTask = dbSet.SingleOrDefault(t => t.JobId == jobId);
                if (currentTask != null)
                {
                    currentTask.IsDone = true;
                }
                return Task.CompletedTask;
            });
        }
    }
}
