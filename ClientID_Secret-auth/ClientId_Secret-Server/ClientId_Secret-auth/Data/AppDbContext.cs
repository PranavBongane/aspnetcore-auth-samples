
using ClientID_SecretAuth.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace ClientID_SecretAuth.Api.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<ApiClient> ApiClients { get; set; }
}
