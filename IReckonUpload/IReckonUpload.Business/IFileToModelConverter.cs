using IReckonUpload.Models.Business;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace IReckonUpload.Business
{
    public interface IFileToModelConverter
    {
        Task<IEnumerable<Product>> GetFromFile(string temporaryFileLocation);
        Task DoFromFile(string temporaryFileLocation, Action<Product> process);
    }
}
