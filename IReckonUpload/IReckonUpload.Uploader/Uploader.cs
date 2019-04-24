using IReckonUpload.Tools;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Net.Http.Headers;
using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace IReckonUpload.Uploader
{
    public class Uploader : IUploader
    {
        public async Task<IUploadedItem> UploadFromStreamAsync(HttpRequest request, HttpContext httpContext, FormOptions formOptions)
        {
            
            MediaTypeHeaderValue contentType = MediaTypeHeaderValue.Parse(request.ContentType);
            var boundary = MultipartRequestHelper.GetBoundary(contentType, formOptions.MultipartBoundaryLengthLimit);

            var reader = new MultipartReader(boundary, httpContext.Request.Body);

            MultipartSection section = await reader.ReadNextSectionAsync();

            var uploadedItem = new UploadedItem();

            while (section != null)
            {
                ContentDispositionHeaderValue contentDisposition;
                var hasContentDispositionHeader = ContentDispositionHeaderValue.TryParse(section.ContentDisposition, out contentDisposition);
                
                if (hasContentDispositionHeader)
                {
                    if (MultipartRequestHelper.HasFileContentDisposition(contentDisposition))
                    {
                        uploadedItem.FileStream = new MemoryStream();

                        await section.Body.CopyToAsync(uploadedItem.FileStream);
                    }
                    else if (MultipartRequestHelper.HasFormDataContentDisposition(contentDisposition))
                    {
                        uploadedItem.IsMultipart = true;
                        uploadedItem.Parts = uploadedItem.Parts ?? new KeyValueAccumulator();

                        var key = HeaderUtilities.RemoveQuotes(contentDisposition.Name);
                        var encoding = GetEncoding(section);
                        using (var streamReader = new StreamReader(
                            section.Body,
                            encoding,
                            detectEncodingFromByteOrderMarks: true,
                            bufferSize: 1024,
                            leaveOpen: true))
                        {
                            var value = await streamReader.ReadToEndAsync();
                            if (string.Equals(value, "undefined", StringComparison.OrdinalIgnoreCase))
                            {
                                value = string.Empty;
                            }

                            uploadedItem.Parts?.Append(key.Value, value);

                            if (uploadedItem.Parts?.ValueCount > formOptions.ValueCountLimit)
                            {
                                throw new InvalidDataException($"Form key count limit {formOptions.ValueCountLimit} exceeded.");
                            }
                        }
                    }
                }
                
                section = await reader.ReadNextSectionAsync();
            }

            return uploadedItem;
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
