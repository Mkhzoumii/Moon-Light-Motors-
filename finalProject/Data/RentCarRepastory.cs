using FINALPROJECT.model;
using FINALPROJECT.DTO;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Net.Http.Json;

namespace FINALPROJECT.Data
{
    public class RentCarRepastory : RentCarInterFace
    {
        private DataCountextEf _entityFramWork;
        private DataCountextDapper _dapper;

        public RentCarRepastory(IConfiguration confg)
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
         public void AddCarForRentImage(CarForRentImages carImage)
    {
#pragma warning disable CS8602 // Dereference of a possibly null reference.
            _entityFramWork.CarForRentImages.Add(carImage);
#pragma warning restore CS8602 // Dereference of a possibly null reference.
        }
         public IEnumerable<CarForRentImages> GetImagesByCarId(int carid)
    {
#pragma warning disable CS8604 // Possible null reference argument.
            return _entityFramWork.CarForRentImages
                       .Where(img => img.carid == carid)
                       .ToList();
#pragma warning restore CS8604 // Possible null reference argument.
        }
         public CarForRentImages GetImageById(int imageId)
    {
#pragma warning disable CS8604 // Possible null reference argument.
#pragma warning disable CS8603 // Possible null reference return.
            return _entityFramWork.CarForRentImages
                       .FirstOrDefault(img => img.ImageId == imageId);  // البحث عن الصورة بناءً على imageId
#pragma warning restore CS8603 // Possible null reference return.
#pragma warning restore CS8604 // Possible null reference argument.
        }


        public List<CarBriefForRent> GetCarsForRent()
        {
#pragma warning disable CS8604 // Possible null reference argument.
            return _entityFramWork.CarsForRent.Select(x => new CarBriefForRent()
            {
                carid = x.carid,
                carname = x.carname,
                model = x.model,
                price = x.price,
                color=x.color,
                body=x.body
            }).ToList();
#pragma warning restore CS8604 // Possible null reference argument.
        }

public bool DeleteCarImages(int carId)
{
            // افترض أن لديك كود EF Core هنا للتعامل مع قاعدة البيانات
#pragma warning disable CS8604 // Possible null reference argument.
            var images = _entityFramWork.CarForRentImages.Where(img => img.carid == carId).ToList();
#pragma warning restore CS8604 // Possible null reference argument.

            if (images == null || images.Count == 0)
    {
        return false; // الصور غير موجودة
    }

    _entityFramWork.CarForRentImages.RemoveRange(images);
    _entityFramWork.SaveChanges();

    return true;
}




        public bool AddCar<T>(T entityToAdd)
        {
            if (entityToAdd != null)
            {
                _entityFramWork.Add(entityToAdd);
                return true;
            }
            return false;
        }

        public CarBriefForRent? GetCarById(int carId)
        {
#pragma warning disable CS8604 // Possible null reference argument.
            return _entityFramWork.CarsForRent
                .Where(x => x.carid == carId)
                .Select(x => new CarBriefForRent()
                {
                    carid = x.carid,
                    model = x.model,
                    carname = x.carname,
                    price = x.price,
                    body=x.body,
                    color=x.color
                })
                .FirstOrDefault();
#pragma warning restore CS8604 // Possible null reference argument.
        }

        // Add the DeleteCar method
        public bool DeleteCar(int carId)
        {
            // Find the car by ID
#pragma warning disable CS8602 // Dereference of a possibly null reference.
            var carEntity = _entityFramWork.CarsForRent.Find(carId);
#pragma warning restore CS8602 // Dereference of a possibly null reference.

            if (carEntity != null)
            {
                _entityFramWork.CarsForRent.Remove(carEntity); // Remove the car from the DB
                return SaveChanges(); // Commit the changes to the database
            }

            return false;
        }

        public async Task<bool> HasActiveOrFutureRentals(int carId)
    {
#pragma warning disable CS8604 // Possible null reference argument.
            return await _entityFramWork.RentRequest
            .AnyAsync(r => r.carid == carId && r.end_rent > DateTime.UtcNow);
#pragma warning restore CS8604 // Possible null reference argument.
        }

public async Task<IActionResult> AddRental([FromBody] PaymentDto request)
{
    // Validate the incoming request
    if (request == null || request.carid <= 0 || request.UserID <= 0 ||
        request.start_rent == DateTime.MinValue || request.end_rent == DateTime.MinValue ||
        request.price <= 0 || string.IsNullOrWhiteSpace(request.payment_type) || 
        request.payment_date == DateTime.MinValue)
    {
        return new BadRequestObjectResult("Invalid rental details provided.");
    }

    // Check if the start date is in the past
    if (request.start_rent < DateTime.UtcNow)
    {
        return new BadRequestObjectResult("The start date cannot be in the past.");
    }

            // Retrieve car details
#pragma warning disable CS8602 // Dereference of a possibly null reference.
            var car = await _entityFramWork.CarsForRent.FindAsync(request.carid);
#pragma warning restore CS8602 // Dereference of a possibly null reference.
            if (car == null)
    {
        return new NotFoundObjectResult("Car not found.");
    }

            // Check for overlapping rental periods
#pragma warning disable CS8604 // Possible null reference argument.
            var overlappingRentals = await _entityFramWork.RentRequest
        .Where(r => r.carid == request.carid &&
                    ((request.start_rent >= r.start_rent && request.start_rent <= r.end_rent) ||
                     (request.end_rent >= r.start_rent && request.end_rent <= r.end_rent) ||
                     (request.start_rent <= r.start_rent && request.end_rent >= r.end_rent)))
        .ToListAsync();
#pragma warning restore CS8604 // Possible null reference argument.

            if (overlappingRentals.Any())
    {
        return new ConflictObjectResult("The car is already rented during the requested period.");
    }

    // Create and save the rental
    var rental = new RentRequest
    {
        carid = request.carid,
        UserID = request.UserID,
        start_rent = request.start_rent,
        end_rent = request.end_rent,
        price = request.price,  
    };

    _entityFramWork.RentRequest.Add(rental);
    await _entityFramWork.SaveChangesAsync();

    // Create the payment info and add it to the database
    var payment = new Payment
    {
        carid = request.carid,
        UserID = request.UserID,
        payment_date = request.payment_date,  // Use the provided payment date
        price = request.price,               // Use the provided price
        payment_type = request.payment_type  // Use the provided payment type
    };

#pragma warning disable CS8602 // Dereference of a possibly null reference.
            _entityFramWork.Payment.Add(payment);
#pragma warning restore CS8602 // Dereference of a possibly null reference.
            await _entityFramWork.SaveChangesAsync();

    return new OkObjectResult(new { Message = "Rental added and payment processed successfully!", RentalId = rental.rentid });
}

public async Task<IActionResult> CheckRentalConflict(RentRequest request)
{
    if (request == null || request.carid <= 0 ||
        request.start_rent == DateTime.MinValue || request.end_rent == DateTime.MinValue)
    {
        return new BadRequestObjectResult("Invalid rental details provided.");
    }

    if (request.start_rent < DateTime.UtcNow)
    {
        return new BadRequestObjectResult("The start date cannot be in the past.");
    }

#pragma warning disable CS8602 // Dereference of a possibly null reference.
            var car = await _entityFramWork.CarsForRent.FindAsync(request.carid);
#pragma warning restore CS8602 // Dereference of a possibly null reference.
            if (car == null)
    {
        return new NotFoundObjectResult("Car not found.");
    }

#pragma warning disable CS8604 // Possible null reference argument.
            var conflictingRentals = await _entityFramWork.RentRequest
        .Where(r => r.carid == request.carid &&
                    ((request.start_rent >= r.start_rent && request.start_rent <= r.end_rent) ||
                     (request.end_rent >= r.start_rent && request.end_rent <= r.end_rent) ||
                     (request.start_rent <= r.start_rent && request.end_rent >= r.end_rent)))
        .ToListAsync();
#pragma warning restore CS8604 // Possible null reference argument.

            if (conflictingRentals.Any())
    {
        return new OkObjectResult(new
        {
            IsAvailable = false,
            Conflicts = conflictingRentals.Select(r => new
            {
                r.start_rent,
                r.end_rent
            })
        });
    }

    var rentalDuration = request.end_rent - request.start_rent;
    if (rentalDuration.TotalDays <= 0)
    {
        return new BadRequestObjectResult("The rental period must be longer than 0 days.");
    }

    var totalPrice = request.price * rentalDuration.Days;

    var response = new
    {
        IsAvailable = true,
        TotalPrice = totalPrice,
        TotalDays = rentalDuration.Days
    };

    // Log the response for debugging purposes

    return new OkObjectResult(response);
}


   public async Task<bool> EditCarForRentAsync(int carId, CarBriefForRent request)
        {
            // Retrieve the car to be updated
#pragma warning disable CS8602 // Dereference of a possibly null reference.
            var car = await _entityFramWork.CarsForRent.FindAsync(carId);
#pragma warning restore CS8602 // Dereference of a possibly null reference.
            if (car == null)
            {
                return false;  // Car not found
            }

            // Update car properties with the new values from the request
            car.carname = request.carname ?? car.carname;
            car.model = request.model ?? car.model;
            car.price = request.price > 0 ? request.price : car.price;
            car.color = request.color ?? car.color;
            car.body = request.body ?? car.body;

            // Save the changes to the database
            try
            {
#pragma warning disable CS8602 // Dereference of a possibly null reference.
                _entityFramWork.CarsForRent.Update(car);
#pragma warning restore CS8602 // Dereference of a possibly null reference.
                await _entityFramWork.SaveChangesAsync();
                return true;  // Successfully updated
            }
            catch
            {
                return false;  // If any error occurs during the save
            }
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

public List<RentRequest> GetRental()
{
#pragma warning disable CS8604 // Possible null reference argument.
            return _entityFramWork.RentRequest.Select(x => new RentRequest()
    {
        carid = x.carid,
        start_rent = x.start_rent,
        end_rent = x.end_rent,
        price = x.price,
        UserID = x.UserID,
        rentid = x.rentid
    }).ToList();
#pragma warning restore CS8604 // Possible null reference argument.
        }

    
    }
}