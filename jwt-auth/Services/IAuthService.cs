using JwtAuthDotNet9.Entities;
using JwtAuthDotNet9.Models;

namespace JwtAuthDotNet9.Services
{
    public interface IAuthService
    {
        /// <summary>
        /// Authenticates a user with the provided credentials.
        /// </summary>
        /// <param name="user">The DTO containing username and password.</param>
        /// <returns>
        /// Returns a JWT token string if login is successful;  
        /// otherwise, <c>null</c> if authentication fails.
        /// </returns>
        Task<string?> LogIn(UserDTO user);

        /// <summary>
        /// Registers a new user in the system.
        /// </summary>
        /// <param name="user">The DTO containing username and password.</param>
        /// <returns>
        /// Returns the created <see cref="User"/> object if registration succeeds;  
        /// otherwise, <c>null</c> if the username already exists.
        /// </returns>
        Task<User?> Register(UserDTO user);
    }
}
