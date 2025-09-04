using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using JwtAuthDotNet9.Entities;
using JwtAuthDotNet9.Models;
using JwtAuthDotNet9.Services;
using JwtAuthDotNet9.Services.implementations;
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
            var result = await authService.Register(request);

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
        public async Task<ActionResult<string>> Login(UserDTO request)
        {
            var result = await authService.LogIn(request);

            if (result != null)
                return result;

            return BadRequest("Invalid Log-in attempt.");
        }

    }
}
