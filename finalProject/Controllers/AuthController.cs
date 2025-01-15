using System.Data;
using System.Security.Cryptography;
using FINALPROJECT.Data;
using FINALPROJECT.DTO;
using FINALPROJECT.Helpers;
using FINALPROJECT.model;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
namespace FINALPROJECT.Controllers
{ 
    [Authorize]
    [ApiController]
    [Route("[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly DataCountextDapper _dapper;
        private readonly IConfiguration _config;
        private readonly AuthHelper _authHelper;
        public AuthController(IConfiguration config)
        {
            _dapper = new DataCountextDapper(config);
            _config = config;
            _authHelper=new AuthHelper(config);
        }
        [AllowAnonymous]
       [HttpPost("Register")]
        public IActionResult Regestir(UserForRegistrationDto userForRegistration)
        {
           
            if (userForRegistration.Password == userForRegistration.PasswordConfirm)
            {
                string sqlCheckUserExists = "SELECT Email FROM FinalProject.UserInfo WHERE Email ='"
                + userForRegistration.Email + "'";
                IEnumerable<string> existsUser = _dapper.loadData<string>(sqlCheckUserExists);
                if (existsUser.Count() == 0)
                {
                    byte[] PasswordSalt = new byte[120 / 8];
                    using (RandomNumberGenerator ran = RandomNumberGenerator.Create())
                    {
                       ran.GetNonZeroBytes(PasswordSalt);
                    }
                    string PasswordSaltPlussSrting = _config.GetSection("AppSittings:PasseordKey").Value
                    + Convert.ToBase64String(PasswordSalt);
                    byte[] PasswordHash =_authHelper.GetPasswordHash(userForRegistration.Password, PasswordSalt);
                    string sqlAddAuth = @"INSERT INTO FinalProject.UserInfo ([FirstName],
                    [LastName],[Email],
                    [PasswordHash],
                    [PasswordSalt]) VALUES('" + userForRegistration.FirstName + @"','" + userForRegistration.LastName + 
                     @"','" + userForRegistration.Email +"',@PasswordHash,@PasswordSalt)";
                    List<SqlParameter> sqlParameters = new List<SqlParameter>();
                    SqlParameter PasswordSaltParameters = new SqlParameter
                    ("@PasswordSalt", SqlDbType.VarBinary);
                    PasswordSaltParameters.Value = PasswordSalt;
                    SqlParameter PasswordHashParameters = new SqlParameter
                  ("@PasswordHash", SqlDbType.VarBinary);
                    PasswordHashParameters.Value = PasswordHash;
                    sqlParameters.Add(PasswordSaltParameters);
                    sqlParameters.Add(PasswordHashParameters);
                    if (_dapper.executSqlParameters(sqlAddAuth, sqlParameters))
                    {
                            return Ok();
                            
                            }
                    }
                    throw new Exception("falied to regester user");
                }
                throw new Exception("user with this Email is exists");
            }
    

        [AllowAnonymous]
   [HttpPost("Login")]
public IActionResult Login(UserForLoginDto userForLoginDto)
{
    // Retrieve password hash, salt, and is_active
    string sqlForHashAndSalt = @"SELECT [PasswordHash], [PasswordSalt], [is_active] 
                                 FROM FinalProject.UserInfo 
                                 WHERE Email = @Email";
    UserForLoginConformationDto userForLoginConformationDto = _dapper.loadDataSingle<UserForLoginConformationDto>(
        sqlForHashAndSalt, 
        new { Email = userForLoginDto.Email });

    if (userForLoginConformationDto == null)
    {
        return StatusCode(401, "Invalid email or password.");
    }

    // Check if the account is active
    if (userForLoginConformationDto.is_active == false)
    {
        return StatusCode(403, "This account has been deactivated.");
    }

    // Verify the password
    byte[] passwordHash = _authHelper.GetPasswordHash(userForLoginDto.Password, userForLoginConformationDto.PasswordSalt);
    for (int index = 0; index < passwordHash.Length; index++)
    {
        if (passwordHash[index] != userForLoginConformationDto.PasswordHash[index])
        {
            return StatusCode(401, "Incorrect password");
        }
    }

    // Retrieve user details
    string userSql = @"SELECT [UserID], [FirstName], [Email], [is_admin] 
                       FROM FinalProject.UserInfo 
                       WHERE Email = @Email";
    var userResult = _dapper.loadDataSingle<UserInfoFromLoginDto>(
        userSql, 
        new { Email = userForLoginDto.Email });

    if (userResult == null)
    {
        return StatusCode(401, "User not found.");
    }

            // Generate token with user information
#pragma warning disable CS8604 // Possible null reference argument.
            string token = _authHelper.CreateToken(userResult.UserID, userResult.is_admin, userResult.Email, userResult.FirstName);
#pragma warning restore CS8604 // Possible null reference argument.

            // Return token and user details
            return Ok(new
    {
        token,
        firstName = userResult.FirstName,
        email = userResult.Email
    });
}

        
[HttpGet("RefreshToken")]
public IActionResult RefreshToken()
{
    // Get the UserID from the token
    string? userId = User.FindFirst("UserID")?.Value;

    // Query to get UserID and is_admin from the database
    string userQuery = @"SELECT [UserID], [is_admin] FROM FinalProject.UserInfo WHERE UserID = @UserID";
    
    var user = _dapper.loadDataSingle<dynamic>(userQuery, new { UserID = userId });

    if (user == null)
    {
        return NotFound("User not found.");
    }

    return Ok(new Dictionary<string, string>
    {
        { "userId", user.UserID.ToString() },
        { "is_admin", user.is_admin.ToString() }
        // You can add the token if needed
        // { "token", _authHelper.CreateToken(user.UserID) }
    });
}



 [HttpPost("UpdatePassword")]
        public IActionResult UpdatePassword(UpdatePasswordDto updatePasswordDto)
        {
            // Get the user ID from the current logged-in user's claim
            string? userId = User.FindFirst("UserID")?.Value;

            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized("User not found.");
            }

            // Step 1: Get the current password hash and salt from the database
            string sqlForHashAndSalt = @"SELECT [PasswordHash], [PasswordSalt] 
                                         FROM FinalProject.UserInfo 
                                         WHERE UserID = @UserID";
            UserForLoginConformationDto userForLoginConformationDto = _dapper.loadDataSingle<UserForLoginConformationDto>(
                sqlForHashAndSalt, new { UserID = userId });

            if (userForLoginConformationDto == null)
            {
                return Unauthorized("User not found.");
            }

            // Step 2: Verify the current password
#pragma warning disable CS8604 // Possible null reference argument.
            byte[] currentPasswordHash = _authHelper.GetPasswordHash(updatePasswordDto.CurrentPassword, userForLoginConformationDto.PasswordSalt);
#pragma warning restore CS8604 // Possible null reference argument.

            if (!currentPasswordHash.SequenceEqual(userForLoginConformationDto.PasswordHash))
            {
                return Unauthorized("Current password is incorrect.");
            }

            // Step 3: Generate new password hash and salt for the new password
            byte[] newPasswordSalt = new byte[120 / 8];
            using (RandomNumberGenerator rng = RandomNumberGenerator.Create())
            {
                rng.GetNonZeroBytes(newPasswordSalt);
            }

#pragma warning disable CS8604 // Possible null reference argument.
            byte[] newPasswordHash = _authHelper.GetPasswordHash(updatePasswordDto.NewPassword, newPasswordSalt);
#pragma warning restore CS8604 // Possible null reference argument.

            // Step 4: Update the password hash and salt in the database
            string sqlUpdatePassword = @"UPDATE FinalProject.UserInfo 
                                         SET PasswordHash = @PasswordHash, PasswordSalt = @PasswordSalt
                                         WHERE UserID = @UserID";

            var sqlParameters = new List<SqlParameter>
            {
                new SqlParameter("@PasswordHash", SqlDbType.VarBinary) { Value = newPasswordHash },
                new SqlParameter("@PasswordSalt", SqlDbType.VarBinary) { Value = newPasswordSalt },
                new SqlParameter("@UserID", SqlDbType.NVarChar) { Value = userId }
            };

            bool isPasswordUpdated = _dapper.executSqlParameters(sqlUpdatePassword, sqlParameters);

            if (isPasswordUpdated)
            {
                return Ok("Password updated successfully.");
            }
            else
            {
                return StatusCode(500, "Failed to update password.");
            }
        }
        [HttpPost("UpdateEmail")]
public IActionResult UpdateEmail(UpdateEmailDto updateEmailDto)
{
    // Get the user ID from the current logged-in user's claim
    int userId = updateEmailDto.UserID;

    if (string.IsNullOrEmpty(userId.ToString()))
    {
        return Unauthorized("User not found.");
    }

    // Step 1: Validate that the user exists in the database
    string sqlCheckUser = @"SELECT COUNT(*) 
                           FROM FinalProject.UserInfo 
                           WHERE UserID = @UserID";

    int userExists = _dapper.ExecuteScalar<int>(sqlCheckUser, new { UserID = userId });

    if (userExists == 0)
    {
        return Unauthorized("User not found.");
    }

    // Step 2: Update the email in the database
    string sqlUpdateEmail = @"UPDATE FinalProject.UserInfo 
                              SET Email = @NewEmail 
                              WHERE UserID = @UserID";

    var sqlParameters = new List<SqlParameter>
    {
        new SqlParameter("@NewEmail", SqlDbType.NVarChar) { Value = updateEmailDto.NewEmail },
        new SqlParameter("@UserID", SqlDbType.NVarChar) { Value = userId }
    };

    bool isEmailUpdated = _dapper.executSqlParameters(sqlUpdateEmail, sqlParameters);

    if (isEmailUpdated)
    {
        return Ok("Email updated successfully.");
    }
    else
    {
        return StatusCode(500, "Failed to update email.");
    }
}
[HttpPost("UpdateFirstName")]
public IActionResult UpdateFirstName(UpdateFirstNameDto updateFirstNameDto)
{
    // Get the user ID from the current logged-in user's claim
    int userId = updateFirstNameDto.UserID;

    if (string.IsNullOrEmpty(userId.ToString()))
    {
        return Unauthorized("User not found.");
    }

    // Step 1: Validate that the user exists in the database
    string sqlCheckUser = @"SELECT COUNT(*) 
                           FROM FinalProject.UserInfo 
                           WHERE UserID = @UserID";

    int userExists = _dapper.ExecuteScalar<int>(sqlCheckUser, new { UserID = userId });

    if (userExists == 0)
    {
        return Unauthorized("User not found.");
    }

    // Step 2: Update the first name in the database
    string sqlUpdateFirstName = @"UPDATE FinalProject.UserInfo 
                                   SET FirstName = @FirstName 
                                   WHERE UserID = @UserID";

    var sqlParameters = new List<SqlParameter>
    {
        new SqlParameter("@FirstName", SqlDbType.NVarChar) { Value = updateFirstNameDto.NewFirstName },
        new SqlParameter("@UserID", SqlDbType.NVarChar) { Value = userId }
    };

    bool isFirstNameUpdated = _dapper.executSqlParameters(sqlUpdateFirstName, sqlParameters);

    if (isFirstNameUpdated)
    {
        return Ok("First name updated successfully.");
    }
    else
    {
        return StatusCode(500, "Failed to update first name.");
    }
}

[HttpPost("UpdateUserDetails")]
public IActionResult UpdateUserDetails(UpdeateUserNameAndEmail updateUserDetailsDto)
{
    // Get the user ID from the current logged-in user's claim
    int userId = updateUserDetailsDto.UserID;

    if (string.IsNullOrEmpty(userId.ToString()))
    {
        return Unauthorized("User not found.");
    }

    // Step 1: Validate that the user exists in the database
    string sqlCheckUser = @"SELECT COUNT(*) 
                           FROM FinalProject.UserInfo 
                           WHERE UserID = @UserID";

    int userExists = _dapper.ExecuteScalar<int>(sqlCheckUser, new { UserID = userId });

    if (userExists == 0)
    {
        return Unauthorized("User not found.");
    }

    // Step 2: Build the SQL update query dynamically based on provided values
    List<string> setClauses = new List<string>();
    List<SqlParameter> sqlParameters = new List<SqlParameter>();

    if (!string.IsNullOrEmpty(updateUserDetailsDto.NewFirstName))
    {
        setClauses.Add("FirstName = @FirstName");
        sqlParameters.Add(new SqlParameter("@FirstName", SqlDbType.NVarChar) { Value = updateUserDetailsDto.NewFirstName });
    }

    if (!string.IsNullOrEmpty(updateUserDetailsDto.NewEmail))
    {
        setClauses.Add("Email = @NewEmail");
        sqlParameters.Add(new SqlParameter("@NewEmail", SqlDbType.NVarChar) { Value = updateUserDetailsDto.NewEmail });
    }

    if (setClauses.Count == 0)
    {
        return BadRequest("No updates provided.");
    }

    string sqlUpdateUser = $@"UPDATE FinalProject.UserInfo 
                              SET {string.Join(", ", setClauses)} 
                              WHERE UserID = @UserID";

    sqlParameters.Add(new SqlParameter("@UserID", SqlDbType.NVarChar) { Value = userId });

    // Execute the update
    bool isUpdated = _dapper.executSqlParameters(sqlUpdateUser, sqlParameters);

    if (isUpdated)
    {
        return Ok("User details updated successfully.");
    }
    else
    {
        return StatusCode(500, "Failed to update user details.");
    }
}}
}