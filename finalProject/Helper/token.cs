using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;

public class TokenHelper
{
   public static (string? UserId, bool? IsAdmin, string? Email, string? FirstName) GetUserDetailsFromToken(string token, string secret)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var key = System.Text.Encoding.ASCII.GetBytes(secret);

        try
        {
            // Token validation parameters
            var validationParameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(key),
                ValidateIssuer = false,
                ValidateAudience = false,
            };

            // Validate the token and extract the claims principal
            var principal = tokenHandler.ValidateToken(token, validationParameters, out SecurityToken validatedToken);

            // Extract the UserId claim
            var userIdClaim = principal.FindFirst("UserId");
            var userId = userIdClaim?.Value;

            // Extract the is_admin claim and safely parse it
            var isAdminClaim = principal.FindFirst("is_admin");
            bool? isAdmin = null;
            if (isAdminClaim != null && bool.TryParse(isAdminClaim.Value, out bool parsedIsAdmin))
            {
                isAdmin = parsedIsAdmin;
            }

            // Extract the Email claim
            var emailClaim = principal.FindFirst("Email");
            var email = emailClaim?.Value;

            // Extract the FirstName claim
            var firstNameClaim = principal.FindFirst("firstName");
            var firstName = firstNameClaim?.Value;

            // Return all extracted details
            return (userId, isAdmin, email, firstName);
        }
        catch (Exception ex)
        {
            // Handle token validation failure
            Console.WriteLine("Token validation failed: " + ex.Message);
            return (null, null, null, null); // Return null values if token is invalid
        }
    }
}

