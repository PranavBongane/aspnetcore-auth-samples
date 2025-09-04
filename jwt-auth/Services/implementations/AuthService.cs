using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Azure.Core;
using JwtAuthDotNet9.Data;
using JwtAuthDotNet9.Entities;
using JwtAuthDotNet9.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

namespace JwtAuthDotNet9.Services.implementations
{
    public class AuthService(IConfiguration configuration, UserDbContext context) : IAuthService
    {
        /// <summary>
        /// Authenticates a user based on the provided credentials.
        /// </summary>
        /// <param name="request">The DTO containing username and password.</param>
        /// <returns>
        /// Returns a JWT token string if authentication is successful;  
        /// otherwise, returns null.
        /// </returns>
        public async Task<string?> LogIn(UserDTO request)
        {
            try
            {
                // Check if the user exists in the database by username
                User? user = await context.Users.FirstOrDefaultAsync(u => u.UserName == request.UserName);
                if (user == null)
                {
                    return null; // User not found
                }

                // Verify the provided password against the stored hashed password
                if (new PasswordHasher<User>().VerifyHashedPassword(user, user.PasswordHash, request.Password) == PasswordVerificationResult.Failed)
                {
                    return null; // Invalid password
                }

                // Create JWT token for the authenticated user
                string token = CreateToken(user);
                return token;
            }
            catch (Exception ex)
            {
                // Rethrow the exception (avoid 'throw ex;' as it loses stack trace)
                throw ex ?? new NotImplementedException();
            }
        }

        /// <summary>
        /// Registers a new user by saving their username and hashed password to the database.
        /// </summary>
        /// <param name="request">The DTO containing user registration details.</param>
        /// <returns>
        /// Returns the created <see cref="User"/> object if successful;  
        /// otherwise, returns null if the username already exists.
        /// </returns>
        public async Task<User?> Register(UserDTO request)
        {
            try
            {
                // Check if the username already exists (case-insensitive)
                if (context.Users.Any(u => u.UserName.ToLower() == request.UserName.ToLower()))
                {
                    return null; // Username already taken
                }

                var user = new User();

                // Hash the password before saving to the database
                var hashedPassword = new PasswordHasher<User>().HashPassword(user, request.Password);

                user.UserName = request.UserName;
                user.PasswordHash = hashedPassword;

                // Save the new user to the database
                context.Users.Add(user);
                await context.SaveChangesAsync();

                return user;
            }
            catch (Exception ex)
            {
                throw ex ?? new NotImplementedException();
            }
        }

        /// <summary>
        /// Generates a JWT token for the given user.
        /// </summary>
        /// <param name="user">The authenticated user object.</param>
        /// <returns>Returns a signed JWT token string.</returns>
        private string CreateToken(User user)
        {
            // Define claims for the JWT (identity info stored in the token)
            var claims = new List<Claim>()
            {
                new Claim(ClaimTypes.Name, user.UserName),
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString())
            };

            // Get secret key from configuration
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration.GetValue<string>("appsettings:token")!));

            // Set signing credentials (HMAC SHA512)
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha512);

            // Create the token descriptor
            var tokenDescriptor = new JwtSecurityToken(
                issuer: configuration.GetValue<string>("appsettings:issuer"),
                audience: configuration.GetValue<string>("appsettings:audience"),
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(configuration.GetValue<double>("appsettings:SessionTimeOut")), // Token valid for 15 minutes
                signingCredentials: creds
            );

            // Generate the JWT token string
            return new JwtSecurityTokenHandler().WriteToken(tokenDescriptor);
        }

    }
}
