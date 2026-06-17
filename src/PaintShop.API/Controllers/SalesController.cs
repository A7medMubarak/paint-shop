using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PaintShop.Application.DTOs.Common;
using PaintShop.Application.DTOs.Sales;
using PaintShop.Application.Services.Interfaces;

namespace PaintShop.API.Controllers;

[ApiController]
[Route("sales")]
[Authorize(Roles = "Owner,Employee")]
public class SalesController : ControllerBase
{
    private readonly ISaleService _saleService;

    public SalesController(ISaleService saleService)
    {
        _saleService = saleService;
    }

    [HttpGet("filtered")]
    public async Task<ActionResult<PagedResult<SaleSummaryDto>>> GetFiltered([FromQuery] SaleFilterRequest filter)
    {
        return Ok(await _saleService.GetFilteredAsync(filter));
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<SaleDto>> GetById(int id)
    {
        return Ok(await _saleService.GetByIdAsync(id));
    }

    [HttpPost]
    public async Task<ActionResult<CreateSaleResponse>> Create([FromBody] CreateSaleRequest request)
    {
        var employeeId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var result = await _saleService.CreateSaleAsync(request, employeeId);
        return CreatedAtAction(nameof(GetById), new { id = result.Sale.Id }, result);
    }

    [HttpPatch("{id}/cancel")]
    [Authorize(Roles = "Owner")]
    public async Task<IActionResult> Cancel(int id)
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        await _saleService.CancelSaleAsync(id, userId);
        return NoContent();
    }


}
