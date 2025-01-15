namespace FINALPROJECT.DTO
{
    public class Maintenance_detailsDto
    {
  
        public int detail_id { get; set; }
        public string? maintenance_type { get; set; }
        public decimal maintenance_price { get; set; }
        public TimeSpan MaintenanceTime { get; set; } 

    }
}