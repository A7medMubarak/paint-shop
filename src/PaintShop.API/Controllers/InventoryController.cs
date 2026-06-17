using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PaintShop.Application.DTOs.Common;
using PaintShop.Application.DTOs.Inventory;
using PaintShop.Application.Services.Interfaces;

namespace PaintShop.API.Controllers;

[ApiController]
[Route("inventory")]
[Authorize(Roles = "Owner,Employee")]
public class InventoryController : ControllerBase
{
    private readonly IInventoryService _inventoryService;

    public InventoryController(IInventoryService inventoryService)
    {
        _inventoryService = inventoryService;
    }

    [HttpGet]
    public async Task<ActionResult<List<InventoryItemDto>>> GetAll([FromQuery] int? location)
    {
        return Ok(await _inventoryService.GetAllAsync(location));
    }

    [HttpGet("filtered")]
    public async Task<ActionResult<PagedResult<InventoryItemDto>>> GetFiltered([FromQuery] InventoryFilterRequest filter)
    {
        return Ok(await _inventoryService.GetFilteredAsync(filter));
    }

    [HttpGet("low-stock")]
    public async Task<ActionResult<List<InventoryItemDto>>> GetLowStock()
    {
        return Ok(await _inventoryService.GetLowStockAsync());
    }

    [HttpPost("add")]
    [Authorize(Roles = "Owner")]
    public async Task<IActionResult> AddStock([FromBody] AddStockRequest request)
    {
        var userId = GetUserId();
        await _inventoryService.AddStockAsync(request, userId);
        return NoContent();
    }

    [HttpPost("adjust")]
    [Authorize(Roles = "Owner")]
    public async Task<IActionResult> AdjustStock([FromBody] AdjustStockRequest request)
    {
        var userId = GetUserId();
        await _inventoryService.AdjustStockAsync(request, userId);
        return NoContent();
    }

    [HttpPost("transfer")]
    [Authorize(Roles = "Owner")]
    public async Task<IActionResult> TransferStock([FromBody] TransferStockRequest request)
    {
        var userId = GetUserId();
        await _inventoryService.TransferStockAsync(request, userId);
        return NoContent();
    }

    [HttpGet("{variantId}/movements")]
    [Authorize(Roles = "Owner")]
    public async Task<ActionResult<List<StockMovementDto>>> GetMovements(int variantId)
    {
        return Ok(await _inventoryService.GetMovementsAsync(variantId));
    }

    private int GetUserId() =>
        int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
}
