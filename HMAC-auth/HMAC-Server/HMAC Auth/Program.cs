using Microsoft.AspNetCore.Authentication;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using ProductApi.Data;
using ProductApi.Security;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

// EF Core: SQLite connection
builder.Services.AddDbContext<AppDbContext>(options =>
{
    var conn = builder.Configuration.GetConnectionString("DefaultConnection") 
               ?? "Data Source=ProductDb.db";
    options.UseSqlServer(conn);
});

// Controllers
builder.Services.AddControllers();

// Memory cache for nonce replay protection
builder.Services.AddMemoryCache();

// HMAC client registry from appsettings.json
builder.Services.AddSingleton<IClientSecretStore>(sp =>
{
    var cfg = sp.GetRequiredService<IConfiguration>().GetSection("Hmac:Clients")
        .Get<List<ClientCredential>>() ?? [];
    return new InMemoryClientSecretStore(cfg);
});

// AuthN + AuthZ
builder.Services.AddAuthentication("Hmac")
    .AddScheme<AuthenticationSchemeOptions, HmacAuthenticationHandler>("Hmac", _ => { });
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("RequireProductManager", p => p.RequireRole("ProductManager"));
});

// Swagger / OpenAPI
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddOpenApi();


var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

// Apply migrations if present; otherwise ensure created (dev convenience)
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    try { db.Database.Migrate(); }
    catch { db.Database.EnsureCreated(); }
}

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
