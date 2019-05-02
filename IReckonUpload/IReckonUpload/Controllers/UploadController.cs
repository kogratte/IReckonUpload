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
                    // The temporary file should be stored in a client specific folder.
                    string jsonFilePath = _appConfig.JsonStorageDirectory + "/" + Guid.NewGuid() + ".json";

                    // What?
                    // Hangfire, a job management system.
                    // Why?
                    // Cause it allow us to respond to the customer as soon as possible, not with a final response, but with a status.
                    // An endpoint to track progression should be added.
                    // If the job fail, we're allowed to see it, see why, requeue it, redeploy a fresh binary with mandatory fixes and replay.
                    // The used importer is very specific, but we can imagine a mechanism where the used importer is defined from customer configuration.
                    var importJobId = _hangfire.BackgroundJobClient.Enqueue<IImportContentFromFile>(x => x.Execute(file.TemporaryLocation, jsonFilePath));

                    // The job is enqueued, and THEN the job is added to DB. But, what if the transaction failed?
                    // The job is runned anyway, without any possibility to track it for
                    // the end user.
                    // I did not found a way to get the jobId inside the transaction. I need it to do the insertion.
                    // The catch allow us to handle gracefully any error with the transaction, and warn the user that something goes wrong.
                    try
                    {
                        this._transactionService.ExecuteAsync((dbCtx) =>
                        {
                            var uploadedFile = new UploadedFile
                            {
                                TempFilePath = file.TemporaryLocation,
                                JsonFilePath = jsonFilePath,
                                // Why a list? Not mandatory. At first look I did not use middlewares, but a pair of tasks.
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
                        // If something goes wrong, remove the job, rethrow the error.
                        _hangfire.BackgroundJobClient.Delete(importJobId);

                        throw;
                    }
                });
                // Here we send the response to the consumer, with the id of the importing task. Using another endpoint, he should be able to track progression.
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
