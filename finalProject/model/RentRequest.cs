namespace FINALPROJECT.model
{
  public class RentRequest
  {
    public int rentid { get; set; }
    public int carid { get; set; }
    public int UserID { get; set; }
    public DateTime start_rent { get; set; }
    public DateTime end_rent { get; set; }
    public decimal price { get; set; }

  }
  

}