using IReckonUpload.Models.Internal;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace IReckonUpload.Business
{
    public interface IJobStatusManagementService
    {
        Task MarkJobAsDone(string jobId);

        IEnumerable<UploadedFile> GetDoneJobs();
    }
}
