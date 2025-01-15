namespace FINALPROJECT.DTO
{
  public class SparePartDto
  {
  public int SpareId { get; set; }

   
    public string? SpareName { get; set; }

 
    public IFormFile?  SpareImage { get; set; }

    public decimal SparePrice { get; set; }

   
    public string? SpareModel { get; set; }

    public int Qty { get; set; }
  }

}