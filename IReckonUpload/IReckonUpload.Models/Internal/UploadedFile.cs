using Newtonsoft.Json;
using System.Collections.Generic;

namespace IReckonUpload.Models.Internal
{
    public class UploadedFile
    {
        [JsonIgnore]
        public int Id { get; set; }

        [JsonIgnore]
        public string TempFilePath { get; set; }

        public virtual IEnumerable<StoreTask> StoreTasks { get; set; }

        public string JsonFilePath { get; set; }
    }
}
