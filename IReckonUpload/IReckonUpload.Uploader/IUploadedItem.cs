using Microsoft.AspNetCore.WebUtilities;
using System;
using System.IO;

namespace IReckonUpload.Uploader
{
    public interface IUploadedItem
    {
        /// <summary>
        /// True if file was uploaded with form/multipart-data
        /// </summary>
        bool IsMultipart { get; }

        /// <summary>
        /// Filled only if file has been uploaded with form/multipart. Otherwise, null
        /// </summary>
        KeyValueAccumulator? Parts { get; }

        /// <summary>
        /// If file has been uploaded without multipart, then the filestream is available
        /// </summary>
        Stream FileStream { get; }
    }
}
