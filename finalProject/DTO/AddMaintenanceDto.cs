namespace FINALPROJECT.DTO
{
    public class AddMaintenanceDto
    {
        internal decimal price;

        public string? maintenance_type { get; set; }
        public int UserID { get; set; }
        public DateTime start_date { get; set; }
        public DateTime end_date { get; set; }

    }
}