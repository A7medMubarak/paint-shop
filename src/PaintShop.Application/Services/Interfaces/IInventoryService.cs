using PaintShop.Application.DTOs.Common;
using PaintShop.Application.DTOs.Inventory;

namespace PaintShop.Application.Services.Interfaces;

public interface IInventoryService
{
    Task<List<InventoryItemDto>> GetAllAsync(int? location = null);
    Task<PagedResult<InventoryItemDto>> GetFilteredAsync(InventoryFilterRequest filter);
    Task<List<InventoryItemDto>> GetLowStockAsync();
    Task AddStockAsync(AddStockRequest request, int userId);
    Task AdjustStockAsync(AdjustStockRequest request, int userId);
    Task TransferStockAsync(TransferStockRequest request, int userId);
    Task<List<StockMovementDto>> GetMovementsAsync(int variantId);
}
