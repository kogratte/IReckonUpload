using Hangfire;

namespace IReckonUpload.Business
{
    public interface IHangfireWrapper
    {
        IBackgroundJobClient BackgroundJobClient { get; }
    }
}
