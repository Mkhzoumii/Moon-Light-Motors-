using FINALPROJECT.model;
using FINALPROJECT.DTO;
using Microsoft.AspNetCore.Mvc;
namespace FINALPROJECT.Data

{
  public interface RentCarInterFace
  {
    public void AddCarForRentImage(CarForRentImages carImage);

    public bool SaveChanges();
    public bool RemoveEntity<T>(T entityToAdd);
    public List<CarBriefForRent> GetCarsForRent();
    public bool AddCar<T>(T entityToAdd);
    public Task<IActionResult> AddRental([FromBody] PaymentDto request);
    public bool DeleteCar(int carId);
    Task<IActionResult> CheckRentalConflict(RentRequest request);
    public CarBriefForRent? GetCarById(int carId);
    public Task<bool> EditCarForRentAsync(int carId, CarBriefForRent request);
    public IEnumerable<CarForRentImages> GetImagesByCarId(int carid);
    public CarForRentImages GetImageById(int imageId);
    public bool DeleteCarImages(int carId);
    public Task<bool> HasActiveOrFutureRentals(int carId);
    public List<RentRequest> GetRental();




  }
}