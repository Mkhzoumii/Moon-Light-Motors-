using AutoMapper;
using FINALPROJECT.Data;
using FINALPROJECT.DTO;
using FINALPROJECT.model;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
namespace FINALPROJECT.Controllers;
[ApiController]
[Route("[controller]")]
public class CarsForRentController : Controller
{
    RentCarInterFace _rentCarsRepasory;
    private DataCountextDapper _dapper;
    Mapper mapper;
    public CarsForRentController(IConfiguration confg, RentCarInterFace rentCarsRepasory)
    {
        _dapper = new DataCountextDapper(confg);
        mapper = new Mapper(new MapperConfiguration(cfg =>
        {
            cfg.CreateMap<CarBriefForRent, CarsForRent>();
        }));
        _rentCarsRepasory = rentCarsRepasory;
    }
    [HttpGet("GetCars")]
    public List<CarBriefForRent> GetCarsForRent()
    {
        var cars = _rentCarsRepasory.GetCarsForRent();

        // If the user is not found, return a 404 Not Found
        if (cars == null)
        {
            throw new Exception("not found");
        }
        return cars;
    }
    [HttpGet("GetCarNames")]
    public ActionResult<IEnumerable<string>> GetCarNames(string query)
    {
        if (string.IsNullOrWhiteSpace(query))
        {
            return BadRequest("Search query cannot be empty.");
        }

        // Replace with your actual logic to fetch car names from the database
#pragma warning disable CS8602 // Dereference of a possibly null reference.
        var carNames = _rentCarsRepasory
        .GetCarsForRent()  // Get list of cars (from your database or repository)
        .Where(car => car.carname.Contains(query, StringComparison.OrdinalIgnoreCase))  // Filter by query
        .Select(car => car.carname)
        .ToList();
#pragma warning restore CS8602 // Dereference of a possibly null reference.

        return Ok(carNames);  // Return matching car names
    }
    [HttpGet("SearchCars")]
    public ActionResult<IEnumerable<CarBriefForRent>> SearchCars(string carName)
    {
        if (string.IsNullOrWhiteSpace(carName))
        {
            return BadRequest("Car name is required for the search.");
        }

        // Fetch cars that match the search term (case insensitive)
#pragma warning disable CS8602 // Dereference of a possibly null reference.
        var cars = _rentCarsRepasory.GetCarsForRent()
            .Where(car => car.carname.Contains(carName, StringComparison.OrdinalIgnoreCase))
            .ToList();
#pragma warning restore CS8602 // Dereference of a possibly null reference.

        if (cars.Count == 0)
        {
            return NotFound("No cars found matching the search term.");
        }

        return Ok(cars);  // Return the matched cars
    }


    [HttpGet("GetSinglecars/{carid}")]
    public CarBriefForRent GetSinglecars(int carid)
    {
        var car = _rentCarsRepasory.GetCarById(carid);
        if (car == null)
        {
            throw new Exception("not found");
        }
        return car;
    }
    [HttpPost("addCar")]
public async Task<IActionResult> AddCar([FromForm] AddCarForRentDto car)
{
    if (car.images == null || !car.images.Any())
    {
        return BadRequest("At least one car image is required.");
    }

    // إنشاء كيان السيارة
    var carEntity = new CarsForRent
    {
        carname = car.carname,
        model = car.model,
        price = car.price,
        color = car.color,
        body = car.body
    };

    try
    {
        // إضافة السيارة إلى قاعدة البيانات
        _rentCarsRepasory.AddCar(carEntity);
        if (!_rentCarsRepasory.SaveChanges())
        {
            throw new Exception("Failed to add car.");
        }

        // تحميل الصور المتعددة وتخزينها في قاعدة البيانات
        foreach (var image in car.images)
        {
            using var memoryStream = new MemoryStream();
            await image.CopyToAsync(memoryStream);

            var carImage = new CarForRentImages
            {
                carid = carEntity.carid,  // ربط الصورة بالسيارة باستخدام carid
                CarImage = memoryStream.ToArray()  // تحويل الصورة إلى باينري (byte array)
            };

            _rentCarsRepasory.AddCarForRentImage(carImage);
        }

        if (!_rentCarsRepasory.SaveChanges())
        {
            throw new Exception("Failed to add car images.");
        }

        return Ok(new { message = "Car and images added successfully", carId = carEntity.carid });
    }
    catch (Exception ex)
    {
        return StatusCode(500, new 
        { 
            error = ex.Message, 
            innerException = ex.InnerException?.Message 
        });
    }
}
 [HttpGet("SortCars")]
    public ActionResult<IEnumerable<CarBriefForRent>> SortCars([FromQuery] string sortOrder)
    {
        // Check for valid sort order
        if (string.IsNullOrEmpty(sortOrder) || (sortOrder.ToLower() != "asc" && sortOrder.ToLower() != "desc"))
        {
            return BadRequest("Invalid sortOrder parameter. Use 'asc' or 'desc'.");
        }

        // Fetch the cars
        var cars = _rentCarsRepasory.GetCarsForRent();

        // Sort the cars based on sortOrder
        var sortedCars = sortOrder.ToLower() == "asc"
            ? cars.OrderBy(car => car.price).ToList()
            : cars.OrderByDescending(car => car.price).ToList();

        // Return sorted cars
        return Ok(sortedCars);
    }
    [HttpPost("AddRental")]
    public async Task<IActionResult> AddRental([FromBody] PaymentDto request)
    {
        return await _rentCarsRepasory.AddRental(request);
    }
   [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteCar(int id)
    {
        // التحقق من تعارض الإيجارات
        bool hasConflicts = await _rentCarsRepasory.HasActiveOrFutureRentals(id);
        if (hasConflicts)
        {
            return BadRequest(new
            {
                Message = "Car cannot be deleted because it has active or future rentals."
            });
        }

        // حذف الصور المرتبطة بالسيارة
        bool imagesDeleted =  _rentCarsRepasory.DeleteCarImages(id);
        if (!imagesDeleted)
        {
            return StatusCode(500, "Failed to delete car images.");
        }

        // حذف السيارة
        bool carDeleted =  _rentCarsRepasory.DeleteCar(id);
        if (!carDeleted)
        {
            return NotFound("Car not found.");
        }

        return Ok("Car and associated images deleted successfully.");
    }

    [HttpPost("CheckRentalConflict")]
    public async Task<IActionResult> CheckRentalConflict([FromBody] RentRequest request)
    {
        return await _rentCarsRepasory.CheckRentalConflict(request);
    }
    [HttpPut("edit-car/{id}")]
    public async Task<IActionResult> EditCarForRent(int id, [FromBody] CarBriefForRent request)
    {
        if (request == null)
        {
            return BadRequest("Invalid car details provided.");
        }

        // Call the repository method to update the car details
        var success = await _rentCarsRepasory.EditCarForRentAsync(id, request);

        if (!success)
        {
            return NotFound($"Car with ID {id} not found.");
        }

        return Ok(new { Message = "Car details updated successfully!" });
    }
[HttpGet("get-images-by-carid/{carid}")]
public IActionResult GetImagesByCarId(int carid)
{
    try
    {
        // الحصول على جميع الصور المرتبطة بالسيارة بناءً على carid
        var carImages = _rentCarsRepasory.GetImagesByCarId(carid);

        if (carImages == null || !carImages.Any())
        {
            return NotFound(new { message = "No images found for the specified car ID." });
        }

        // استخراج imageId من الصور المرفوعة
        var imageIds = carImages.Select(img => img.ImageId).ToList();

        return Ok(new { carid = carid, imageIds });
    }
    catch (Exception ex)
    {
        return StatusCode(500, new { error = ex.Message });
    }
}
[HttpGet("GetRental")]
public List<RentRequest> GetRental()
{
    var rentals = _rentCarsRepasory.GetRental();

    // If no rentals are found, return a 404 Not Found
    if (rentals == null)
    {
        throw new Exception("not found");
    }
    return rentals;
}
    
[HttpGet("get-image-by-id/{imageId}")]
public IActionResult GetImageById(int imageId)
{
    try
    {
        // استرجاع الصورة من قاعدة البيانات باستخدام imageId
        var image = _rentCarsRepasory.GetImageById(imageId);

        if (image == null)
        {
            return NotFound(new { message = "Image not found for the specified imageId." });
        }

            // إرجاع الصورة كـ byte array (باينري)
#pragma warning disable CS8604 // Possible null reference argument.
            return File(image.CarImage, "image/jpeg");  // يمكن تغيير الـ MIME Type حسب نوع الصورة
#pragma warning restore CS8604 // Possible null reference argument.
        }
    catch (Exception ex)
    {
        return StatusCode(500, new { error = ex.Message });
    }}
    }