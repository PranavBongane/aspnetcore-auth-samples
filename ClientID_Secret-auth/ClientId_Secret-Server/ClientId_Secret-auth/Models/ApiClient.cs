
using System.ComponentModel.DataAnnotations;

namespace ClientID_SecretAuth.Api.Models;

public class ApiClient
{
    [Key]
    public int Id { get; set; }
    public string ClientId { get; set; } = string.Empty;
    public string SecretHash { get; set; } = string.Empty;
    public string Salt { get; set; } = string.Empty;
    public string Role { get; set; } = "User";
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
