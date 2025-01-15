using FINALPROJECT.model;
using FINALPROJECT.DTO;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FINALPROJECT.Data
{
    public class MaintenanceRepository : MaintenanceInterFace
    {
        private DataCountextEf _entityFramWork;
        private DataCountextDapper _dapper;

        public MaintenanceRepository(IConfiguration confg)
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
        public bool AddEntity<T>(T entityToAdd)
        {
            if (entityToAdd != null)
            {
                _entityFramWork.Add(entityToAdd);
                return true;
            }
            return false;
        }
        public List<Maintenance> GetAllMaintenance()
        {
#pragma warning disable CS8604 // Possible null reference argument.
            return _entityFramWork.Maintenance.ToList();
#pragma warning restore CS8604 // Possible null reference argument.
        }
        public List<object> GetAllMaintenanceWithUserName()
{
#pragma warning disable CS8604 // Possible null reference argument.
            var result = (from maintenance in _entityFramWork.Maintenance
                  join user in _entityFramWork.User // افترض أن جدول المستخدمين اسمه UserInfo
                  on maintenance.UserID equals user.UserID
                  select new
                  {
                      maintenance.maintenance_id,
                      maintenance.maintenance_type,
                      maintenance.UserID,
                      maintenance.start_date,
                      maintenance.end_date,
                      maintenance.price,
                      user.FirstName // افترض أن اسم المستخدم موجود في عمود userName
                  }).ToList();
#pragma warning restore CS8604 // Possible null reference argument.

            return result.Cast<object>().ToList(); // تحويل النتيجة إلى List<object>
}

        public Maintenance GetMaintenanceById(int id)
        {
#pragma warning disable CS8604 // Possible null reference argument.
#pragma warning disable CS8603 // Possible null reference return.
            return _entityFramWork.Maintenance.FirstOrDefault(p => p.maintenance_id == id);
#pragma warning restore CS8603 // Possible null reference return.
#pragma warning restore CS8604 // Possible null reference argument.
        }
     public bool UpdateMaintenanceByUser(MaintenanceDto maintenanceDto)
{
#pragma warning disable CS8604 // Possible null reference argument.
            var existingMaintenance = _entityFramWork.Maintenance
        .FirstOrDefault(m => m.maintenance_id == maintenanceDto.maintenance_id && m.UserID == maintenanceDto.UserID);
#pragma warning restore CS8604 // Possible null reference argument.

            if (existingMaintenance == null)
    {
        return false; // Maintenance record not found or does not belong to the user
    }

    // Update fields
    existingMaintenance.maintenance_type = maintenanceDto.maintenance_type;
    existingMaintenance.start_date = maintenanceDto.start_date;

#pragma warning disable CS8602 // Dereference of a possibly null reference.
            _entityFramWork.Maintenance.Update(existingMaintenance);
#pragma warning restore CS8602 // Dereference of a possibly null reference.
            _entityFramWork.SaveChanges();
    return true;
}

        public bool DeleteMaintenance(int id)
        {
#pragma warning disable CS8604 // Possible null reference argument.
            var maintenance = _entityFramWork.Maintenance.FirstOrDefault(p => p.maintenance_id == id);
#pragma warning restore CS8604 // Possible null reference argument.
            if (maintenance != null)
            {
                _entityFramWork.Remove(maintenance);
                _entityFramWork.SaveChanges();
                return true;
            }
            return false;
        }
public AddMaintenanceDto AddMaintenance(AddMaintenanceDto maintenanceDto)
{
    try
    {
                // 1. جلب تفاصيل الصيانة
#pragma warning disable CS8604 // Possible null reference argument.
                var maintenanceDetails = _entityFramWork.MaintenanceDetales
            .FirstOrDefault(md => md.maintenance_type == maintenanceDto.maintenance_type);
#pragma warning restore CS8604 // Possible null reference argument.

                if (maintenanceDetails == null)
        {
            throw new InvalidOperationException("Invalid maintenance type.");
        }

        // 2. التحقق من أن start_date ليس في الماضي
        if (maintenanceDto.start_date < DateTime.Now)
        {
            throw new InvalidOperationException("The start date cannot be in the past.");
        }

        // 3. حساب وقت النهاية بإضافة وقت الصيانة إلى start_date
        var endDate = maintenanceDto.start_date.Add(maintenanceDetails.maintenance_time);

                // 4. التحقق من تعارض مع مواعيد الصيانة الأخرى
#pragma warning disable CS8604 // Possible null reference argument.
                var conflictingMaintenance = _entityFramWork.Maintenance
            .Where(m => m.UserID == maintenanceDto.UserID &&
                        m.start_date < endDate &&
                        m.end_date > maintenanceDto.start_date)
            .FirstOrDefault();
#pragma warning restore CS8604 // Possible null reference argument.

                if (conflictingMaintenance != null)
        {
            throw new InvalidOperationException("There is a conflict with another maintenance appointment.");
        }

        // 5. إضافة الحجز إلى قاعدة البيانات
        var maintenance = new Maintenance
        {
            maintenance_type = maintenanceDto.maintenance_type,
            start_date = maintenanceDto.start_date,
            UserID = maintenanceDto.UserID,
            price = maintenanceDetails.maintenance_price,
            end_date = endDate  // إضافة وقت الانتهاء
        };

        _entityFramWork.Maintenance.Add(maintenance);
        _entityFramWork.SaveChanges(); // حفظ التغييرات

        // 6. إرجاع البيانات بعد إضافة الحجز
        maintenanceDto.price = maintenanceDetails.maintenance_price;
        maintenanceDto.end_date = endDate;  // إرجاع وقت الانتهاء مع البيانات

        return maintenanceDto;
    }
    catch (InvalidOperationException ex)
    {
        // التعامل مع الاستثناءات الخاصة بالتحقق من الصحة أو تعارض المواعيد
        throw new InvalidOperationException(ex.Message);
    }
    catch (Exception ex)
    {
        // التعامل مع الأخطاء غير المتوقعة
        throw new Exception("An unexpected error occurred while processing the maintenance request.", ex);
    }
}
public UpdateMaintenanceDto UpdateMaintenance(int maintenanceId, UpdateMaintenanceDto maintenanceDto)
{
    try
    {
        // 1. طباعة النوع المدخل للتحقق
        Console.WriteLine($"Received Maintenance Type: '{maintenanceDto.maintenance_type}'");

                // 2. جلب تفاصيل الصيانة باستخدام النوع المدخل
#pragma warning disable CS8602 // Dereference of a possibly null reference.
#pragma warning disable CS8604 // Possible null reference argument.
             var maintenanceDetails = _entityFramWork.MaintenanceDetales
    .FirstOrDefault(md => md.maintenance_type == maintenanceDto.maintenance_type);

if (maintenanceDetails == null)
{
    throw new InvalidOperationException($"Invalid maintenance type: '{maintenanceDto.maintenance_type}'. Please choose a valid type.");
}


                // 3. جلب الموعد الحالي
#pragma warning disable CS8604 // Possible null reference argument.
                var existingMaintenance = _entityFramWork.Maintenance
            .FirstOrDefault(m => m.maintenance_id == maintenanceId);
#pragma warning restore CS8604 // Possible null reference argument.

                if (existingMaintenance == null)
        {
            throw new InvalidOperationException("Maintenance appointment not found.");
        }

        // 4. التحقق مما إذا كان الموعد قد انتهى
        if (existingMaintenance.end_date < DateTime.UtcNow)
        {
            throw new InvalidOperationException("Cannot update a maintenance appointment that has already ended.");
        }

        // 5. التحقق من أن وقت البداية الجديد ليس في الماضي
        if (maintenanceDto.start_date < DateTime.UtcNow)
        {
            throw new InvalidOperationException("The start date cannot be in the past.");
        }

        // 6. حساب وقت الانتهاء الجديد
        var newEndDate = maintenanceDto.start_date.Add(maintenanceDetails.maintenance_time);

        // 7. التحقق من تعارض الوقت الجديد مع المواعيد الأخرى
        var conflictingMaintenance = _entityFramWork.Maintenance
            .Where(m => m.maintenance_id != maintenanceId &&
                        m.start_date < newEndDate &&
                        m.end_date > maintenanceDto.start_date)
            .FirstOrDefault();

        if (conflictingMaintenance != null)
        {
            throw new InvalidOperationException("There is a conflict with another maintenance appointment.");
        }

        // 8. تحديث بيانات الموعد
        existingMaintenance.maintenance_type = maintenanceDto.maintenance_type;
        existingMaintenance.start_date = maintenanceDto.start_date;
        existingMaintenance.end_date = newEndDate;
        existingMaintenance.price = maintenanceDetails.maintenance_price;

        _entityFramWork.SaveChanges(); // حفظ التغييرات

        // 9. تحديث البيانات المُرجعة
        maintenanceDto.end_date = newEndDate;
        return maintenanceDto;
    }
    catch (InvalidOperationException ex)
    {
        // التعامل مع الاستثناءات الخاصة بالتحقق من الصحة أو تعارض المواعيد
        throw new InvalidOperationException(ex.Message);
    }
    catch (Exception ex)
    {
        // التعامل مع الأخطاء غير المتوقعة
        throw new Exception("An unexpected error occurred while updating the maintenance request.", ex);
    }
}



        private IActionResult StatusCode(int v, object value)
        {
            throw new NotImplementedException();
        }

        private IActionResult BadRequest(object value)
        {
            throw new NotImplementedException();
        }

        public object CheckMaintenanceAvailability(CheckMaintenanceDto request)
    {
        // 1. تحقق من أن التاريخ المطلوب ليس في الماضي
        if (request.start_date < DateTime.Now)
        {
            throw new InvalidOperationException("The start date cannot be in the past.");
        }

            // 2. جلب التفاصيل الخاصة بنوع الصيانة من جدول MaintenanceDetails
#pragma warning disable CS8604 // Possible null reference argument.
            var maintenanceDetails = _entityFramWork.MaintenanceDetales
            .Where(md => md.maintenance_type == request.maintenance_type)
            .Select(md => new { md.maintenance_time, md.maintenance_price })
            .FirstOrDefault();
#pragma warning restore CS8604 // Possible null reference argument.

            if (maintenanceDetails == null)
        {
            throw new InvalidOperationException("Invalid maintenance type.");
        }

        // 3. حساب وقت النهاية بناءً على مدة الصيانة
        var endTime = request.start_date.Add(maintenanceDetails.maintenance_time);

            // 4. التحقق من وجود تعارض مع مواعيد الصيانة الأخرى
#pragma warning disable CS8604 // Possible null reference argument.
            var conflictingMaintenance = _entityFramWork.Maintenance
            .Where(m => m.start_date < endTime && m.end_date > request.start_date)
            .OrderByDescending(m => m.end_date)
            .FirstOrDefault();
#pragma warning restore CS8604 // Possible null reference argument.

            if (conflictingMaintenance != null)
        {
            return new
            {
                message = "There is a conflict with another maintenance appointment.",
                last_end_time = conflictingMaintenance.end_date
            };
        }

        // 5. إذا لم يكن هناك تعارض، إرجاع البيانات المطلوبة
        return new
        {
            message = "The requested time is available.",
            maintenance_time = maintenanceDetails.maintenance_time,
            maintenance_price = maintenanceDetails.maintenance_price
        };
    }


        private ActionResult Ok(object value)
        {
            throw new NotImplementedException();
        }

        private ActionResult Conflict(object value)
        {
            throw new NotImplementedException();
        }

        private ActionResult NotFound(string v)
        {
            throw new NotImplementedException();
        }

        private ActionResult BadRequest(string v)
        {
            throw new NotImplementedException();
        }

        public int? GetMaintenanceIdByUserId(int userId)
{
#pragma warning disable CS8604 // Possible null reference argument.
            var maintenance = _entityFramWork.Maintenance
        .FirstOrDefault(m => m.UserID == userId);  // Finds the first maintenance record for the user
#pragma warning restore CS8604 // Possible null reference argument.

            return maintenance?.maintenance_id;  // Return maintenance_id or null if no record found
}

        }}