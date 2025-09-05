using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using JwtAuthDotNet9.Entities;
using JwtAuthDotNet9.Models;
using JwtAuthDotNet9.Services;
using JwtAuthDotNet9.Services.implementations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Primitives;
using Microsoft.IdentityModel.Tokens;

namespace JwtAuthDotNet9.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController(IAuthService authService) : ControllerBase
    {
        /// <summary>
        /// Registers a new user.
        /// </summary>
        /// <param name="request">The user details (username, password, etc.).</param>
        /// <returns>
        /// Returns <see cref="OkObjectResult"/> with a success message if registration is successful,  
        /// or <see cref="BadRequestObjectResult"/> if the user already exists.
        /// </returns>
        [HttpPost("register")]
        public async Task<ActionResult<User>> Register(UserDTO request)
        {
            var result = await authService.RegisterAsync(request);

            if (result != null)
                return Ok("User registered.....✔️");

            return BadRequest("User already exists.");
        }

        /// <summary>
        /// Authenticates a user and logs them in.
        /// </summary>
        /// <param name="request">The user credentials (username and password).</param>
        /// <returns>
        /// Returns a JWT token string if login is successful,  
        /// or <see cref="BadRequestObjectResult"/> if the login attempt is invalid.
        /// </returns>
        [HttpPost("login")]
        public async Task<ActionResult<TokenResponseDTO?>> Login(UserDTO request)
        {
            var result = await authService.LogInAsync(request);

            if (result != null)
                return result;

            return BadRequest("Invalid Log-in attempt.");
        }
       
        /// <summary>
        /// Refreshes authentication tokens (JWT + refresh token) for a client,  
        /// if the provided refresh token is valid.
        /// </summary>
        /// <param name="request">
        /// The <see cref="RefreshTokenRequestDTO"/> containing the user ID and refresh token.
        /// </param>
        /// <returns>
        /// An <see cref="ActionResult{T}"/> containing a <see cref="TokenResponseDTO"/> with new tokens  
        /// if successful; otherwise, <see cref="UnauthorizedResult"/>.
        /// </returns>
        [HttpPost("refreshtokens")]
        public async Task<ActionResult<TokenResponseDTO>> RefreshAuthTokens(RefreshTokenRequestDTO request)
        {
            // Delegate refresh logic to the authentication service
            var result = await authService.RefreshTokensAsync(request);

            // Validate response (null, invalid token, or missing fields)
            if (result is null || result.RefreshToken is null || result.AccessToken is null)
            {
                return Unauthorized("Invalid refresh token.....🤨");
            }

            // Return new JWT + refresh token
            return Ok(result);
        }

        /// <summary>
        /// Endpoint accessible only to authenticated users with the "Admin" role.
        /// </summary>
        /// <returns>
        /// An <see cref="ActionResult"/> containing a success message if authorized.
        /// </returns>
        [Authorize(Roles = "Admin")]
        [HttpGet("adminsOnly")]
        public ActionResult AuthsOnly()
        {
            return Ok("You are Admin..😊");
        }

        /// <summary>
        /// Endpoint accessible only to authenticated users with the "User" role.
        /// </summary>
        /// <returns>
        /// An <see cref="ActionResult"/> containing a success message if authorized.
        /// </returns>
        [Authorize(Roles = "User")]
        [HttpGet("usersOnly")]
        public ActionResult PandusOnly()
        {
            return Ok("You are User..😊");
        }

    }
}
