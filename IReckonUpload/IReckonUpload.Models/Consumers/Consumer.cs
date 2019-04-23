using IReckonUpload.DAL;

namespace IReckonUpload.Models.Consumers
{
    public class Consumer : IConsumer
    {
        public string Username {get; set; }
        public string Password { get; set; }
    }
}
