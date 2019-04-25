using Hangfire;
using IReckonUpload.Business;
using IReckonUpload.Business.Jobs;
using IReckonUpload.Models.Configuration;
using IReckonUpload.Tools;
using IReckonUpload.Uploader;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace IReckonUpload.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class UploadController : ControllerBase
    {
        private readonly IHangfireWrapper _hangfire;
        private readonly IUploader _fileUploader;
        private readonly ILogger<UploadController> _logger;
        private readonly AppConfigurationOptions _appConfig;

        // Get the default form options so that we can use them to set the default limits for
        // request body data
        private static readonly FormOptions _defaultFormOptions = new FormOptions();

        public UploadController(ILogger<UploadController> logger, IUploader uploader, IHangfireWrapper hangfire,
            IOptions<AppConfigurationOptions> appConfig)
        {
            _hangfire = hangfire;
            _fileUploader = uploader;
            _logger = logger;
            _appConfig = appConfig?.Value;
        }

        /// <summary>
        /// Send a file using POST before processing it.
        /// </summary>
        [HttpPost]
        [DisableRequestSizeLimit]
        public async Task<IActionResult> Post()
        {
            if (!MultipartRequestHelper.IsMultipartContentType(Request.ContentType))
            {
                return BadRequest();
            }

            try
            {
                EnsureJsonDirectoryExists();
                var jsonFilePath = _appConfig.JsonStorageDirectory + "/" + Guid.NewGuid() + ".json";

                IUploadResult result = await _fileUploader.UploadFromStreamAsync(Request, HttpContext, _defaultFormOptions);
                var ids = new Dictionary<string, Dictionary<string, string>>();

                result.Files.ToList().ForEach(file =>
                {
                    var dbStoreJobId = _hangfire.BackgroundJobClient.Enqueue<IStoreIntoDatabase>(x => x.Execute(file.TemporaryLocation));
                    var jsonStoreJobId = _hangfire.BackgroundJobClient.Enqueue<IStoreAsJsonFile>(x => x.Execute(file.TemporaryLocation, jsonFilePath));

                    ids.Add(file.FileName, new Dictionary<string, string>
                    {
                        { "StoreAsJsonJobID", jsonStoreJobId },
                        { "StoreAsDbJobID", dbStoreJobId },
                        { "JsonLocation", jsonFilePath }
                    });
                });

                return Ok(ids);
            }
            catch (Exception e)
            {
                _logger.LogError(e.Message, e);
                return StatusCode((int)HttpStatusCode.InternalServerError);
            }
        }

        private void EnsureJsonDirectoryExists()
        {
            if (!Directory.Exists(_appConfig.JsonStorageDirectory))
            {
                Directory.CreateDirectory(_appConfig.JsonStorageDirectory);
            }
        }
    }
}
