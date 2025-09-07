using System.ComponentModel.DataAnnotations;

namespace ProductApi.Models;

// Simple Product entity for CRUD
public class Product
{
    public int Id { get; set; }

    [Required, MaxLength(120)]
    public string Name { get; set; } = string.Empty;

    [Required, MaxLength(64)]
    public string Sku { get; set; } = string.Empty;

    [Range(0, double.MaxValue)]
    public decimal Price { get; set; }

    public string? Description { get; set; }

    public DateTime CreatedUtc { get; set; } = DateTime.UtcNow;
}
