using System.Collections.Generic;
using Microsoft.Extensions.Primitives;

namespace IReckonUpload.Uploader
{
    public class UploadResult : IUploadResult
    {
        public IDictionary<string, StringValues> Model { get; set; }
        public IEnumerable<IMultipartFileInfo> Files { get; set; }
    }
}