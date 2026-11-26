using LAB_10_PMS_API.Data;
using LAB_10_PMS_API.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LAB_10_PMS_API.Controllers;

[Route("api/[controller]")]
[ApiController]
public class ProductsController(AppDbContext context, IWebHostEnvironment environment) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IEnumerable<Product>>> GetProducts()
    {
        return Ok(await context.Products.AsNoTracking().ToListAsync());
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<Product>> GetProduct(int id)
    {
        var product = await context.Products.FindAsync(id);
        return product is null ? NotFound("Product not found") : Ok(product);
    }

    [HttpPost]
    public async Task<ActionResult<Product>> CreateProduct([FromForm] Product product, IFormFile? photo)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        if (photo is null)
            return BadRequest("Product image is required");

        var imageError = ValidateImage(photo);
        if (imageError is not null)
            return BadRequest(imageError);

        product.Image = await SaveImage(photo);
        context.Products.Add(product);
        await context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetProduct), new { id = product.Id }, product);
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<Product>> UpdateProduct(int id, [FromForm] Product updatedProduct, IFormFile? photo)
    {
        var product = await context.Products.FindAsync(id);
        if (product is null)
            return NotFound("Product not found");

        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        if (photo is not null)
        {
            var imageError = ValidateImage(photo);
            if (imageError is not null)
                return BadRequest(imageError);

            var oldImage = product.Image;
            product.Image = await SaveImage(photo);
            DeleteImage(oldImage);
        }

        product.Title = updatedProduct.Title;
        product.Description = updatedProduct.Description;
        product.Price = updatedProduct.Price;
        product.Quantity = updatedProduct.Quantity;
        product.Status = updatedProduct.Status;

        await context.SaveChangesAsync();
        return Ok(product);
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> DeleteProduct(int id)
    {
        var product = await context.Products.FindAsync(id);
        if (product is null)
            return NotFound("Product not found");

        DeleteImage(product.Image);
        context.Products.Remove(product);
        await context.SaveChangesAsync();
        return Ok("Product deleted successfully");
    }

    private static string? ValidateImage(IFormFile photo)
    {
        var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".webp" };
        var extension = Path.GetExtension(photo.FileName).ToLowerInvariant();

        if (!allowedExtensions.Contains(extension))
            return "Only JPG, JPEG, PNG and WEBP images are allowed.";
        if (photo.Length == 0)
            return "Empty image is not allowed.";
        if (photo.Length > 5 * 1024 * 1024)
            return "Maximum image size is 5 MB.";

        return null;
    }

    private async Task<string> SaveImage(IFormFile photo)
    {
        var folder = Path.Combine(environment.WebRootPath ?? Path.Combine(environment.ContentRootPath, "wwwroot"), "uploads");
        Directory.CreateDirectory(folder);

        var fileName = $"{Guid.NewGuid()}{Path.GetExtension(photo.FileName).ToLowerInvariant()}";
        await using var stream = System.IO.File.Create(Path.Combine(folder, fileName));
        await photo.CopyToAsync(stream);
        return $"/uploads/{fileName}";
    }

    private void DeleteImage(string image)
    {
        if (string.IsNullOrWhiteSpace(image))
            return;

        var path = Path.Combine(environment.WebRootPath ?? Path.Combine(environment.ContentRootPath, "wwwroot"), "uploads", Path.GetFileName(image));
        if (System.IO.File.Exists(path))
            System.IO.File.Delete(path);
    }
}