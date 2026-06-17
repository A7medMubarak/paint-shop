using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PaintShop.Application.DTOs.Common;
using PaintShop.Application.DTOs.Customers;
using PaintShop.Application.Services.Interfaces;

namespace PaintShop.API.Controllers;

[ApiController]
[Route("customers")]
[Authorize(Roles = "Owner,Employee")]
public class CustomersController : ControllerBase
{
    private readonly ICustomerService _customerService;

    public CustomersController(ICustomerService customerService)
    {
        _customerService = customerService;
    }

    [HttpGet]
    public async Task<ActionResult<List<CustomerDto>>> GetAll()
    {
        return Ok(await _customerService.GetAllAsync());
    }

    [HttpGet("filtered")]
    public async Task<ActionResult<PagedResult<CustomerDto>>> GetFiltered([FromQuery] CustomerFilterRequest filter)
    {
        return Ok(await _customerService.GetFilteredAsync(filter));
    }

    [HttpGet("search")]
    public async Task<ActionResult<List<CustomerDto>>> Search([FromQuery] string q)
    {
        return Ok(await _customerService.SearchAsync(q));
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<CustomerDetailDto>> GetById(int id)
    {
        return Ok(await _customerService.GetByIdAsync(id));
    }

    [HttpPost]
    public async Task<ActionResult<CustomerDto>> Create([FromBody] CreateCustomerRequest request)
    {
        var result = await _customerService.CreateAsync(request);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<CustomerDto>> Update(int id, [FromBody] CreateCustomerRequest request)
    {
        return Ok(await _customerService.UpdateAsync(id, request));
    }
}
