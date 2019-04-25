using Hangfire;

namespace IReckonUpload.Business.Hangfire
{
    public class HangfireWrapper : IHangfireWrapper
    {
        public IBackgroundJobClient BackgroundJobClient => new BackgroundJobClient(JobStorage.Current);
    }
}
