using System.Collections.Generic;
using Microsoft.Extensions.Primitives;

namespace IReckonUpload.Uploader
{
    public interface IUploadResult
    {
        IEnumerable<IMultipartFileInfo> Files { get; set; }
        IDictionary<string, StringValues> Model { get; set; }
    }
}