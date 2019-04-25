using IReckonUpload.DAL;
using IReckonUpload.Models.Business;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace IReckonUpload.Business.Jobs
{
    public interface IStoreIntoDatabase
    {
        Task Execute(string temporaryFilename);
    }

    /// <summary>
    /// Using hangfire, there is an automatic retry.
    /// Each time the delay is increased.
    /// Default retry is 10.
    /// </summary>
    public class StoreIntoDatabase : IStoreIntoDatabase
    {
        private readonly IFileToModelConverter _converter;
        private readonly ILogger<StoreIntoDatabase> _logger;
        private readonly ITransactionService _transactionService;

        public StoreIntoDatabase(ITransactionService transationService, 
            ILogger<StoreIntoDatabase> logger,
            IFileToModelConverter converter)
        {
            _converter = converter;
            _logger = logger;
            _transactionService = transationService;
        }

        public async Task Execute(string temporaryFileLocation)
        {
            try
            {
                // Using an adapter, it's really easy to make the storage SOLID!
                await this._transactionService.ExecuteAsync(async dbCtx =>
                {
                    IEnumerable<Product> products = await _converter.GetFromFile(temporaryFileLocation);

                    var dbSet = dbCtx.Set<Product>();

                    await dbSet.AddRangeAsync(products);
                });
            } catch (Exception e)
            {
                _logger.LogError(e.Message, e);

                throw;
            }
        }
    }
}
