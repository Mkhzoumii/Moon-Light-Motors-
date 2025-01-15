using AutoMapper;
using FINALPROJECT.Data;
using FINALPROJECT.DTO;
using FINALPROJECT.model;
using Microsoft.AspNetCore.Mvc;
namespace FINALPROJECT.Controllers;
[ApiController]
[Route("[controller]")]
public class CarsController : Controller
{
    CarsinterfaceRepastory _carsRepasory;
    private DataCountextDapper _dapper;
    Mapper mapper;
    public CarsController(IConfiguration confg, CarsinterfaceRepastory carsRepasory)
    {
        _dapper = new DataCountextDapper(confg);
        mapper = new Mapper(new MapperConfiguration(cfg =>
        {
            cfg.CreateMap<CarBrief, Cars>();
        }));
        _carsRepasory = carsRepasory;
    }
    [HttpGet("GetAvailableCars")]
    public List<CarBrief> GetCars()
    {
        var cars = _carsRepasory.GetAvailableCars();

        // If the user is not found, return a 404 Not Found
        if (cars == null)
        {
            throw new Exception("not found");
        }
        return cars;
    }

    [HttpGet("GetSinglecars/{carid}")]
    public CarBrief GetSinglecars(int carid)
    {
        var car = _carsRepasory.GetCarById(carid);
        if (car == null)
        {
            throw new Exception("not found");
        }
        return car;
    }
   
  [HttpPost("AddCar")]
public async Task<IActionResult> AddCar([FromForm] AddCarDto car)
{
    if (car.CarImage == null || !car.CarImage.Any())
    {
        return BadRequest("At least one car image is required.");
    }

    // إنشاء كيان السيارة
    var carEntity = new Cars
    {
        CarName = car.CarName,
        CarModel = car.CarModel,
        CarPrice = car.CarPrice,
        CarDetails = car.CarDetails,
        DrivType=car.DrivType,
        Fule=car.Fule,
        Consumption=car.Consumption,
        Power=car.Power,
        Transnmission=car.Transnmission

    };

    try
    {
        // إضافة السيارة إلى قاعدة البيانات
        _carsRepasory.AddCar(carEntity);
        if (!_carsRepasory.SaveChanges())
        {
            throw new Exception("Failed to add car.");
        }

        // تحميل الصور المتعددة وتخزينها في قاعدة البيانات
       foreach (var image in car.CarImage)
{
    using var memoryStream = new MemoryStream();
    await image.CopyToAsync(memoryStream);

    var carImage = new CarForSaleImage
    {
        CarID = carEntity.CarID,  // التأكد من استخدام CarID من السيارة المضافة
        CarImage = memoryStream.ToArray()  // تحويل الصورة إلى باينري (byte array)
    };

    _carsRepasory.AddCarSaleImage(carImage);
}

// حفظ الصور
if (!_carsRepasory.SaveChanges())
{
    throw new Exception("Failed to add car images.");
}

        return Ok(new { message = "Car and images added successfully", CarID = carEntity.CarID });
    }
    catch (Exception ex)
    {
        return StatusCode(500, new 
        { 
            error = ex.Message, 
            innerException = ex.InnerException?.Message 
        });}
}
   [HttpPost("AddSale")]
    public  Task<IActionResult> AddSale([FromBody] PaymentfrosaleDto request)
    {
        return  _carsRepasory.AddSale(request);

    }

    
    [HttpGet("SearchSalCars")]
    public ActionResult<IEnumerable<CarBrief>> SearchCars( string carName)
    {
        if (string.IsNullOrWhiteSpace(carName))
        {
            return BadRequest("Car name is required for the search.");
        }

        // Fetch cars that match the search term (case insensitive)
#pragma warning disable CS8602 // Dereference of a possibly null reference.
        var cars = _carsRepasory.GetAvailableCars()
            .Where(car => car.CarName.Contains(carName, StringComparison.OrdinalIgnoreCase))
            .ToList();
#pragma warning restore CS8602 // Dereference of a possibly null reference.

        if (cars.Count == 0)
        {
            return NotFound("No cars found matching the search term.");
        }

        return Ok(cars);  // Return the matched cars
    }

    [HttpGet("SortSalCars")]
public ActionResult<IEnumerable<CarBrief>> SortCars([FromQuery] string sortOrder)
{
    // Check for valid sort order
    if (string.IsNullOrEmpty(sortOrder) || (sortOrder.ToLower() != "asc" && sortOrder.ToLower() != "desc"))
    {
        return BadRequest("Invalid sortOrder parameter. Use 'asc' or 'desc'.");
    }

    // Fetch the cars
    var cars = _carsRepasory.GetAvailableCars();

    // Sort the cars based on sortOrder
    var sortedCars = sortOrder.ToLower() == "asc"
        ? cars.OrderBy(car => car.CarPrice).ToList()
        : cars.OrderByDescending(car => car.CarPrice).ToList();

    // Return sorted cars
    return Ok(sortedCars);
}
[HttpGet("GetCarSalNames")]
public ActionResult<IEnumerable<string>> GetCarSalNames(string query)
{
    if (string.IsNullOrWhiteSpace(query))
    {
        return BadRequest("Search query cannot be empty.");
    }

        // Replace with your actual logic to fetch car names from the database
#pragma warning disable CS8602 // Dereference of a possibly null reference.
        var carNames = _carsRepasory
        .GetAvailableCars()  // Get list of cars (from your database or repository)
        .Where(car => car.CarName.Contains(query, StringComparison.OrdinalIgnoreCase))  // Filter by query
        .Select(car => car.CarName)
        .ToList();
#pragma warning restore CS8602 // Dereference of a possibly null reference.

        return Ok(carNames);  // Return matching car names
}
   [HttpDelete("{id}")]
public IActionResult DeleteCar(int id)
{
    // حذف الصور المرتبطة أولاً
    bool imagesDeleted = _carsRepasory.DeleteCarImages(id);

    if (!imagesDeleted)
    {
        return NotFound("Car images not found or could not be deleted.");
    }

    // حذف السيارة
    bool deleted = _carsRepasory.DeleteCar(id);

    if (!deleted)
    {
        return NotFound("Car not found.");
    }

    return Ok("Car and its images deleted successfully.");
}

         [HttpPut("edit-car/{id}")]
        public async Task<IActionResult> EditSaleCar(int id, [FromBody] CarBrief request)
        {
            if (request == null)
            {
                return BadRequest("Invalid car details provided.");
            }

            // Call the repository method to update the car details
            var success = await _carsRepasory.EditSaleCar(id, request);
            
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
        var carImages = _carsRepasory.GetImagesByCarId(carid);

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
[HttpGet("get-image-by-id/{imageId}")]
public IActionResult GetImageById(int imageId)
{
    try
    {
        // استرجاع الصورة من قاعدة البيانات باستخدام imageId
        var image = _carsRepasory.GetImageById(imageId);

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
    }
}
[HttpGet("GetAllCars")]
    public List<CarBrief> GetAllCars()
    {
        var cars = _carsRepasory.GetAllCars();

        // If the user is not found, return a 404 Not Found
        if (cars == null)
        {
            throw new Exception("not found");
        }
        return cars;
    }

}
