namespace IReckonUpload.DAL
{
    public interface IConsumerRepository
    {
        IConsumer Find(string username, string password);
    }
}
