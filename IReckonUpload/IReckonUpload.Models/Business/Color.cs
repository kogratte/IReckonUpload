using Newtonsoft.Json;
using System.Collections.Generic;

namespace IReckonUpload.Models.Business
{
    public class Color
    {
        [JsonIgnore]
        public int Id { get; set; }
        public string Code { get; set; }
        public string Label { get; set; }

        [JsonIgnore]
        public virtual IEnumerable<Product> Products { get; set; }
    }
}
