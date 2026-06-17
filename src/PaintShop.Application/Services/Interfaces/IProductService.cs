using PaintShop.Application.DTOs.Common;
using PaintShop.Application.DTOs.Products;

namespace PaintShop.Application.Services.Interfaces;

public interface IProductService
{
    Task<List<ProductDto>> GetAllAsync();
    Task<PagedResult<ProductDto>> GetFilteredAsync(ProductFilterRequest filter);
    Task<ProductDto> GetByIdAsync(int id);
    Task<ProductDto> CreateAsync(CreateProductRequest request);
    Task<ProductDto> UpdateAsync(int id, CreateProductRequest request);
    Task ToggleActiveAsync(int id);
    Task<ProductVariantDto> CreateVariantAsync(int productId, CreateProductVariantRequest request);
    Task<ProductVariantDto> UpdateVariantAsync(int id, UpdateProductVariantRequest request);
    Task ToggleVariantActiveAsync(int id);
    Task<List<ProductDto>> SearchAsync(string query);
}
