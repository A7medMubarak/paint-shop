using PaintShop.Application.DTOs.Common;
using PaintShop.Application.DTOs.Customers;

namespace PaintShop.Application.Services.Interfaces;

public interface ICustomerService
{
    Task<List<CustomerDto>> GetAllAsync();
    Task<PagedResult<CustomerDto>> GetFilteredAsync(CustomerFilterRequest filter);
    Task<CustomerDetailDto> GetByIdAsync(int id);
    Task<CustomerDto> CreateAsync(CreateCustomerRequest request);
    Task<CustomerDto> UpdateAsync(int id, CreateCustomerRequest request);
    Task<List<CustomerDto>> SearchAsync(string query);
}
