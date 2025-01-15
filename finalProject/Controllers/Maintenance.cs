using System.Text.RegularExpressions;
using AutoMapper;
using FINALPROJECT.Data;
using FINALPROJECT.DTO;
using FINALPROJECT.model;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
namespace FINALPROJECT.Controllers;
[ApiController]
[Route("[controller]")]
public class MaintenanceController : Controller
{
    MaintenanceInterFace _maintenanceRepasory;
    private DataCountextDapper _dapper;
    Mapper mapper;
    public MaintenanceController(IConfiguration confg, MaintenanceInterFace maintenance)
    {
        _dapper = new DataCountextDapper(confg);
        mapper = new Mapper(new MapperConfiguration(cfg =>
        {
            cfg.CreateMap<Maintenance, MaintenanceDto>();
        }));
        _maintenanceRepasory = maintenance;
    }
    [HttpGet("GetAllMaintenance")]
    public List<Maintenance> GetAllMaintenance()
    {
        return _maintenanceRepasory.GetAllMaintenance();
    }
    [HttpGet("GetAllMaintenanceWithUserName")]
public IActionResult GetAllMaintenanceWithUserName()
{
    var data = _maintenanceRepasory.GetAllMaintenanceWithUserName();
    return Ok(data);
}
[HttpPut("UpdateMaintenance/{id}")]
public IActionResult UpdateMaintenance(int id, [FromBody] UpdateMaintenanceDto maintenanceDto)
{
    try
    {
        // استدعاء الدالة لتحديث بيانات الموعد
        var updatedMaintenance = _maintenanceRepasory.UpdateMaintenance(id, maintenanceDto);
        return Ok(new
        {
            message = "Maintenance appointment updated successfully.",
            data = updatedMaintenance
        });
    }
    catch (InvalidOperationException ex)
    {
        // التعامل مع الأخطاء المتعلقة بالتحقق من الصحة أو تعارض المواعيد
        return BadRequest(new { message = ex.Message });
    }
    catch (Exception ex)
    {
        // التعامل مع الأخطاء غير المتوقعة
        return StatusCode(500, new { message = "An unexpected error occurred.", details = ex.Message });
    }
}

    [HttpGet("GetMaintenanceById/{id}")]
    public IActionResult  GetMaintenanceById(int id)
    {
        var maintenance = _maintenanceRepasory.GetMaintenanceById(id);
        if (maintenance == null)
        {
            return NotFound(); // Return 404 if no maintenance found
        }
        return Ok(maintenance);

    }
   [HttpPut("{userId}")]
public IActionResult UpdateMaintenanceByUser(int userId, [FromBody] MaintenanceDto maintenanceDto)
{
    if (!ModelState.IsValid)
    {
        return BadRequest("Invalid maintenance data.");
    }

    // If `maintenance_id` is not provided, fetch it based on `userId`
    if (maintenanceDto.maintenance_id <= 0)
    {
        var maintenanceId = _maintenanceRepasory.GetMaintenanceIdByUserId(userId);

        if (maintenanceId == null)
        {
            return NotFound("No maintenance record found for the specified user.");
        }

        maintenanceDto.maintenance_id = maintenanceId.Value;  // Automatically set the `maintenance_id` if not provided
    }

    // Ensure the provided `userId` matches the `UserID` in the maintenance record
    if (userId != maintenanceDto.UserID)
    {
        return Unauthorized("You can only update your own maintenance records.");
    }

    var result = _maintenanceRepasory.UpdateMaintenanceByUser(maintenanceDto);

    if (!result)
    {
        return NotFound("Maintenance record not found or you don't have permission to update it.");
    }

    return Ok("Maintenance record updated successfully.");
}
[HttpPost("add-maintenance")]
    public IActionResult AddMaintenance([FromBody] AddMaintenanceDto maintenanceDto)
    {
        try
        {
            // التحقق من البيانات المدخلة
            if (!ModelState.IsValid)
            {
                return BadRequest(new
                {
                    success = false,
                    errorMessage = "Invalid maintenance data.",
                    errors = ModelState.Values
                        .SelectMany(v => v.Errors)
                        .Select(e => e.ErrorMessage)
                });
            }

            // استدعاء الدالة في الـ Repository لإضافة الحجز
            var addedMaintenance = _maintenanceRepasory.AddMaintenance(maintenanceDto);

            // إرجاع استجابة ناجحة مع تفاصيل الحجز
            return Ok(new
            {
                success = true,
                message = "Maintenance record added successfully.",
                maintenance = new
                {
                    addedMaintenance.UserID,
                    addedMaintenance.maintenance_type,
                    addedMaintenance.start_date,
                    addedMaintenance.end_date,  // إرجاع وقت الانتهاء
                    addedMaintenance.price // إرجاع السعر
                }
            });
        }
        catch (InvalidOperationException ex) when (ex.Message.Contains("conflict"))
        {
            // معالجة تعارضات الحجز
            return Conflict(new
            {
                success = false,
                errorMessage = ex.Message
            });
        }
        catch (InvalidOperationException ex)
        {
            // معالجة أخطاء أخرى
            return BadRequest(new
            {
                success = false,
                errorMessage = ex.Message
            });
        }
        catch (Exception ex)
        {
            // معالجة الأخطاء غير المتوقعة
            return StatusCode(500, new
            {
                success = false,
                errorMessage = "An unexpected error occurred.",
                details = ex.Message
            });
        }}

// Helper method to parse conflict details from exception messages
private object? ParseConflictDetails(string message)
{
    try
    {
        // Example logic to parse start_date and end_date from the exception message
        var startMatch = Regex.Match(message, @"Start Time: (\d{4}-\d{2}-\d{2} \d{2}:\d{2}:\d{2})");
        var endMatch = Regex.Match(message, @"End Time: (\d{4}-\d{2}-\d{2} \d{2}:\d{2}:\d{2})");

        if (startMatch.Success && endMatch.Success)
        {
            return new
            {
                start_date = DateTime.Parse(startMatch.Groups[1].Value),
                end_date = DateTime.Parse(endMatch.Groups[1].Value)
            };
        }
    }
    catch
    {
        // Log or handle parsing errors if needed
    }

    return null; // Return null if unable to parse
}

// This is a helper method to extract conflicting date details from the exception message
private (DateTime start_date, DateTime end_date)? GetConflictDetails(string errorMessage)
{
    var startDateMatch = Regex.Match(errorMessage, @"Start Time: (\S+)");
    var endDateMatch = Regex.Match(errorMessage, @"End Time: (\S+)");

    if (startDateMatch.Success && endDateMatch.Success)
    {
        if (DateTime.TryParse(startDateMatch.Groups[1].Value, out var startDate) &&
            DateTime.TryParse(endDateMatch.Groups[1].Value, out var endDate))
        {
            return (startDate, endDate);
        }
    }

    return null;  // Return null if no valid dates are found
}


[HttpDelete("DeleteMaintenance/{id}")]
public IActionResult DeleteMaintenance(int id)
{
    // Call the repository method to delete the maintenance record by id
    bool isDeleted = _maintenanceRepasory.DeleteMaintenance(id);

    if (isDeleted)
    {
        return Ok(new { message = "Maintenance record deleted successfully." });
    }
    else
    {
        return NotFound(new { message = "Maintenance record not found." });
    }
}
[HttpPost("CheckAvailability")]
  public IActionResult CheckAvailability([FromBody] CheckMaintenanceDto request)
    {
        try
        {
            var result = _maintenanceRepasory.CheckMaintenanceAvailability(request);
            return Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            // تسجيل الخطأ باستخدام Console.WriteLine (اختياري)
            Console.WriteLine($"Business logic error: {ex.Message}");
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            // تسجيل الخطأ باستخدام Console.WriteLine (اختياري)
            Console.WriteLine($"Unexpected error: {ex.Message}");
            return StatusCode(500, new { message = "An error occurred.", details = ex.Message });
        }
    }
}