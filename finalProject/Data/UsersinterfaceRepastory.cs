using FINALPROJECT.model;
using FINALPROJECT.DTO;
namespace FINALPROJECT.Data

{
    public interface interfaceRepastory
    {
        public bool SaveChanges();
        public bool RemoveEntity<T>(T entityToAdd);
        public IEnumerable<Users> GetUsers();
        public List<CarBrief> GetCars();
        public bool AddCar<T>(T entityToAdd);
        public Users GetSingleUsers(int UserId);
    }
}