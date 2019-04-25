using IReckonUpload.Models.Business;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace IReckonUpload.Business
{
    public interface IFileToModelConverter
    {
        Task<IEnumerable<Product>> GetFromFile(string temporaryFileLocation);
    }
}
