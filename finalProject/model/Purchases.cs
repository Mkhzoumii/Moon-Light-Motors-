namespace FINALPROJECT.model
{
    public class Purchases
    {
        public int? PurchaseID { get; set; }
        public int? userId { get; set; }
        public int? spareId { get; set; }
        public string? sparePartName { get; set; }
        public decimal Price { get; set; }
    }
}