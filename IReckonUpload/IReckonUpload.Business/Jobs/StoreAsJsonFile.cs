using Hangfire;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IReckonUpload.Business.Jobs
{
    public interface IStoreAsJsonFile
    {
        Task Execute(string temporaryFileLocation, string targetFileName);
    }

    public class StoreAsJsonFile : IStoreAsJsonFile
    {
        private readonly JsonSerializerSettings _jsonSerializerSettings;
        private readonly IFileToModelConverter _converter;
        private readonly ILogger<StoreIntoDatabase> _logger;
        private readonly Guid uid;

        public StoreAsJsonFile(ILogger<StoreIntoDatabase> logger, IFileToModelConverter converter, JsonSerializerSettings jsonSerializerSettings)
        {
            _jsonSerializerSettings = jsonSerializerSettings;
            _converter = converter;
            _logger = logger;

            this.uid = Guid.NewGuid();
        }

        [AutomaticRetry(Attempts = 3)]
        public async Task Execute(string temporaryFileLocation, string targetFilename)
        {
      
            var products = await this._converter.GetFromFile(temporaryFileLocation);
            try
            {
                using (var fs = new FileStream(targetFilename, FileMode.Create))
                {
                    fs.Write(Encoding.UTF8.GetBytes("[" + Environment.NewLine));

                    foreach (var product in products)
                    {
                        var jsonProduct = JsonConvert.SerializeObject(product, Formatting.Indented, _jsonSerializerSettings);
                        await fs.WriteAsync(Encoding.UTF8.GetBytes(jsonProduct + "," + Environment.NewLine));
                    }

                    await fs.WriteAsync(Encoding.UTF8.GetBytes("]" + Environment.NewLine));
                }
                _logger.LogInformation($"{products.Count()} products written to {targetFilename}");
            }
            catch (Exception e)
            {
                _logger.LogError(e.Message, e);
                File.Delete(targetFilename);
            }
        }
    }
}
