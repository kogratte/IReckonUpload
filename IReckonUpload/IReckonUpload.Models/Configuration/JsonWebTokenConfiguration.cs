namespace IReckonUpload.Models.Configuration
{
    public class JsonWebTokenConfiguration
    {
        /// <summary>
        /// Passphrase used to encrypt token. NEVER communicate it.
        /// </summary>
        public string Secret { get; set; }

        /// <summary>
        /// The token sender.
        /// </summary>
        public string Issuer { get; set; }

        /// <summary>
        /// Validity of the provided token. In days.
        /// </summary>
        public int Validity { get; set; }
    }
}
