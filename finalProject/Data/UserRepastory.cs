using FINALPROJECT.model;
using FINALPROJECT.DTO;

namespace FINALPROJECT.Data

{
    public class UserRepastory : interfaceRepastory
    {
        private DataCountextEf _entityFramWork;


        public UserRepastory(IConfiguration confg)
        {
            _entityFramWork = new DataCountextEf(confg);
        }
        public bool SaveChanges()
        {
            return _entityFramWork.SaveChanges() > 0;
        }
        public bool RemoveEntity<T>(T entityToAdd)
        {
            if (entityToAdd != null)
            {
                _entityFramWork.Remove(entityToAdd);
                return true;
            }
            return false;
        }
        public IEnumerable<Users> GetUsers()
    {
#pragma warning disable CS8604 // Possible null reference argument.
        IEnumerable<Users> user = _entityFramWork.User.ToList<Users>();
#pragma warning restore CS8604 // Possible null reference argument.
        return user;
    }
     public Users GetSingleUsers(int UserId)
    {
#pragma warning disable CS8604 // Possible null reference argument.
        Users? user = _entityFramWork.User.Where(u => u.UserID == UserId).FirstOrDefault<Users>();
#pragma warning restore CS8604 // Possible null reference argument.
        if (user != null)
        {

            return user;
        }
        throw new Exception("falid to get user");

        // return responesArray;
    }

        public List<CarBrief> GetCars()
        {
            throw new NotImplementedException();
        }

        public bool AddCar<T>(T entityToAdd)
        {
            throw new NotImplementedException();
        }
    }
}