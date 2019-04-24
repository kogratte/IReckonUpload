using System.IO;
using Microsoft.AspNetCore.WebUtilities;

namespace IReckonUpload.Uploader
{
    public class UploadedItem : IUploadedItem
    {
        public bool IsMultipart { get; set; }

        public KeyValueAccumulator? Parts { get; set; }

        public Stream FileStream { get; set; }
    }
}
