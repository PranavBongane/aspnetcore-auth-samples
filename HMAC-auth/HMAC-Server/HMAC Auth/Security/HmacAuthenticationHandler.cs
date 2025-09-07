using System;
using System.Security.Cryptography;
using System.Text;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;

namespace ProductApi.Security;

// Simple POCO to hold client credentials + role
public sealed class ClientCredential(string ClientId, string Secret, string Role = "ProductManager")
{
    public string ClientId { get; init; } = ClientId;
    public string Secret { get; init; } = Secret;
    public string Role { get; init; } = Role;
}

public interface IClientSecretStore
{
    Task<ClientCredential?> GetClientAsync(string clientId, CancellationToken ct = default);
}

public sealed class InMemoryClientSecretStore : IClientSecretStore
{
    private readonly IReadOnlyDictionary<string, ClientCredential> _clients;
    public InMemoryClientSecretStore(IEnumerable<ClientCredential> creds)
        => _clients = creds.ToDictionary(c => c.ClientId, c => c);

    public Task<ClientCredential?> GetClientAsync(string clientId, CancellationToken ct = default)
        => Task.FromResult(_clients.TryGetValue(clientId, out var c) ? c : null);
}

/// <summary>
/// Custom HMAC authentication handler. Requires headers:
///   x-client-id, x-date (UTC), x-nonce, x-signature
/// Canonical string: METHOD\nPATH\nQUERY\nDATE\nNONCE\nBODY_SHA256_HEX
/// Signature: base64(HMACSHA256(secret, canonical))
/// </summary>
public sealed class HmacAuthenticationHandler : AuthenticationHandler<AuthenticationSchemeOptions>
{
    private const string SchemeName = "Hmac";
    private const string H_CLIENT_ID = "x-client-id";
    private const string H_DATE = "x-date";
    private const string H_NONCE = "x-nonce";
    private const string H_SIGNATURE = "x-signature";

    private readonly IClientSecretStore _store;
    private readonly IMemoryCache _cache;
    private readonly IConfiguration _cfg;

    public HmacAuthenticationHandler(
     IOptionsMonitor<AuthenticationSchemeOptions> options,
     ILoggerFactory logger,
     UrlEncoder encoder,
     ISystemClock clock,
     IClientSecretStore store,
     IMemoryCache cache,
     IConfiguration cfg)
     : base(options, logger, encoder, clock)
    {
        _store = store;
        _cache = cache;
        _cfg = cfg;
    }

    protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        var req = Request;

        // 1) Required headers
        if (!req.Headers.TryGetValue(H_CLIENT_ID, out var clientIdVals) ||
            !req.Headers.TryGetValue(H_DATE, out var dateVals) ||
            !req.Headers.TryGetValue(H_NONCE, out var nonceVals) ||
            !req.Headers.TryGetValue(H_SIGNATURE, out var sigVals))
        {
            return AuthenticateResult.Fail("Missing HMAC headers");
        }

        var clientId = clientIdVals.ToString();
        var dateRaw = dateVals.ToString();
        var nonce = nonceVals.ToString();
        var signature = sigVals.ToString();

        // 2) Clock skew check
        var skewSeconds = _cfg.GetValue<int?>("Hmac:ClockSkewSeconds") ?? 300;
        if (!DateTimeOffset.TryParse(dateRaw, out var sent) ||
            Math.Abs((Clock.UtcNow - sent).TotalSeconds) > skewSeconds)
        {
            return AuthenticateResult.Fail("x-date outside allowed skew");
        }

        // 3) Replay protection
        var replayWindow = TimeSpan.FromMinutes(_cfg.GetValue<int?>("Hmac:ReplayWindowMinutes") ?? 10);
        var replayKey = $"hmac:nonce:{clientId}:{nonce}";
        if (_cache.TryGetValue(replayKey, out _))
        {
            return AuthenticateResult.Fail("Replay detected");
        }

        // 4) Lookup client
        var client = await _store.GetClientAsync(clientId, Context.RequestAborted);
        if (client is null) return AuthenticateResult.Fail("Unknown clientId");

        // 5) Build canonical and verify signature
        var canonical = await BuildCanonicalAsync(req, dateRaw, nonce, Context.RequestAborted);
        var expected = ComputeHmacBase64(client.Secret, canonical);
        if (!FixedTimeEquals(expected, signature))
        {
            return AuthenticateResult.Fail("Invalid signature");
        }

        // 6) Mark nonce as used
        _cache.Set(replayKey, true, replayWindow);

        // 7) Create principal with role
        var id = new System.Security.Claims.ClaimsIdentity(SchemeName);
        id.AddClaim(new("client_id", client.ClientId));
        id.AddClaim(new(System.Security.Claims.ClaimTypes.Role, client.Role));
        var ticket = new AuthenticationTicket(new System.Security.Claims.ClaimsPrincipal(id), SchemeName);
        return AuthenticateResult.Success(ticket);
    }

    private static async Task<string> BuildCanonicalAsync(HttpRequest req, string date, string nonce, CancellationToken ct)
    {
        string bodyHashHex = "";
        req.EnableBuffering();
        if (req.ContentLength is > 0)
        {
            using var ms = new MemoryStream();
            await req.Body.CopyToAsync(ms, ct);
            req.Body.Position = 0;
            var bodyBytes = ms.ToArray();
            var bodyHash = SHA256.HashData(bodyBytes);
            bodyHashHex = Convert.ToHexString(bodyHash).ToLowerInvariant();
        }

        var method = req.Method.ToUpperInvariant();
        var path = req.Path.ToString();
        var query = req.QueryString.HasValue ? req.QueryString.Value : string.Empty;
        return string.Join('\n', new[] { method, path, query, date, nonce, bodyHashHex });
    }

    private static string ComputeHmacBase64(string secret, string canonical)
    {
        var key = Encoding.UTF8.GetBytes(secret);
        var data = Encoding.UTF8.GetBytes(canonical);
        using var h = new HMACSHA256(key);
        return Convert.ToBase64String(h.ComputeHash(data));
    }

    private static bool FixedTimeEquals(string a, string b)
    {
        try
        {
            var ba = Convert.FromBase64String(a);
            var bb = Convert.FromBase64String(b);
            return CryptographicOperations.FixedTimeEquals(ba, bb);
        }
        catch { return string.Equals(a, b, StringComparison.Ordinal); }
    }
}
