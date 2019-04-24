namespace IReckonUpload.Models.Business
{
    public class Product
    {
        public int Id { get; set; }
        public string Key { get; set; }
        public string Code { get; set; }
        public Color Color { get; set; }
        public string Description { get; set; }
        public double Price { get; set; }
        public double DiscountPrice { get; set; }
        public DeliveryRange DeliveryRange { get; set; }
        public string Q1 { get; set; }
        public int Size { get; set; }
    }

    

   
}
