namespace FINALPROJECT.DTO
{
    public class PaymentDto
    {
        public DateTime start_rent { get; set; }
         public DateTime end_rent { get; set; }
        public int payment_id { get; set; }
        public int carid { get; set; }
        public int UserID { get; set; }
        public DateTime payment_date { get; set; }
        public decimal price { get; set; }
        public string? payment_type { get; set; }

    }
}