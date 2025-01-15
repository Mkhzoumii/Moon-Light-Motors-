namespace FINALPROJECT.DTO
{
    public class AddCarForRentDto
    {
        public string? carname { get; set; }
        public string? model { get; set; }
        public decimal price { get; set; }
    public IEnumerable<IFormFile>? images { get; set; } // تعديل هنا لقبول مجموعة من الصور
        public string? color { get; set; }
        public string? body { get; set; }



    }
}