namespace FINALPROJECT.model
{
    public class MaintenanceDetales
    {
        public int detail_id { get; set; }
        public string? maintenance_type { get; set; }
        public decimal maintenance_price { get; set; }
        public TimeSpan maintenance_time { get; set; } 
    }
}