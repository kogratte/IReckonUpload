namespace IReckonUpload.Models.Configuration
{
    public class AppConfigurationOptions
    {
        public JsonWebTokenConfiguration JsonWebTokenConfig { get; set; }

        public string ApplicationName { get; set; }
        public string JsonStorageDirectory { get; set; }
    }
}
