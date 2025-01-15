using AutoMapper;
using FINALPROJECT.Data;
using FINALPROJECT.DTO;
using FINALPROJECT.model;
using Microsoft.AspNetCore.Mvc;
namespace FINALPROJECT.Controllers;
[ApiController]
[Route("[controller]")]
public class UsersController:Controller
{
     private DataCountextEf _entityFramWork;
    interfaceRepastory _userRepasory;
        private DataCountextDapper _dapper;

    Mapper mapper;
    public UsersController(IConfiguration confg, interfaceRepastory userRepasory)
    {
        _dapper=new DataCountextDapper(confg);
        _entityFramWork = new DataCountextEf(confg);
        mapper = new Mapper(new MapperConfiguration(cfg =>
        {
            cfg.CreateMap<UsersToAddDto, Users>();
        }));
        _userRepasory = userRepasory;
    }
    [HttpGet("GetUsers")]
    public IEnumerable<Users> GetUsers()
    {
#pragma warning disable CS8604 // Possible null reference argument.
        IEnumerable<Users> users = _userRepasory.GetUsers();
#pragma warning restore CS8604 // Possible null reference argument.
        return users;
    } 
   [HttpGet("GetSingleUsers")]
public IActionResult GetSingleUsers()
{
    // Get the Authorization header from the request
    string token = Request.Headers["Authorization"].ToString().Replace("Bearer ", "");

    if (string.IsNullOrEmpty(token))
    {
        return Unauthorized("Token is required.");
    }

    // Secret key used to validate the JWT token
    string secretKey = "nm,dnajkncljsdncafdafdfsdfdfjnbdscjdbdnmcbnmdsbcmdsbcmndsbcdsncnmcmbcdsbc,c,smncsmdnc,sncdmsdcs,mdncsdscmdnc,manc,mnac,dcjndsnwnncwc!@"; 

    // Extract user details from the token (including UserId and is_admin)
    var (userId, isAdmin,email,firstName) = TokenHelper.GetUserDetailsFromToken(token, secretKey);

    if (string.IsNullOrEmpty(userId))
    {
        return Unauthorized("Invalid token or user ID not found.");
    }

    // Use a parameterized query to avoid SQL injection risks
    string userQuery = @"
        SELECT [UserID], [is_admin] 
        FROM FinalProject.UserInfo 
        WHERE UserID = @UserID";

    var user = _dapper.loadDataSingle<Users>(userQuery, new { UserID = userId });

    if (user == null)
    {
        return NotFound("User not found.");
    }

    // Return the user data, including userId and is_admin
    return Ok(new
    {
        userId = user.UserID,
        is_admin = isAdmin // Return is_admin value from the token
    });
}


    [HttpPut("EditUsers")]
    public IActionResult EditUsers(Users User)
    {

#pragma warning disable CS8604 // Possible null reference argument.
        Users? userDb = _userRepasory.GetSingleUsers(User.UserID);
#pragma warning restore CS8604 // Possible null reference argument.
        if (userDb != null)
        {
            userDb.FirstName = User.FirstName;
            userDb.LastName = User.LastName;
            userDb.Email = User.Email;
        }
        if (_userRepasory.SaveChanges())
        {
            return Ok();
        }
        throw new Exception("falid to edit user");
        throw new Exception("falid to get user");

    }
   [HttpDelete("deactivateUser/{UserId}")]
public IActionResult DeactivateUser(int UserId)
{
    try
    {
            // البحث عن المستخدم
#pragma warning disable CS8604 // Possible null reference argument.
            var userDb = _entityFramWork.User.SingleOrDefault(u => u.UserID == UserId);
#pragma warning restore CS8604 // Possible null reference argument.

            if (userDb == null)
        {
            return NotFound(new { message = "User not found." });
        }

        // تحديث قيمة is_active إلى false
        _entityFramWork.Entry(userDb).Property(u => u.is_active).CurrentValue = false;

        // حفظ التغييرات
        var result = _entityFramWork.SaveChanges();

        if (result > 0)
        {
            return Ok(new { message = "User deactivated successfully." });
        }
        else
        {
            return StatusCode(500, new { message = "Failed to deactivate user." });
        }
    }
    catch (Exception ex)
    {
        return StatusCode(500, new { message = "An error occurred.", error = ex.Message });
    }
}
[HttpGet("GetSingleUser/{userId}")]
public IActionResult GetSingleUser(int userId)
{
    // جلب المستخدم من المستودع
    var user = _userRepasory.GetSingleUsers(userId);

    // التحقق من وجود المستخدم
    if (user == null)
    {
        return NotFound(new { message = "User not found." });
    }

    // إعادة بيانات المستخدم
    return Ok(user);
}



}
