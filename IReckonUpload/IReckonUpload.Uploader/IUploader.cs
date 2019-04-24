using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;

namespace IReckonUpload.Uploader
{
    public interface IUploader
    {
        Task<IUploadedItem> UploadFromStreamAsync(HttpRequest request, HttpContext httpContext, Microsoft.AspNetCore.Http.Features.FormOptions formOptions);
    }
}
