namespace FINALPROJECT.DTO
{
    public class AddCarDto
    {
        public string? CarName { get; set; }
        public string? CarModel { get; set; }
        public decimal CarPrice { get; set; }
        public IEnumerable<IFormFile>? CarImage  { get; set; }
        public string? CarDetails { get; set; }
         public string? DrivType { get; set; }
        public string? Power { get; set; }
        public string? Fule { get; set; }
        public string? Consumption { get; set; }
        public string? Transnmission { get; set; }



    }
}