using IReckonUpload.Tools;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Net.Http.Headers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace IReckonUpload.Uploader
{
    /// <summary>
    /// This  upload engine as been designed using the official streaming api documentation
    /// </summary>
    public class Uploader : IUploader
    {
        public async Task<IUploadResult> UploadFromStreamAsync(HttpRequest request, HttpContext httpContext, FormOptions formOptions)
        {
            var files = new List<IMultipartFileInfo>();

            MediaTypeHeaderValue contentType = MediaTypeHeaderValue.Parse(request.ContentType);
            var boundary = MultipartRequestHelper.GetBoundary(contentType, formOptions.MultipartBoundaryLengthLimit);

            var reader = new MultipartReader(boundary, httpContext.Request.Body);

            MultipartSection section = await reader.ReadNextSectionAsync();

            var formAccumulator = new KeyValueAccumulator();

            while (section != null)
            {
                ContentDispositionHeaderValue contentDisposition;
                var hasContentDispositionHeader = ContentDispositionHeaderValue.TryParse(section.ContentDisposition, out contentDisposition);
                
                if (hasContentDispositionHeader)
                {
                    if (MultipartRequestHelper.HasFileContentDisposition(contentDisposition))
                    {
                        var formFile = new MultipartFileInfo
                        {
                            Name = section.AsFileSection().Name,
                            FileName = section.AsFileSection().FileName,
                            Length = section.Body.Length,
                        };
                        files.Add(await HandleFileSection(section, formFile));
                    }
                    else if (MultipartRequestHelper.HasFormDataContentDisposition(contentDisposition))
                    {
                        formAccumulator = await AccumulateForm(formAccumulator, section, contentDisposition, formOptions);
                    }
                }
                
                section = await reader.ReadNextSectionAsync();
            }

            // HERE their is something really bad. If my file is large, then the response is large.
            // It may be more efficient to write all bites to disk and to return filestream.
            // Next step? Do research, find an alternative!
            return new UploadResult {
                Model = formAccumulator.GetResults(),
                Files = files
            };
        }
        private async Task<IMultipartFileInfo> HandleFileSection(MultipartSection fileSection, IMultipartFileInfo formFile)
        {
            string targetFilePath;

            targetFilePath = Path.GetTempFileName();

            using (var targetStream = File.Create(targetFilePath))
            {
                await fileSection.Body.CopyToAsync(targetStream);
            }

            var tFormFile = new MultipartFileInfo
            {
                Name = formFile.Name,
                Length = formFile.Length,
                FileName = formFile.FileName,
                TemporaryLocation = targetFilePath
            };

            return tFormFile;
        }

        private async Task<KeyValueAccumulator> AccumulateForm(KeyValueAccumulator formAccumulator, MultipartSection section, ContentDispositionHeaderValue contentDisposition, FormOptions formOptions)
        {
            var key = HeaderUtilities.RemoveQuotes(contentDisposition.Name);
            var encoding = GetEncoding(section);
            // Buffer size may be optimized depending on the running machine?
            using (var streamReader = new StreamReader(
                section.Body,
                encoding,
                detectEncodingFromByteOrderMarks: true,
                bufferSize: 20 * 1024,
                leaveOpen: true))
            {
                var value = await streamReader.ReadToEndAsync();

                if (string.Equals(value, "undefined", StringComparison.OrdinalIgnoreCase))
                {
                    value = string.Empty;
                }

                formAccumulator.Append(key.Value, value);

                if (formAccumulator.ValueCount > formOptions.ValueCountLimit)
                {
                    throw new InvalidDataException($"Form key count limit {formOptions.ValueCountLimit} exceeded.");
                }
            }

            return formAccumulator;
        }

        private static Encoding GetEncoding(MultipartSection section)
        {
            MediaTypeHeaderValue mediaType;
            var hasMediaTypeHeader = MediaTypeHeaderValue.TryParse(section.ContentType, out mediaType);
            // UTF-7 is insecure and should not be honored. UTF-8 will succeed in 
            // most cases.
            if (!hasMediaTypeHeader || Encoding.UTF7.Equals(mediaType.Encoding))

            {
                return Encoding.UTF8;
            }
            return mediaType.Encoding;
        }
    }
}
