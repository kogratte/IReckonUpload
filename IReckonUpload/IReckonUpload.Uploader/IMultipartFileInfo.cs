namespace IReckonUpload.Uploader
{
    public interface IMultipartFileInfo
    {
        string FileName { get; }
        long Length { get; }
        string Name { get; }
        string TemporaryLocation { get; }
    }
}
