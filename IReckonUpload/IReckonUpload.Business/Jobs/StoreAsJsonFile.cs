using Hangfire;
using Hangfire.Server;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace IReckonUpload.Business.Jobs
{
    public interface IStoreAsJsonFile
    {
        Task Execute(string temporaryFileLocation, string targetFileName, PerformContext context);
    }

    public class StoreAsJsonFile : IStoreAsJsonFile
    {
        private readonly IJobStatusManagementService _jobStatusManagementService;
        private readonly JsonSerializerSettings _jsonSerializerSettings;
        private readonly IFileToModelConverter _converter;

        public StoreAsJsonFile(IFileToModelConverter converter, 
            JsonSerializerSettings jsonSerializerSettings,
            IJobStatusManagementService jobStatusManagementService)
        {
            _jobStatusManagementService = jobStatusManagementService;
            _jsonSerializerSettings = jsonSerializerSettings;
            _converter = converter;
        }

        [AutomaticRetry(Attempts = 3)]
        public async Task Execute(string temporaryFileLocation, 
            string targetFilename, 
            PerformContext context)
        {
            try
            {
                using (var tw = new StreamWriter(targetFilename, false, Encoding.UTF8, 65536))
                {
                    tw.WriteLine("[");
                    await this._converter.DoFromFile(temporaryFileLocation, async product =>
                    {
                        var jsonProduct = JsonConvert.SerializeObject(product, Formatting.Indented, _jsonSerializerSettings);
                        tw.WriteLine(jsonProduct + ",");
                    });

                    tw.WriteLine("]");
                }

                await _jobStatusManagementService.MarkJobAsDone(context.BackgroundJob.Id);
            }
            catch (Exception)
            {
                File.Delete(targetFilename);

                throw;
            }
        }
    }
}
