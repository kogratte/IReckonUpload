namespace IReckonUpload.Uploader
{

    public class MultipartFileInfo : IMultipartFileInfo
    {
        public string Name { get; internal set; }
        public string FileName { get; internal set; }
        public long Length { get; internal set; }
        public string TemporaryLocation { get; internal set; }
    }
}
