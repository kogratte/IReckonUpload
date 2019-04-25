using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations.Schema;

namespace IReckonUpload.Models.Internal
{
    public class StoreTask
    {
        [JsonIgnore]
        public int Id { get; set; }
        public string JobId { get; set; }

        [JsonIgnore]
        public virtual UploadedFile UploadedFile { get; set; }
        [ForeignKey("UploadedFile")]
        [JsonIgnore]
        public int UploadedFileId { get; set; }

        public bool IsDone { get; set; }
    }
}
