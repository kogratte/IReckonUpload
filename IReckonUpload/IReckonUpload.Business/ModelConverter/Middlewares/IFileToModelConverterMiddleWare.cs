using IReckonUpload.Models.Business;
using System.Threading.Tasks;

namespace IReckonUpload.Business.ModelConverter.Middlewares
{
    public interface IFileToModelConverterBaseMiddleware
    {
        Task OnDone();
    }

    public interface IFileToModelOnRun : IFileToModelConverterBaseMiddleware
    {
        Task OnRun(string sourceFile);
    }

    public interface IFileToModelOnRead : IFileToModelConverterBaseMiddleware
    {
        Task OnRead(Product product);
    }

    public interface IFileToModelOnColorSearch: IFileToModelConverterBaseMiddleware
    {
        Color Search(Color color);
    }

    public interface IFileToModelOnRangeSearch: IFileToModelConverterBaseMiddleware
    {
        DeliveryRange Search(DeliveryRange range);
    }
}
