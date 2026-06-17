using Microsoft.EntityFrameworkCore;
using PaintShop.Application.DTOs.Common;
using PaintShop.Application.DTOs.Customers;
using PaintShop.Application.DTOs.Sales;
using PaintShop.Application.Services.Interfaces;
using PaintShop.Domain.Entities;
using PaintShop.Domain.Interfaces;

namespace PaintShop.Application.Services.Implementations;

public class CustomerService : ICustomerService
{
    private readonly IApplicationDbContext _context;

    public CustomerService(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<CustomerDto>> GetAllAsync()
    {
        var customers = await _context.Customers
            .OrderByDescending(c => c.CreatedAt)
            .ToListAsync();

        return customers.Select(c => new CustomerDto
        {
            Id = c.Id,
            Name = c.Name,
            Phone = c.Phone,
            CreatedAt = c.CreatedAt
        }).ToList();
    }

    public async Task<CustomerDetailDto> GetByIdAsync(int id)
    {
        var customer = await _context.Customers
            .Include(c => c.Sales)
                .ThenInclude(s => s.Employee)
            .Include(c => c.Sales)
                .ThenInclude(s => s.Items)
            .FirstOrDefaultAsync(c => c.Id == id)
            ?? throw new KeyNotFoundException("Customer not found");

        return new CustomerDetailDto
        {
            Id = customer.Id,
            Name = customer.Name,
            Phone = customer.Phone,
            CreatedAt = customer.CreatedAt,
            RecentSales = customer.Sales
                .OrderByDescending(s => s.CreatedAt)
                .Take(10)
                .Select(s => new SaleSummaryDto
                {
                    Id = s.Id,
                    EmployeeName = s.Employee?.Username ?? "",
                    CustomerName = customer.Name,
                    TotalAmount = s.TotalAmount,
                    Status = s.Status.ToString(),
                    CreatedAt = s.CreatedAt,
                    ItemCount = s.Items.Count
                })
                .ToList()
        };
    }

    public async Task<CustomerDto> CreateAsync(CreateCustomerRequest request)
    {
        var customer = new Customer
        {
            Name = request.Name,
            Phone = request.Phone,
            CreatedAt = DateTime.Now
        };

        _context.Customers.Add(customer);
        await _context.SaveChangesAsync();

        return new CustomerDto
        {
            Id = customer.Id,
            Name = customer.Name,
            Phone = customer.Phone,
            CreatedAt = customer.CreatedAt
        };
    }

    public async Task<PagedResult<CustomerDto>> GetFilteredAsync(CustomerFilterRequest filter)
    {
        if (filter.Page < 1) filter.Page = 1;
        if (filter.PageSize < 1) filter.PageSize = 20;

        var query = _context.Customers.AsQueryable();

        if (!string.IsNullOrWhiteSpace(filter.Search))
            query = query.Where(c => c.Name.Contains(filter.Search) || (c.Phone != null && c.Phone.Contains(filter.Search)));

        var totalCount = await query.CountAsync();

        var customers = await query
            .OrderByDescending(c => c.CreatedAt)
            .Skip((filter.Page - 1) * filter.PageSize)
            .Take(filter.PageSize)
            .ToListAsync();

        return new PagedResult<CustomerDto>
        {
            Items = customers.Select(c => new CustomerDto
            {
                Id = c.Id,
                Name = c.Name,
                Phone = c.Phone,
                CreatedAt = c.CreatedAt
            }).ToList(),
            TotalCount = totalCount,
            Page = filter.Page,
            PageSize = filter.PageSize
        };
    }

    public async Task<List<CustomerDto>> SearchAsync(string query)
    {
        var customers = await _context.Customers
            .Where(c => c.Name.Contains(query) || (c.Phone != null && c.Phone.Contains(query)))
            .OrderByDescending(c => c.CreatedAt)
            .ToListAsync();

        return customers.Select(c => new CustomerDto
        {
            Id = c.Id,
            Name = c.Name,
            Phone = c.Phone,
            CreatedAt = c.CreatedAt
        }).ToList();
    }

    public async Task<CustomerDto> UpdateAsync(int id, CreateCustomerRequest request)
    {
        var customer = await _context.Customers.FindAsync(id)
            ?? throw new KeyNotFoundException("Customer not found");

        customer.Name = request.Name;
        customer.Phone = request.Phone;

        await _context.SaveChangesAsync();

        return new CustomerDto
        {
            Id = customer.Id,
            Name = customer.Name,
            Phone = customer.Phone,
            CreatedAt = customer.CreatedAt
        };
    }
}
