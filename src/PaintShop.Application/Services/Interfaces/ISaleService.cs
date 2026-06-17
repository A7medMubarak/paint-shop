using PaintShop.Application.DTOs.Common;
using PaintShop.Application.DTOs.Sales;

namespace PaintShop.Application.Services.Interfaces;

public interface ISaleService
{
    Task<CreateSaleResponse> CreateSaleAsync(CreateSaleRequest request, int employeeId);
    Task<SaleDto> GetByIdAsync(int id);
    Task<PagedResult<SaleSummaryDto>> GetFilteredAsync(SaleFilterRequest filter);
    Task CancelSaleAsync(int saleId, int userId);
}
