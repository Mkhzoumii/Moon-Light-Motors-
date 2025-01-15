using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
namespace FINALPROJECT.Helpers
{
    public class AuthHelper
    {
        private readonly IConfiguration _config;
        public AuthHelper(IConfiguration config)
        {
            _config = config;

        }
        public byte[] GetPasswordHash(string password, byte[] PasswordSalt)
        {
            string PasswordSaltPlussSrting = _config.GetSection("AppSittings:PasseordKey").Value
            + Convert.ToBase64String(PasswordSalt);
            return KeyDerivation.Pbkdf2(
            password: password,
            salt: Encoding.ASCII.GetBytes(PasswordSaltPlussSrting),
            prf: KeyDerivationPrf.HMACSHA256,
            iterationCount: 100000,
            numBytesRequested: 256 / 8);
        }

      public string CreateToken(int userId, bool isAdmin, string email, string firstName)
{
    // Create claims, including user-specific claims
    Claim[] claims = new Claim[]
    {
        new Claim("UserId", userId.ToString()),
        new Claim("is_admin", isAdmin.ToString()), // Add is_admin as a claim
        new Claim("Email", email),                // Add email as a claim
        new Claim("FirstName", firstName)         // Add first name as a claim
    };

    // Generate the key using the token key from configuration
#pragma warning disable CS8604 // Possible null reference argument.
    SymmetricSecurityKey tokenKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(
        _config.GetSection("AppSittings:TokenKey").Value));
#pragma warning restore CS8604 // Possible null reference argument.

    // Define signing credentials
    SigningCredentials credentials = new SigningCredentials(
        tokenKey, SecurityAlgorithms.HmacSha512Signature);

    // Define the token descriptor
    SecurityTokenDescriptor tokenDescriptor = new SecurityTokenDescriptor()
    {
        Subject = new ClaimsIdentity(claims),
        SigningCredentials = credentials,
        Expires = DateTime.Now.AddDays(1) // Set token expiration
    };

    // Create and return the token
    JwtSecurityTokenHandler tokenHandler = new JwtSecurityTokenHandler();
    SecurityToken token = tokenHandler.CreateToken(tokenDescriptor);
    return tokenHandler.WriteToken(token);
}


    }
}