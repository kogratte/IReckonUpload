using Hangfire;
using Hangfire.Server;
using IReckonUpload.DAL;
using IReckonUpload.Models.Business;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace IReckonUpload.Business.Jobs
{
    public interface IStoreIntoDatabase
    {
        Task Execute(string temporaryFilename, PerformContext context);
    }

    /// <summary>
    /// Using hangfire, there is an automatic retry.
    /// Each time the delay is increased.
    /// Default retry is 10.
    /// </summary>
    public class StoreIntoDatabase : IStoreIntoDatabase
    {
        private readonly IJobStatusManagementService _jobStatusManagementService;
        private readonly IFileToModelConverter _converter;
        private readonly ILogger<StoreIntoDatabase> _logger;
        private readonly ITransactionService _transactionService;

        public StoreIntoDatabase(ITransactionService transationService, 
            ILogger<StoreIntoDatabase> logger,
            IFileToModelConverter converter,
            IJobStatusManagementService jobStatusManagementService)
        {
            _jobStatusManagementService = jobStatusManagementService;
            _converter = converter;
            _logger = logger;
            _transactionService = transationService;
        }

        [AutomaticRetry(Attempts = 3)]
        public async Task Execute(string temporaryFileLocation, PerformContext context)
        {
            try
            {
                // Using an adapter, it's really easy to make the storage SOLID!
                await this._transactionService.ExecuteAsync(async dbCtx =>
                {
                    var dbSet = dbCtx.Set<Product>();

                    await _converter.DoFromFile(temporaryFileLocation, async (product) =>
                    {
                        dbSet.Add(product);
                    });
                });

                await _jobStatusManagementService.MarkJobAsDone(context.BackgroundJob.Id);
            } catch (Exception e)
            {
                _logger.LogError(e.Message, e);

                throw;
            }
        }
    }
}
