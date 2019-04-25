using Newtonsoft.Json;

namespace IReckonUpload.Models.Business
{
    public class Product
    {
        [JsonIgnore]
        public int Id { get; set; }

        public string Key { get; set; }

        public string ArticleCode { get; set; }

        public virtual Color Color { get; set; }

        public string Description { get; set; }

        public double Price { get; set; }

        public double DiscountedPrice { get; set; }

        public virtual DeliveryRange DeliveredIn { get; set; }

        public string Q1 { get; set; }

        // Can be something like 80A if we're talking about bra...
        public string Size { get; set; }
    }
}
