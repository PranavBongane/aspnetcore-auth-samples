using System.Net.Http.Json;
using System.Security.Cryptography;
using System.Text;

var baseAddress = "https://localhost:7260"; 
var clientId = "manager-1";
var secret = "pm-secret";

// Helper to send signed request
async Task<HttpResponseMessage> SendAsync(HttpMethod method, string path, object? body = null)
{
    var now = DateTimeOffset.UtcNow.ToString("r");
    var nonce = Guid.NewGuid().ToString("N");

    string query = string.Empty;
    string canonical;
    byte[] bodyBytes = Array.Empty<byte>();

    var request = new HttpRequestMessage(method, baseAddress + path);

    if (body is not null)
    {
        var json = System.Text.Json.JsonSerializer.Serialize(body);
        bodyBytes = Encoding.UTF8.GetBytes(json);
        request.Content = new StringContent(json, Encoding.UTF8, "application/json");
    }

    var bodyHashHex = bodyBytes.Length == 0 ? "" : Convert.ToHexString(SHA256.HashData(bodyBytes)).ToLowerInvariant();
    canonical = string.Join('\n', new[] { method.Method, path, query, now, nonce, bodyHashHex });
    var signature = ComputeHmacBase64(secret, canonical);

    request.Headers.TryAddWithoutValidation("x-client-id", clientId);
    request.Headers.TryAddWithoutValidation("x-date", now);
    request.Headers.TryAddWithoutValidation("x-nonce", nonce);
    request.Headers.TryAddWithoutValidation("x-signature", signature);

    var handler = new HttpClientHandler { ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator };
    using var http = new HttpClient(handler);
    return await http.SendAsync(request);
}

static string ComputeHmacBase64(string secret, string message)
{
    var key = Encoding.UTF8.GetBytes(secret);
    var data = Encoding.UTF8.GetBytes(message);
    using var h = new HMACSHA256(key);
    return Convert.ToBase64String(h.ComputeHash(data));
}

// Demonstrate CRUD
Console.WriteLine("== List (initial) ==");
var listResp = await SendAsync(HttpMethod.Get, "/api/products/getAll");
Console.WriteLine(await listResp.Content.ReadAsStringAsync());
Console.ReadLine();

Console.WriteLine("== Create ==");
var createBody = new { name = "Demo Product", sku = "SKU-002", price = 19.99m, description = "Sample item" };
var createResp = await SendAsync(HttpMethod.Post, "/api/products/create", createBody);
Console.WriteLine(createResp.StatusCode);
var createdJson = await createResp.Content.ReadAsStringAsync();
Console.WriteLine(createdJson);
Console.ReadLine();

// Extract new ID
var doc = System.Text.Json.JsonDocument.Parse(createdJson);
var id = doc.RootElement.GetProperty("id").GetInt32();

Console.WriteLine("== Update ==");
var updateBody = new { id, name = "Demo Product (Updated)", sku = "SKU-001", price = 29.99m, description = "Updated" };
var updateResp = await SendAsync(HttpMethod.Put, $"/api/products/update?id={id}&&update={updateBody}");
Console.WriteLine(updateResp.StatusCode);
Console.ReadLine();

Console.WriteLine("== Get One ==");
var getResp = await SendAsync(HttpMethod.Get, $"/api/products/getById/{id}");
Console.WriteLine(await getResp.Content.ReadAsStringAsync());
Console.ReadLine();

Console.WriteLine("== Delete ==");
var delResp = await SendAsync(HttpMethod.Delete, $"/api/products/delete/{id}");
Console.WriteLine(delResp.StatusCode);
Console.ReadLine();

Console.WriteLine("== List (final) ==");
var list2 = await SendAsync(HttpMethod.Get, "/api/products/getAll");
Console.WriteLine(await list2.Content.ReadAsStringAsync());
Console.ReadLine();
