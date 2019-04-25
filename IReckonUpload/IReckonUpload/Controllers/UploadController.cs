using Hangfire;
using IReckonUpload.Business.Hangfire;
using IReckonUpload.DAL;
using IReckonUpload.Jobs;
using IReckonUpload.Models.Configuration;
using IReckonUpload.Models.Internal;
using IReckonUpload.Tools;
using IReckonUpload.Uploader;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
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
        private readonly ITransactionService _transactionService;
        private readonly IHangfireWrapper _hangfire;
        private readonly IUploader _fileUploader;
        private readonly ILogger<UploadController> _logger;
        private readonly AppConfigurationOptions _appConfig;

        // Get the default form options so that we can use them to set the default limits for
        // request body data
        private static readonly FormOptions _defaultFormOptions = new FormOptions();

        public UploadController(ILogger<UploadController> logger, IUploader uploader, IHangfireWrapper hangfire,
            IOptions<AppConfigurationOptions> appConfig,
            ITransactionService transactionService)
        {
            _transactionService = transactionService;
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

                IUploadResult result = await _fileUploader.UploadFromStreamAsync(Request, HttpContext, _defaultFormOptions);
                var uploadedFiles = new List<UploadedFile>();

                result.Files.ToList().ForEach(file =>
                {
                    string jsonFilePath = _appConfig.JsonStorageDirectory + "/" + Guid.NewGuid() + ".json";

                    //var dbStoreJobId = _hangfire.BackgroundJobClient.Enqueue<IStoreIntoDatabase>(x => x.Execute(file.TemporaryLocation, null));
                    //var jsonStoreJobId = _hangfire.BackgroundJobClient.Enqueue<IStoreAsJsonFile>(x => x.Execute(file.TemporaryLocation, jsonFilePath, null));
                    var importJobId = _hangfire.BackgroundJobClient.Enqueue<IImportContentFromFile>(x => x.Execute(file.TemporaryLocation, jsonFilePath));
                    try
                    {
                        this._transactionService.ExecuteAsync((dbCtx) =>
                        {
                            var uploadedFile = new UploadedFile
                            {
                                TempFilePath = file.TemporaryLocation,
                                JsonFilePath = jsonFilePath,
                                StoreTasks = new List<StoreTask>
                                {
                                    new StoreTask { JobId = importJobId }
                                }
                            };

                            uploadedFiles.Add(uploadedFile);
                            dbCtx.Set<UploadedFile>().Add(uploadedFile);

                            return Task.CompletedTask;
                        });
                    }
                    catch (Exception)
                    {
                        _hangfire.BackgroundJobClient.Delete(importJobId);

                        throw;
                    }
                });

                return Ok(JsonConvert.SerializeObject(uploadedFiles));
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
