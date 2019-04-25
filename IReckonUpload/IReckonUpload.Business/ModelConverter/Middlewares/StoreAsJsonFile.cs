using IReckonUpload.Models.Business;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace IReckonUpload.Business.ModelConverter.Middlewares
{
    public interface IStoreAsJsonFile : IFileToModelOnRead
    {
        void SetTargetFile(string target);
    }

    public class StoreAsJsonFile : IStoreAsJsonFile
    {
        private readonly JsonSerializerSettings _jsonSerializerSettings;
        private string targetFile { get; set; }

        public StoreAsJsonFile(JsonSerializerSettings settings)
        {
            _jsonSerializerSettings = settings;

        }

        public Task OnRead(Product product)
        {
            var jsonProduct = JsonConvert.SerializeObject(product, Formatting.Indented, _jsonSerializerSettings);
            Write(jsonProduct + ",");

            return Task.CompletedTask;
        }

        public Task OnDone()
        {
            Write("]");

            return Task.CompletedTask;
        }

        private void Write(string v)
        {
            if (string.IsNullOrWhiteSpace(targetFile))
            {
                throw new NullReferenceException(nameof(targetFile));
            }

            using (var tw = new StreamWriter(targetFile, true, Encoding.UTF8, 65536))
            {
                tw.WriteLine(v);
            }
        }

        public void SetTargetFile(string target)
        {
            this.targetFile = target;
            File.Delete(this.targetFile);
            Write("[");
        }
    }
}
