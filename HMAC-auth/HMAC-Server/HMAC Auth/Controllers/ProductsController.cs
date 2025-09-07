using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProductApi.Data;
using ProductApi.Models;

namespace ProductApi.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Policy = "RequireProductManager")] // require signed request with ProductManager role
public class ProductsController : ControllerBase
{
    private readonly AppDbContext _db;
    public ProductsController(AppDbContext db) => _db = db;

    // GET: api/products
    [HttpGet("getAll")]
    public async Task<ActionResult<IEnumerable<Product>>> GetAll()
        => await _db.Products.AsNoTracking().OrderBy(p => p.Id).ToListAsync();

    // GET: api/products/{id}
    [HttpGet("getById")]
    public async Task<ActionResult<Product>> GetById(int id)
    {
        var p = await _db.Products.FindAsync(id);
        return p is null ? NotFound() : Ok(p);
    }

    // POST: api/products
    [HttpPost("create")]
    public async Task<ActionResult<Product>> Create(Product product)
    {
        // Server-side CreatedUtc
        product.CreatedUtc = DateTime.UtcNow;

        _db.Products.Add(product);
        await _db.SaveChangesAsync();
        return CreatedAtAction(nameof(GetById), new { id = product.Id }, product);
    }

    // PUT: api/products/{id}
    [HttpPut("update")]
    public async Task<IActionResult> Update(int id, Product update)
    {
        if (id != update.Id) return BadRequest("ID mismatch");

        var exists = await _db.Products.AsNoTracking().AnyAsync(p => p.Id == id);
        if (!exists) return NotFound();

        update.CreatedUtc = await _db.Products.Where(p => p.Id == id)
            .Select(p => p.CreatedUtc).FirstAsync(); // preserve original

        _db.Products.Update(update);
        await _db.SaveChangesAsync();
        return NoContent();
    }

    // DELETE: api/products/{id}
    [HttpDelete("delete")]
    public async Task<IActionResult> Delete(int id)
    {
        var p = await _db.Products.FindAsync(id);
        if (p is null) return NotFound();
        _db.Products.Remove(p);
        await _db.SaveChangesAsync();
        return NoContent();
    }
}
