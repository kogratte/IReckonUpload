using IReckonUpload.Business.ModelConverter.Middlewares;
using IReckonUpload.Models.Business;
using System;
using System.Threading.Tasks;

namespace IReckonUpload.Business.ModelConverter.Core
{
    public interface IFileToModelConverter
    {
        Task ProcessFromFile(string temporaryFileLocation, Action<Product> process);
        Task ProcessFromFile(string temporaryFileLocation);

        IFileToModelConverter UseMiddleware<T>(Action<T> configure) where T : IFileToModelConverterBaseMiddleware;
        IFileToModelConverter UseMiddleware<T>() where T : IFileToModelConverterBaseMiddleware;
    }
}
