
using ClientID_SecretAuth.Api.Data;
using ClientID_SecretAuth.Api.Helpers;
using Microsoft.AspNetCore.Authentication;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using System.Security.Claims;
using System.Text;
using System.Text.Encodings.Web;

namespace ClientID_SecretAuth.Api.Security;

public class ClientSecretAuthHandler : AuthenticationHandler<AuthenticationSchemeOptions>
{
    private readonly AppDbContext _db;

    public ClientSecretAuthHandler(
        IOptionsMonitor<AuthenticationSchemeOptions> options,
        ILoggerFactory logger,
        UrlEncoder encoder,
        ISystemClock clock,
        AppDbContext db) : base(options, logger, encoder, clock)
    {
        _db = db;
    }

    protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        if (!Request.Headers.ContainsKey("Authorization"))
            return AuthenticateResult.Fail("Missing Authorization Header");

        try
        {
            var authHeader = Request.Headers["Authorization"].ToString();
            if (!authHeader.StartsWith("Basic ", StringComparison.OrdinalIgnoreCase))
                return AuthenticateResult.Fail("Invalid Authorization Header");

            var token = authHeader.Substring("Basic ".Length).Trim();
            var credentialString = Encoding.UTF8.GetString(Convert.FromBase64String(token));
            var parts = credentialString.Split(':');

            if (parts.Length != 2)
                return AuthenticateResult.Fail("Invalid Basic Auth format");

            var clientId = parts[0];
            var secret = parts[1];

            var client = await _db.ApiClients.FirstOrDefaultAsync(c => c.ClientId == clientId);
            if (client == null || !SecretHasher.VerifySecret(secret, client.Salt, client.SecretHash))
                return AuthenticateResult.Fail("Invalid ClientId or Secret");

            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, client.ClientId),
                new Claim(ClaimTypes.Role, client.Role)
            };

            var identity = new ClaimsIdentity(claims, Scheme.Name);
            var principal = new ClaimsPrincipal(identity);
            var ticket = new AuthenticationTicket(principal, Scheme.Name);

            return AuthenticateResult.Success(ticket);
        }
        catch
        {
            return AuthenticateResult.Fail("Invalid Authorization Header");
        }
    }
}
