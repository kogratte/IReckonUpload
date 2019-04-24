using IReckonUpload.CustomAttributes;
using IReckonUpload.Tools;
using IReckonUpload.Uploader;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace IReckonUpload.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class UploadController : ControllerBase
    {
        private readonly IUploader _fileUploader;

        // Get the default form options so that we can use them to set the default limits for
        // request body data
        private static readonly FormOptions _defaultFormOptions = new FormOptions();

        public UploadController(IUploader fileUploader)
        {
            _fileUploader = fileUploader;
        }

        // POST api/upload
        [HttpPost]
        [DisableFormValueModelBinding]
        [RequestFormLimits(MultipartBodyLengthLimit = int.MaxValue)]
        [RequestSizeLimit(int.MaxValue)]
        public async Task<IActionResult> Post()
        {
            if (!MultipartRequestHelper.IsMultipartContentType(Request.ContentType))
            {
                return BadRequest();
            }

            IUploadedItem uploadedFile = await _fileUploader.UploadFromStreamAsync(Request, HttpContext, _defaultFormOptions);

            if (uploadedFile.IsMultipart)
            {

            }
            else
            {

            }
            

            return new OkResult();
        }

        
    }
}
