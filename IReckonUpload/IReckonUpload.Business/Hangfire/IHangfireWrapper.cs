using Hangfire;

namespace IReckonUpload.Business.Hangfire
{
    public interface IHangfireWrapper
    {
        IBackgroundJobClient BackgroundJobClient { get; }
    }
}
