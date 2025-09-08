
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ClientID_SecretAuth.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProductController : ControllerBase
{
    private static readonly List<string> Products = new() { "Laptop", "Phone", "Tablet" };

    [HttpGet]
    [Authorize(Policy = "RequireUser")]
    public IActionResult GetProducts() => Ok(Products);

    [HttpPost]
    [Authorize(Policy = "RequireAdmin")]
    public IActionResult AddProduct([FromBody] string product)
    {
        Products.Add(product);
        return Ok(Products);
    }
}
