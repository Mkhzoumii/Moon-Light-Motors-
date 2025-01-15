using FINALPROJECT.model;
using FINALPROJECT.DTO;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
namespace FINALPROJECT.Data
{
    public class CarsRepastory : CarsinterfaceRepastory
    {
        private DataCountextEf _entityFramWork;
        private DataCountextDapper _dapper;

        public CarsRepastory(IConfiguration confg)
        {
            _entityFramWork = new DataCountextEf(confg);
            _dapper = new DataCountextDapper(confg);
        }

        public bool SaveChanges()
        {
            return _entityFramWork.SaveChanges() > 0;
        }
        public CarForSaleImage GetImageById(int imageId)
        {
#pragma warning disable CS8604 // Possible null reference argument.
#pragma warning disable CS8603 // Possible null reference return.
            return _entityFramWork.CarForSaleImage
                       .FirstOrDefault(img => img.ImageId == imageId);  // البحث عن الصورة بناءً على imageId
#pragma warning restore CS8603 // Possible null reference return.
#pragma warning restore CS8604 // Possible null reference argument.
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

        public bool DeleteCarImages(int carId)
        {
            // افترض أن لديك كود EF Core هنا للتعامل مع قاعدة البيانات
#pragma warning disable CS8604 // Possible null reference argument.
            var images = _entityFramWork.CarForSaleImage.Where(img => img.CarID == carId).ToList();
#pragma warning restore CS8604 // Possible null reference argument.

            if (images == null || images.Count == 0)
            {
                return false; // الصور غير موجودة
            }

            _entityFramWork.CarForSaleImage.RemoveRange(images);
            _entityFramWork.SaveChanges();

            return true;
        }
        public async Task<bool> HasActiveOrFutureRentals(int carId)
        {
#pragma warning disable CS8604 // Possible null reference argument.
            return await _entityFramWork.RentRequest
            .AnyAsync(r => r.carid == carId && r.end_rent > DateTime.UtcNow);
#pragma warning restore CS8604 // Possible null reference argument.
        }

        public void AddCarSaleImage(CarForSaleImage carImage)
        {
#pragma warning disable CS8602 // Dereference of a possibly null reference.
            _entityFramWork.CarForSaleImage.Add(carImage);
#pragma warning restore CS8602 // Dereference of a possibly null reference.
        }
        public IEnumerable<CarForSaleImage> GetImagesByCarId(int CarID)
        {
#pragma warning disable CS8604 // Possible null reference argument.
            return _entityFramWork.CarForSaleImage
                       .Where(img => img.CarID == CarID)
                       .ToList();
#pragma warning restore CS8604 // Possible null reference argument.
        }

        // Method to get all cars
        public List<CarBrief> GetAvailableCars()
        {
            // Get cars with UserID = null (not rented)
#pragma warning disable CS8604 // Possible null reference argument.
            List<CarBrief> availableCars = _entityFramWork.Cars
        .Where(x => x.UserID == null) // Filter for cars that are not rented
        .Select(x => new CarBrief()
        {
            CarID = x.CarID,
            CarDetails = x.CarDetails,
            CarModel = x.CarModel,
            CarName = x.CarName,
            CarPrice = x.CarPrice,
            UserID = x.UserID,
            DrivType = x.DrivType,
            Fule = x.Fule,
            Transnmission = x.Transnmission,
            Power = x.Power,
            Consumption = x.Consumption
        })
        .ToList();
#pragma warning restore CS8604 // Possible null reference argument.

            return availableCars;
        }


        // Method to add a car
        public void AddCar(Cars car)
        {
#pragma warning disable CS8602 // Dereference of a possibly null reference.
            _entityFramWork.Cars.Add(car);
#pragma warning restore CS8602 // Dereference of a possibly null reference.
        }
        // Method to get a car by its ID
        public CarBrief GetCarById(int carId)
        {
#pragma warning disable CS8604 // Possible null reference argument.
            var car = _entityFramWork.Cars
                .Where(x => x.CarID == carId)
                .Select(x => new CarBrief()
                {
                    CarID = x.CarID,
                    CarDetails = x.CarDetails,
                    CarModel = x.CarModel,
                    CarName = x.CarName,
                    CarPrice = x.CarPrice,
                    DrivType = x.DrivType,
                    Fule = x.Fule,
                    Transnmission = x.Transnmission,
                    Power = x.Power,
                    Consumption = x.Consumption
                })
                .FirstOrDefault();
#pragma warning restore CS8604 // Possible null reference argument.

#pragma warning disable CS8603 // Possible null reference return.
            return car;
#pragma warning restore CS8603 // Possible null reference return.
        }

        // New method to delete a car by ID
        public bool DeleteCar(int carId)
        {
            // Find the car by ID
#pragma warning disable CS8602 // Dereference of a possibly null reference.
            var carEntity = _entityFramWork.Cars.Find(carId);
#pragma warning restore CS8602 // Dereference of a possibly null reference.

            if (carEntity != null)
            {
                _entityFramWork.Cars.Remove(carEntity); // Remove the car from the DB
                return SaveChanges(); // Commit the changes to the database
            }

            return false;
        }
        public async Task<IActionResult> AddSale([FromBody] PaymentfrosaleDto request)
        {
            // Validate the incoming request
            if (request == null || request.carid <= 0 || request.UserID <= 0 ||
                request.price <= 0 || string.IsNullOrWhiteSpace(request.payment_type) ||
                request.payment_date == DateTime.MinValue)
            {
                return BadRequest("Invalid sale details provided.");
            }

            // Retrieve car details for sale
#pragma warning disable CS8602 // Dereference of a possibly null reference.
            var car = await _entityFramWork.Cars.FindAsync(request.carid);
#pragma warning restore CS8602 // Dereference of a possibly null reference.
            if (car == null)
            {
                return NotFound("Car not found.");
            }

            // Check if the car is already sold
            if (car.UserID != null && car.UserID != 0)
            {
                return Conflict("This car has already been sold.");
            }

            // Retrieve the user
#pragma warning disable CS8602 // Dereference of a possibly null reference.
            var user = await _entityFramWork.User.FindAsync(request.UserID);
#pragma warning restore CS8602 // Dereference of a possibly null reference.
            if (user == null)
            {
                return NotFound("User not found.");
            }

            // Create and save the sale record
            var sale = new saleRequest
            {
                carid = request.carid,
                UserID = request.UserID,
                sale_date = request.payment_date
            };

#pragma warning disable CS8602 // Dereference of a possibly null reference.
            _entityFramWork.saleRequest.Add(sale);
#pragma warning restore CS8602 // Dereference of a possibly null reference.
            await _entityFramWork.SaveChangesAsync();

            // Update the car's UserID to reflect the sale
            car.UserID = request.UserID;
            _entityFramWork.Cars.Update(car);  // Ensure you're referencing the correct table (Cars or RentCar)
            await _entityFramWork.SaveChangesAsync();

            // Create the payment info and add it to the database
            var payment = new Payment
            {
                carid = request.carid,
                UserID = request.UserID,
                payment_date = request.payment_date,
                price = request.price,
                payment_type = request.payment_type
            };

            try
            {
#pragma warning disable CS8602 // Dereference of a possibly null reference.
                _entityFramWork.Payment.Add(payment);
#pragma warning restore CS8602 // Dereference of a possibly null reference.
                await _entityFramWork.SaveChangesAsync();
            }
            catch (DbUpdateException ex)
            {
                var innerExceptionMessage = ex.InnerException?.Message ?? "No inner exception.";
                var fullExceptionMessage = $"{ex.Message}. Inner exception: {innerExceptionMessage}";
                return StatusCode(500, $"An error occurred while saving payment: {fullExceptionMessage}");
            }

            return Ok(new { Message = "Sale processed and payment added successfully!", SaleId = sale.saleid });
        }


        private IActionResult Ok(object value)
        {
            return new JsonResult(value) { StatusCode = 200 };  // HTTP 200 OK
        }

        private IActionResult BadRequest(string v)
        {
            return new JsonResult(new { Message = v }) { StatusCode = 400 };  // HTTP 400 Bad Request
        }

        private IActionResult Conflict(string v)
        {
            return new JsonResult(new { Message = v }) { StatusCode = 409 };  // HTTP 409 Conflict
        }

        private IActionResult NotFound(string v)
        {
            return new JsonResult(new { Message = v }) { StatusCode = 404 };  // HTTP 404 Not Found
        }

        private IActionResult StatusCode(int v1, string v2)
        {
            return new JsonResult(new { Message = v2 }) { StatusCode = v1 };  // Custom status code
        }
        public async Task<bool> EditSaleCar(int carId, CarBrief request)
        {
            // Retrieve the car to be updated
#pragma warning disable CS8602 // Dereference of a possibly null reference.
            var car = await _entityFramWork.Cars.FindAsync(carId);
#pragma warning restore CS8602 // Dereference of a possibly null reference.
            if (car == null)
            {
                return false;  // Car not found
            }

            // Update car properties with the new values from the request
            car.CarName = request.CarName ?? car.CarName;
            car.CarModel = request.CarModel ?? car.CarModel;
            car.CarPrice = request.CarPrice > 0 ? request.CarPrice : car.CarPrice;
            car.CarDetails = request.CarDetails ?? car.CarDetails;
            car.Consumption = request.Consumption ?? car.Consumption;
            car.Fule = request.Fule ?? car.Fule;
            car.DrivType = request.DrivType ?? car.DrivType;
            car.Power = request.Power ?? car.Power;
            car.Transnmission = request.Transnmission ?? car.Transnmission;


            // Save the changes to the database
            try
            {
#pragma warning disable CS8602 // Dereference of a possibly null reference.
                _entityFramWork.Cars.Update(car);
#pragma warning restore CS8602 // Dereference of a possibly null reference.
                await _entityFramWork.SaveChangesAsync();
                return true;  // Successfully updated
            }
            catch
            {
                return false;  // If any error occurs during the save
            }
        }
    
     public List<CarBrief> GetAllCars()
        {
            // Get cars with UserID = null (not rented)
#pragma warning disable CS8604 // Possible null reference argument.
            List<CarBrief> availableCars = _entityFramWork.Cars.Select(x => new CarBrief()
        {
            CarID = x.CarID,
            CarDetails = x.CarDetails,
            CarModel = x.CarModel,
            CarName = x.CarName,
            CarPrice = x.CarPrice,
            UserID = x.UserID,
            DrivType = x.DrivType,
            Fule = x.Fule,
            Transnmission = x.Transnmission,
            Power = x.Power,
            Consumption = x.Consumption
        })
        .ToList();
#pragma warning restore CS8604 // Possible null reference argument.

            return availableCars;
        }

}
}