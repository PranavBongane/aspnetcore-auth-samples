
using ClientID_SecretAuth.Api.Data;
using ClientID_SecretAuth.Api.Helpers;
using ClientID_SecretAuth.Api.Models;
using Microsoft.AspNetCore.Mvc;
using System.Security.Cryptography;

namespace ClientID_SecretAuth.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly AppDbContext _db;
    public AuthController(AppDbContext db) => _db = db;

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.UserName))
            return BadRequest("Username is required");

        var clientId = Guid.NewGuid().ToString("N");
        var rawSecret = Convert.ToBase64String(RandomNumberGenerator.GetBytes(32));

        var (hash, salt) = SecretHasher.HashSecret(rawSecret);

        var apiClient = new ApiClient
        {
            ClientId = clientId,
            SecretHash = hash,
            Salt = salt,
            Role = request.Role ?? "User"
        };

        _db.ApiClients.Add(apiClient);
        await _db.SaveChangesAsync();

        return Ok(new
        {
            apiClient.ClientId,
            Secret = rawSecret,
            apiClient.Role,
            Note = "⚠️ Save your keys now. The secret will only appear once."
        });
    }
}

public class RegisterRequest
{
    public string UserName { get; set; } = string.Empty;
    public string? Role { get; set; }
}
