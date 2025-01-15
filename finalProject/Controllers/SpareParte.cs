using AutoMapper;
using FINALPROJECT.Data;
using FINALPROJECT.DTO;
using FINALPROJECT.model;
using Microsoft.AspNetCore.Mvc;
namespace FINALPROJECT.Controllers;
[ApiController]
[Route("[controller]")]
public class SpareParteController : Controller
{
    SparePartInterface _sparePart;
    private DataCountextDapper _dapper;
    Mapper mapper;
    public SpareParteController(IConfiguration confg, SparePartInterface sparePart)
    {
        _dapper = new DataCountextDapper(confg);
        mapper = new Mapper(new MapperConfiguration(cfg =>
        {
            cfg.CreateMap<SparePartBrifDto, SparePart>();
        }));
        _sparePart = sparePart;
    }
    [HttpPost("AddSparePart")]
    public async Task<IActionResult> AddSparePart([FromForm] AddSparePartDto Spare)
    {
        //  AddCarDto cars = mapper.Map<AddCarDto>(car);//هاي طريقه ال mapper

        if (Spare.SpareImage == null || Spare.SpareImage.Length == 0)
        {
            return BadRequest("SpareParte image is required.");
        }

        using var memoryStream = new MemoryStream();
        await Spare.SpareImage.CopyToAsync(memoryStream);

        var carEntity = new SparePart
        {
            SpareName = Spare.SpareName,
            SparePrice = Spare.SparePrice,
            SpareModel = Spare.SpareModel,
            Qty = Spare.Qty,
            SpareImage = memoryStream.ToArray() // Save image as byte array
        };

        _sparePart.AddSpareParte(carEntity);
        if (_sparePart.SaveChanges())
        {
            return Ok();
        }
        throw new Exception("falid to spare parte");
    }
    [HttpGet("GetSpareParte")]
    public List<SparePartBrifDto> GetSpareParte()
    {
        var spare = _sparePart.GetAllSpareParte();

        // If the user is not found, return a 404 Not Found
        if (spare == null)
        {
            throw new Exception("not found");
        }
        return spare;
    }
    [HttpGet("GetSpareImage/{SpareId}")]
    public IActionResult GetSpareImage(int SpareId)
    {
        string userQuery = $@"
        SELECT [SpareImage]
        FROM FinalProject.SpareParts
        WHERE SpareId = " + SpareId;

        var spare = _dapper.loadDataSingle<byte[]>(userQuery);  // Pass just the SQL query as a string

        // If the user is not found, return a 404 Not Found
        if (spare == null)
        {
            throw new Exception("not found");
        }

        return File(spare, "image/jpeg"); // or the appropriate MIME type
    }
   [HttpPut("edit-car/{spareId}")]
    public async Task<IActionResult> EditCarForRent(int spareId, [FromBody] SparePartBrifDto request)
    {
        if (request == null)
        {
            return BadRequest("Invalid car details provided.");
        }

        // Call the repository method to update the car details
        var success = await _sparePart.EditSparePartAsync(spareId, request);

        if (!success)
        {
            return NotFound($"Spare Part with ID {spareId} not found.");
        }

        return Ok(new { Message = "Spare Part details updated successfully!" });
    }
    [HttpGet("GetSparePartById")]
    public SparePartBrifDto? GetSparePartById(int id)
    {

        var spare = _sparePart.GetSparePartById(id);
        if (spare == null)
        {
            throw new Exception("not found");
        }
        return spare;
    }

    [HttpGet("SearchSalCars")]
    public ActionResult<IEnumerable<SparePartBrifDto>> SearchCars(string spareName, string SpareModel)
    {
        // التأكد من وجود قيمة للاسم والطراز
        if (string.IsNullOrWhiteSpace(spareName) && string.IsNullOrWhiteSpace(SpareModel))
        {
            return BadRequest("Car name or model is required for the search.");
        }

        // استرجاع القطع التي تطابق الاسم والطراز (بحث غير حساس لحالة الحروف)
#pragma warning disable CS8602 // Dereference of a possibly null reference.
        var spare = _sparePart.GetAllSpareParte()
            .Where(spare =>
                (string.IsNullOrWhiteSpace(spareName) || spare.SpareName.Contains(spareName, StringComparison.OrdinalIgnoreCase)) &&
                (string.IsNullOrWhiteSpace(SpareModel) || spare.SpareModel.Contains(SpareModel, StringComparison.OrdinalIgnoreCase)))
            .ToList();
#pragma warning restore CS8602 // Dereference of a possibly null reference.

        if (spare.Count == 0)
        {
            return NotFound("No cars found matching the search criteria.");
        }

        return Ok(spare);  // إعادة القطع المطابقة
    }
    [HttpGet("SortSalCars")]
    public ActionResult<IEnumerable<SparePartBrifDto>> SortCars([FromQuery] string sortOrder)
    {
        // Check for valid sort order
        if (string.IsNullOrEmpty(sortOrder) || (sortOrder.ToLower() != "asc" && sortOrder.ToLower() != "desc"))
        {
            return BadRequest("Invalid sortOrder parameter. Use 'asc' or 'desc'.");
        }

        // Fetch the cars
        var cars = _sparePart.GetAllSpareParte();

        // Sort the cars based on sortOrder
        var sortedCars = sortOrder.ToLower() == "asc"
            ? cars.OrderBy(car => car.SparePrice).ToList()
            : cars.OrderByDescending(car => car.SparePrice).ToList();

        // Return sorted cars
        return Ok(sortedCars);
    }
    

    [HttpPost("AddToBasket")]
public IActionResult AddToBasket([FromBody] BasketDto basketDto)
{
    // Validate the input data
    if (basketDto == null || basketDto.spareId == 0 || basketDto.userId == 0)
    {
        return BadRequest("Invalid input data.");
    }

    // Call the repository method to add the item to the basket
    bool isAdded = _sparePart.AddToBasket(basketDto);
    if (!isAdded)
    {
        return BadRequest("The selected spare part is out of stock or could not be added.");
    }

    return Ok("Item added to the basket and stock updated successfully.");
}
[HttpGet("GetBasket")]
public IActionResult GetBasket(int userId)
{
    // احصل على قائمة العناصر في السلة
    var basketItems = _sparePart.GetBasket(userId);

    // إذا كانت السلة فارغة أو null
    if (basketItems == null || !basketItems.Any())
    {
        return Ok(new List<BasketDto>()); // أعد قائمة فارغة
    }

    return Ok(basketItems); // أعد العناصر
}

[HttpDelete("RemoveFromBasket/{spareId}/{userId}")]
public IActionResult RemoveFromBasket(int userId, int spareId)
{
    bool isRemoved = _sparePart.RemoveFromBasket(userId, spareId);

    if (!isRemoved)
    {
        return BadRequest("Failed to remove the item from the basket.");
    }

    return Ok("Item removed from the basket successfully.");
}

[HttpPost("FinalizePurchase/{userId}")]
public IActionResult FinalizePurchase(int userId)
{
    // قم بتثبيت الشراء
    bool isFinalized = _sparePart.FinalizePurchase(userId);

    if (!isFinalized)
    {
        return BadRequest("The basket is empty or purchase could not be finalized.");
    }

    return Ok("Purchase finalized successfully.");
}
[HttpDelete("deleteSparePart/{spareId}")]
public IActionResult DeleteSparePart(int spareId)
{
    bool isDeleted = _sparePart.deleteSparePart(spareId);

    if (!isDeleted)
    {
        return NotFound("Spare part not found.");
    }

    return Ok("Spare part deleted successfully.");
}   }