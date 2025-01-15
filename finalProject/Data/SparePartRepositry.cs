using FINALPROJECT.model;
using FINALPROJECT.DTO;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FINALPROJECT.Data
{
    public class SparePartRepositry :SparePartInterface
    {
        private DataCountextEf _entityFramWork;
        private DataCountextDapper _dapper;

        public SparePartRepositry(IConfiguration confg)
        {
            _entityFramWork = new DataCountextEf(confg);
            _dapper = new DataCountextDapper(confg);
        }

        public bool SaveChanges()
        {
            return _entityFramWork.SaveChanges() > 0;
        }

        public bool RemoveEntity<T>(T entityToRemove)
        {
            if (entityToRemove != null)
            {
                _entityFramWork.Remove(entityToRemove);
                return true;
            }
            return false;
        }
        public bool AddEntity<T>(T entityToAdd)
        {
            if (entityToAdd != null)
            {
                _entityFramWork.Add(entityToAdd);
                return true;
            }
            return false;
        }

  public bool AddSpareParte<T>(T entityToAdd)
        {
            if (entityToAdd != null)
            {
                _entityFramWork.Add(entityToAdd);
                return true;
            }
            return false;
        }
        
        public List<SparePartBrifDto> GetAllSpareParte()
{
    #pragma warning disable CS8604 // Possible null reference argument.
    return _entityFramWork.SparePart
        .Where(x => x.Qty > 0) // تصفية القطع التي تحتوي على كمية أكبر من صفر
        .Select(x => new SparePartBrifDto()
        {
            SpareId = x.SpareId,
            SpareName = x.SpareName,
            SparePrice = x.SparePrice,
            SpareModel = x.SpareModel,
            Qty = x.Qty,
        }).ToList();
    #pragma warning restore CS8604 // Possible null reference argument.
}

        public SparePartBrifDto? GetSparePartById(int id)
        {
#pragma warning disable CS8604 // Possible null reference argument.
            var sparePart = _entityFramWork.SparePart.FirstOrDefault(x => x.SpareId == id);
#pragma warning restore CS8604 // Possible null reference argument.

            if (sparePart == null)
    {
        return null; // Return null if no matching SparePart is found
    }

    // Map SparePart to SparePartBrifDto
    return new SparePartBrifDto
    {
        SpareId = sparePart.SpareId,
        SpareName = sparePart.SpareName,
        SparePrice = sparePart.SparePrice,
        SpareModel = sparePart.SpareModel,
        Qty = sparePart.Qty
    };
        }
        public bool UpdateSpareParte(SparePart sparePart)
        {
            _entityFramWork.Entry(sparePart).State = EntityState.Modified;
            return true;
        }
      public bool AddToBasket(BasketDto basketDto)
{
            // Fetch the spare part from the database
#pragma warning disable CS8604 // Possible null reference argument.
            var sparePart = _entityFramWork.SparePart.FirstOrDefault(sp => sp.SpareId == basketDto.spareId);
#pragma warning restore CS8604 // Possible null reference argument.
            if (sparePart == null || sparePart.Qty <= 0)
    {
        return false; // Indicating failure, as the part is not available
    }

    // Decrease the quantity in stock
    sparePart.Qty -= 1;

    // Create a new basket item
    var basketItem = new Basket
    {
        spareId = basketDto.spareId,
        userId = basketDto.userId,
        qty = basketDto.qty,
        sparePartName = basketDto.sparePartName,
        sparePrice = basketDto.sparePrice
    };

            // Add the item to the basket
#pragma warning disable CS8602 // Dereference of a possibly null reference.
            _entityFramWork.Basket.Add(basketItem);
#pragma warning restore CS8602 // Dereference of a possibly null reference.

            // Save changes to the database
            return _entityFramWork.SaveChanges() > 0;
}
 public async Task<bool> EditSparePartAsync(int spareId, SparePartBrifDto request)
        {
            // Retrieve the car to be updated
#pragma warning disable CS8602 // Dereference of a possibly null reference.
            var sparePart = await _entityFramWork.SparePart.FindAsync(spareId); 
#pragma warning restore CS8602 // Dereference of a possibly null reference.
            if (sparePart == null)
            {
                return false;  // Car not found
            }

            // Update car properties with the new values from the request
            sparePart.SpareName = request.SpareName ?? sparePart.SpareName;
            sparePart.SpareModel = request.SpareModel ?? sparePart.SpareModel;
            sparePart.SparePrice = request.SparePrice > 0 ? request.SparePrice : sparePart.SparePrice;
            sparePart.Qty = request.Qty > 0 ? request.Qty : sparePart.Qty;

            // Save the changes to the database
            try
            {
#pragma warning disable CS8602 // Dereference of a possibly null reference.
                _entityFramWork.SparePart.Update(sparePart);
#pragma warning restore CS8602 // Dereference of a possibly null reference.
                await _entityFramWork.SaveChangesAsync();
                return true;  // Successfully updated
            }
            catch
            {
                return false;  // If any error occurs during the save
            }
        }

       public List<BasketDto> GetBasket(int userId)
{
            // Retrieve the basket items for the specified userId
#pragma warning disable CS8604 // Possible null reference argument.
            var basketItems = _entityFramWork.Basket
                                      .Where(b => b.userId == userId)  // Filter by userId
                                      .ToList();
#pragma warning restore CS8604 // Possible null reference argument.

            // Optionally, you can convert it to BasketDto if needed
            var basketDtoList = basketItems.Select(b => new BasketDto
    {
        spareId = b.spareId,
        sparePartName = b.sparePartName,
        sparePrice = b.sparePrice,
        qty = b.qty,
        userId = b.userId
    }).ToList();

    return basketDtoList;
}
public bool RemoveFromBasket(int userId, int spareId)
{
            // Retrieve the basket item for the specified userId and spareId
#pragma warning disable CS8604 // Possible null reference argument.
            var basketItem = _entityFramWork.Basket
                                   .FirstOrDefault(b => b.userId == userId && b.spareId == spareId);
#pragma warning restore CS8604 // Possible null reference argument.

            if (basketItem != null)
    {
        _entityFramWork.Basket.Remove(basketItem);
        _entityFramWork.SaveChanges();

        return true;
    }

    return false;
}
public bool FinalizePurchase(int userId)
{
    // احصل على السلة الخاصة بالمستخدم
    var basketItems = GetBasket(userId);
    if (basketItems == null || !basketItems.Any())
    {
        return false; // السلة فارغة
    }

    foreach (var item in basketItems)
    {
        // إنشاء سجل جديد في جدول الشراء
        var purchase = new Purchases
        {
            userId = userId,
            spareId = item.spareId,
            sparePartName = item.sparePartName,
            Price = item.sparePrice
        };
#pragma warning disable CS8602 // Dereference of a possibly null reference.
                _entityFramWork.Purchases.Add(purchase);
#pragma warning restore CS8602 // Dereference of a possibly null reference.
            }

    // إزالة العناصر من السلة بعد تثبيت الشراء
    RemoveAllFromBasket(userId);

    // حفظ التغييرات
    _entityFramWork.SaveChanges();
    return true;
}
public void RemoveAllFromBasket(int userId)
{
#pragma warning disable CS8604 // Possible null reference argument.
            var basketItems = _entityFramWork.Basket.Where(b => b.userId == userId).ToList();
#pragma warning restore CS8604 // Possible null reference argument.
            if (basketItems.Any())
    {
        _entityFramWork.Basket.RemoveRange(basketItems);
        _entityFramWork.SaveChanges();
    }
}

      public bool deleteSparePart(int spareId)
{
    #pragma warning disable CS8604 // Possible null reference argument.
    // البحث عن جميع السجلات المرتبطة بالقطعة في جدول المبيعات
    var purchases = _entityFramWork.Purchases.Where(p => p.spareId == spareId).ToList();
    
    // حذف جميع السجلات المرتبطة من جدول المبيعات
    if (purchases != null && purchases.Any())
    {
        _entityFramWork.Purchases.RemoveRange(purchases);
    }

    // البحث عن القطعة في جدول القطع
    var sparePart = _entityFramWork.SparePart.FirstOrDefault(x => x.SpareId == spareId);
    #pragma warning restore CS8604 // Possible null reference argument.

    // حذف القطعة إذا وجدت
    if (sparePart != null)
    {
        _entityFramWork.SparePart.Remove(sparePart);
        _entityFramWork.SaveChanges();
        return true;
    }

    return false;
}

        }}