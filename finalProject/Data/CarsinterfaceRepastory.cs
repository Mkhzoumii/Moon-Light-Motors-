using FINALPROJECT.model;
using FINALPROJECT.DTO;
using Microsoft.AspNetCore.Mvc;
namespace FINALPROJECT.Data

{
  public interface CarsinterfaceRepastory
  {
    public bool SaveChanges();
    public bool RemoveEntity<T>(T entityToAdd);
    public List<CarBrief> GetAvailableCars();
    public void AddCar(Cars car);
    public CarBrief GetCarById(int carId);
    public bool DeleteCar(int carId);
    public Task<bool> EditSaleCar(int carId, CarBrief request);
    public void AddCarSaleImage(CarForSaleImage carImage);
    public IEnumerable<CarForSaleImage> GetImagesByCarId(int carid);
    public CarForSaleImage GetImageById(int imageId);

    public bool DeleteCarImages(int carId);
    public Task<bool> HasActiveOrFutureRentals(int carId);
    public List<CarBrief> GetAllCars();


    public Task<IActionResult> AddSale([FromBody] PaymentfrosaleDto request);
  }
}