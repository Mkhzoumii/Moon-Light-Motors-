namespace FINALPROJECT.DTO
{
    public class MaintenanceDto
    {
        public int maintenance_id { get; set; }
        public string? maintenance_type { get; set; }
        public int UserID { get; set; }
        public DateTime start_date { get; set; }
        public DateTime end_date { get; set; }

    }
}