using System.IdentityModel.Tokens.Jwt;
using System.Runtime.CompilerServices;
using System.Security.Claims;
using System.Security.Cryptography;
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
        /// <param name="request">
        /// The <see cref="UserDTO"/> containing the username and password.
        /// </param>
        /// <returns>
        /// A <see cref="TokenResponseDTO"/> containing the JWT and refresh token if authentication is successful;  
        /// otherwise, <c>null</c>.
        /// </returns>
        public async Task<TokenResponseDTO?> LogInAsync(UserDTO request)
        {
            try
            {
                // Attempt to retrieve the user from the database by username
                User? user = await context.Users.FirstOrDefaultAsync(u => u.UserName == request.UserName);
                if (user == null)
                {
                    return null; // User does not exist
                }

                // Verify the provided password against the stored hashed password
                var passwordVerification = new PasswordHasher<User>()
                    .VerifyHashedPassword(user, user.PasswordHash, request.Password);

                if (passwordVerification == PasswordVerificationResult.Failed)
                {
                    return null; // Password mismatch
                }

               return await CreateAuthTokensAsync(user);
            }
            catch (Exception)
            {
                // Preserve original stack trace when rethrowing
                throw;
            }
        }

        /// <summary>
        /// Refreshes authentication tokens (JWT + refresh token) for a user,
        /// based on a valid refresh token request.
        /// </summary>
        /// <param name="request">
        /// The <see cref="RefreshTokenRequestDTO"/> containing the user ID and refresh token.
        /// </param>
        /// <returns>
        /// A <see cref="TokenResponseDTO"/> containing new authentication tokens if validation succeeds;  
        /// otherwise, <c>null</c>.
        /// </returns>
        public async Task<TokenResponseDTO?> RefreshTokensAsync(RefreshTokenRequestDTO request)
        {
            // Validate that the refresh token is valid for the specified user
            var user = await ValidateRefreshTokenAsync(request.UserId, request.RefreshToken);

            if (user is null)
            {
                // Either user not found or refresh token invalid/expired
                return null;
            }

            // Generate new JWT and refresh token for the authenticated user
            return await CreateAuthTokensAsync(user);
        }

        /// <summary>
        /// Registers a new user by saving their username and hashed password to the database.
        /// </summary>
        /// <param name="request">The DTO containing user registration details.</param>
        /// <returns>
        /// Returns the created <see cref="User"/> object if successful;  
        /// otherwise, returns null if the username already exists.
        /// </returns>
        public async Task<User?> RegisterAsync(UserDTO request)
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
        private string CreateAuthTokenAsync(User user)
        {
            // Define claims for the JWT (identity info stored in the token)
            var claims = new List<Claim>()
            {
                new Claim(ClaimTypes.Name, user.UserName),
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Role, user.Role.ToString())
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

        /// <summary>
        /// Generates a new secure refresh token.
        /// </summary>
        /// <returns>
        /// A base64-encoded cryptographically secure refresh token string.
        /// </returns>
        private async Task<string> GenerateRefreshTokenAsync()
        {
            // Create a 256-bit (32-byte) random number buffer
            var randomnumber = new byte[32];

            // Use a cryptographic random number generator to fill the buffer
            using var ran = RandomNumberGenerator.Create();
            ran.GetBytes(randomnumber);

            // Convert the random number to a Base64 string to use as the token
            return Convert.ToBase64String(randomnumber);
        }

        /// <summary>
        /// Generates a new refresh token, assigns it to the given user,
        /// sets its expiry time, and saves the changes in the database.
        /// </summary>
        /// <param name="user">
        /// The user object to which the refresh token will be assigned.
        /// </param>
        /// <returns>
        /// The newly generated refresh token as a string.
        /// </returns>
        private async Task<string> GenerateAndSaveRefreshTokenAsync(User user)
        {
            // Generate a secure refresh token
            var refreshtoken = await GenerateRefreshTokenAsync();

            // Assign the refresh token to the user
            user.RefreshToken = refreshtoken;

            // Set the token expiry time (e.g., 3 minutes from now)
            // In production, you might want this to be longer (e.g., days/weeks)
            user.RefreshTokenExpiary = DateTime.UtcNow.AddMinutes(3);

            // Save the updated user entity (with new refresh token) to the database
            await context.SaveChangesAsync();

            // Return the refresh token so it can be sent back to the client
            return refreshtoken;
        }

        /// <summary>
        /// Validates whether the provided refresh token is valid for the specified user.
        /// </summary>
        /// <param name="userId">
        /// The unique identifier of the user whose refresh token is being validated.
        /// </param>
        /// <param name="refreshToken">
        /// The refresh token string to validate against the stored token.
        /// </param>
        /// <returns>
        /// The <see cref="User"/> entity if the refresh token is valid;  
        /// otherwise, <c>null</c>.
        /// </returns>
        private async Task<User?> ValidateRefreshTokenAsync(Guid userId, string refreshToken)
        {
            try
            {
                // Look up the user by their unique ID
                var user = await context.Users.FindAsync(userId);

                // Validate:
                // 1. User exists
                // 2. Stored refresh token matches the provided token
                // 3. Refresh token has not expired
                if (user is null
                    || user.RefreshToken != refreshToken
                    || user.RefreshTokenExpiary <= DateTime.UtcNow)
                {
                    return null;
                }

                // Token is valid → return the user entity
                return user;
            }
            catch (Exception)
            {
                // Preserve original stack trace when rethrowing
                throw;
            }
        }

        /// <summary>
        /// Creates a pair of authentication tokens (JWT and refresh token) for the specified user.
        /// </summary>
        /// <param name="user">
        /// The <see cref="User"/> entity for which the tokens will be generated.
        /// </param>
        /// <returns>
        /// A <see cref="TokenResponseDTO"/> containing both the JWT token and the refresh token.
        /// </returns>
        private async Task<TokenResponseDTO> CreateAuthTokensAsync(User user)
        {
            // Authentication successful → generate JWT and refresh token
            TokenResponseDTO response = new TokenResponseDTO()
            {
                //UserId for further Operations
                UserId = user.Id,

                // JWT token used for short-lived authorization in API requests
                AccessToken = CreateAuthTokenAsync(user),

                // Refresh token used for obtaining new JWTs when the old one expires
                RefreshToken = await GenerateAndSaveRefreshTokenAsync(user)
            };

            return response;
        }
    }
}
