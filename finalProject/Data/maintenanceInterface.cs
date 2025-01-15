using FINALPROJECT.model;
using FINALPROJECT.DTO;
using Microsoft.AspNetCore.Mvc;
namespace FINALPROJECT.Data

{
        public interface MaintenanceInterFace
        {
                public bool SaveChanges();
                public bool RemoveEntity<T>(T entityToAdd);
                public bool AddEntity<T>(T entityToAdd);
                public List<Maintenance> GetAllMaintenance();
                public Maintenance GetMaintenanceById(int id);
                public List<object> GetAllMaintenanceWithUserName();

                public bool UpdateMaintenanceByUser(MaintenanceDto maintenanceDto);
                public bool DeleteMaintenance(int id);
                public AddMaintenanceDto AddMaintenance(AddMaintenanceDto maintenanceDto);
                public int? GetMaintenanceIdByUserId(int userId);
                public object CheckMaintenanceAvailability(CheckMaintenanceDto request);
                public UpdateMaintenanceDto UpdateMaintenance(int maintenanceId, UpdateMaintenanceDto maintenanceDto);



        }
}