using Hangfire;

namespace IReckonUpload.Business
{
    public class HangfireWrapper : IHangfireWrapper
    {
        public IBackgroundJobClient BackgroundJobClient => new BackgroundJobClient(JobStorage.Current);
    }
}
