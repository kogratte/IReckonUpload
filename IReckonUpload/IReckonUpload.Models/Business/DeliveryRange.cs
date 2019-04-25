using Newtonsoft.Json;
using System.Collections.Generic;

namespace IReckonUpload.Models.Business
{
    /// <summary>
    /// Based on data I can see, a delivery range is formatted by
    /// %x-%y %z, with x and y numbers, z a unity.
    /// 
    /// Considering I am doing an adapter for a very specific customer, the model
    /// is stucked to the definition he provide, to ensure the communications quality.
    /// 
    /// If we want to make something much more generic, we still can use strings,
    /// but the result gonna be ugly to manipulate / filter / query
    /// </summary>
    public class DeliveryRange
    {
        [JsonIgnore]
        public int Id { get; set; }
        public int RangeStart { get; set; }
        public int RangeEnd { get; set; }
        public string Unit { get; set; }
        public string Raw { get; set; }

        [JsonIgnore]
        public virtual IEnumerable<Product> Products { get; set; }
    }
}
