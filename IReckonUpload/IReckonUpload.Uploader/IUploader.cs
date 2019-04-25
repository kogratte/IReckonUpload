using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using System.Threading.Tasks;

namespace IReckonUpload.Uploader
{
    public interface IUploader
    {
        Task<IUploadResult> UploadFromStreamAsync(HttpRequest request, HttpContext httpContext, FormOptions formOptions);
    }
}
