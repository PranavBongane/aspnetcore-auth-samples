using JwtAuthDotNet9.Entities;
using JwtAuthDotNet9.Models;

namespace JwtAuthDotNet9.Services
{
    public interface IAuthService
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
        Task<TokenResponseDTO?> LogInAsync(UserDTO request);

        /// <summary>
        /// Registers a new user in the system.
        /// </summary>
        /// <param name="user">The DTO containing username and password.</param>
        /// <returns>
        /// Returns the created <see cref="User"/> object if registration succeeds;  
        /// otherwise, <c>null</c> if the username already exists.
        /// </returns>
        Task<User?> RegisterAsync(UserDTO request);

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
        Task<TokenResponseDTO?> RefreshTokensAsync(RefreshTokenRequestDTO request);
    }
}
