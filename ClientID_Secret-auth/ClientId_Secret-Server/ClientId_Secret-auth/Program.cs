
using ClientID_SecretAuth.Api.Data;
using ClientID_SecretAuth.Api.Security;
using Microsoft.AspNetCore.Authentication;
using Microsoft.EntityFrameworkCore;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer("Data Source=ClientSecrets.db"));

builder.Services.AddAuthentication("ClientSecret")
    .AddScheme<AuthenticationSchemeOptions, ClientSecretAuthHandler>("ClientSecret", _ => { });

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("RequireAdmin", policy => policy.RequireRole("Admin"));
    options.AddPolicy("RequireUser", policy => policy.RequireRole("User"));
});

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.Migrate();
}

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.Run();
