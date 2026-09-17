using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PaintShop.Application.DTOs.Common;
using PaintShop.Application.DTOs.Products;
using PaintShop.Application.Services.Interfaces;

namespace PaintShop.API.Controllers;

[ApiController]
[Route("products")]
public class ProductsController : ControllerBase
{
    private readonly IProductService _productService;

    public ProductsController(IProductService productService)
    {
        _productService = productService;
    }

    [HttpGet]
    [Authorize(Roles = "Owner,Employee")]
    public async Task<ActionResult<List<ProductDto>>> GetAll()
    {
        return Ok(await _productService.GetAllAsync());
    }

    [HttpGet("{id}")]
    [Authorize(Roles = "Owner,Employee")]
    public async Task<ActionResult<ProductDto>> GetById(int id)
    {
        return Ok(await _productService.GetByIdAsync(id));
    }

    [HttpGet("filtered")]
    [Authorize(Roles = "Owner,Employee")]
    public async Task<ActionResult<PagedResult<ProductDto>>> GetFiltered([FromQuery] ProductFilterRequest filter)
    {
        return Ok(await _productService.GetFilteredAsync(filter));
    }

    [HttpGet("search")]
    [Authorize(Roles = "Owner,Employee")]
    public async Task<ActionResult<List<ProductDto>>> Search([FromQuery] string? q)
    {
        if (string.IsNullOrWhiteSpace(q))
            return Ok(new List<ProductDto>());
        return Ok(await _productService.SearchAsync(q));
    }

    [HttpPost]
    [Authorize(Roles = "Owner")]
    public async Task<ActionResult<ProductDto>> Create([FromBody] CreateProductRequest request)
    {
        var result = await _productService.CreateAsync(request);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "Owner")]
    public async Task<ActionResult<ProductDto>> Update(int id, [FromBody] CreateProductRequest request)
    {
        return Ok(await _productService.UpdateAsync(id, request));
    }

    [HttpPatch("{id}/toggle")]
    [Authorize(Roles = "Owner")]
    public async Task<IActionResult> ToggleActive(int id)
    {
        await _productService.ToggleActiveAsync(id);
        return NoContent();
    }

    [HttpPost("{productId}/variants")]
    [Authorize(Roles = "Owner")]
    public async Task<ActionResult<ProductVariantDto>> CreateVariant(int productId, [FromBody] CreateProductVariantRequest request)
    {
        var result = await _productService.CreateVariantAsync(productId, request);
        return CreatedAtAction(nameof(GetById), new { id = productId }, result);
    }

    [HttpPut("variants/{id}")]
    [Authorize(Roles = "Owner")]
    public async Task<ActionResult<ProductVariantDto>> UpdateVariant(int id, [FromBody] UpdateProductVariantRequest request)
    {
        return Ok(await _productService.UpdateVariantAsync(id, request));
    }

    [HttpPatch("variants/{id}/toggle")]
    [Authorize(Roles = "Owner")]
    public async Task<IActionResult> ToggleVariantActive(int id)
    {
        await _productService.ToggleVariantActiveAsync(id);
        return NoContent();
    }
}
