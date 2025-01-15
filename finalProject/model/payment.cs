namespace FINALPROJECT.model
{
    public class Payment
    {
        public int payment_id { get; set; }
        public int carid { get; set; }
        public int UserID { get; set; }
        public DateTime payment_date { get; set; }
        public decimal price { get; set; }
        public string? payment_type { get; set; }
    }

}