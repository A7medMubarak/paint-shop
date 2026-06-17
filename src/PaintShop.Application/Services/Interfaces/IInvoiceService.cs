namespace PaintShop.Application.Services.Interfaces;

public interface IInvoiceService
{
    Task<string> GenerateHtmlAsync(int saleId);
}
