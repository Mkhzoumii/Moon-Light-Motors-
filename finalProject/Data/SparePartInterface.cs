using FINALPROJECT.model;
using FINALPROJECT.DTO;
using Microsoft.AspNetCore.Mvc;
namespace FINALPROJECT.Data

{
  public interface SparePartInterface
  {
    public bool SaveChanges();
    public bool RemoveEntity<T>(T entityToAdd);
    public bool AddSpareParte<T>(T entityToAdd);
    public List<SparePartBrifDto> GetAllSpareParte();
    public SparePartBrifDto? GetSparePartById(int id);
    public bool UpdateSpareParte(SparePart sparePart);
    public bool AddToBasket(BasketDto basketDto);
    public List<BasketDto> GetBasket(int userId);
    public bool RemoveFromBasket(int userId, int spareId);
    public void RemoveAllFromBasket(int userId);
    public bool FinalizePurchase(int userId);
     public  Task<bool> EditSparePartAsync(int spareId, SparePartBrifDto request);
        public bool deleteSparePart(int spareId);

    }
}