using System.Net;
using System.Text;
using Microsoft.EntityFrameworkCore;
using PaintShop.Application.Services.Interfaces;
using PaintShop.Domain.Interfaces;

namespace PaintShop.Application.Services.Implementations;

public class InvoiceService : IInvoiceService
{
    private readonly IApplicationDbContext _context;

    public InvoiceService(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<string> GenerateHtmlAsync(int saleId)
    {
        var sale = await _context.Sales
            .Include(s => s.Customer)
            .Include(s => s.Employee)
            .Include(s => s.Items)
                .ThenInclude(si => si.Variant)
                    .ThenInclude(v => v.Product)
            .FirstOrDefaultAsync(s => s.Id == saleId)
            ?? throw new KeyNotFoundException("Sale not found");

        var subtotal = sale.Items.Sum(i => i.UnitPrice * i.Quantity);
        var sb = new StringBuilder();

        sb.AppendLine("<!DOCTYPE html>");
        sb.AppendLine("<html><head><meta charset=\"utf-8\">");
        sb.AppendLine("<title>Invoice</title>");
        sb.AppendLine("<style>");
        sb.AppendLine("body { font-family: Arial, sans-serif; margin: 40px; }");
        sb.AppendLine("h1 { color: #333; border-bottom: 2px solid #333; padding-bottom: 10px; }");
        sb.AppendLine("table { width: 100%; border-collapse: collapse; margin: 20px 0; }");
        sb.AppendLine("th, td { padding: 10px; text-align: left; border-bottom: 1px solid #ddd; }");
        sb.AppendLine("th { background-color: #f5f5f5; }");
        sb.AppendLine(".total { font-size: 1.2em; font-weight: bold; text-align: right; }");
        sb.AppendLine("</style></head><body>");
        sb.AppendLine($"<h1>Invoice #{sale.Id}</h1>");
        sb.AppendLine($"<p><strong>Date:</strong> {sale.CreatedAt:yyyy-MM-dd HH:mm}</p>");
        sb.AppendLine($"<p><strong>Customer:</strong> {WebUtility.HtmlEncode(sale.Customer?.Name ?? "-")}</p>");
        sb.AppendLine($"<p><strong>Phone:</strong> {WebUtility.HtmlEncode(sale.Customer?.Phone ?? "-")}</p>");
        sb.AppendLine($"<p><strong>Employee:</strong> {WebUtility.HtmlEncode(sale.Employee?.Username ?? "-")}</p>");
        sb.AppendLine("<table><thead><tr>");
        sb.AppendLine("<th>Product</th><th>Base</th><th>Size</th><th>Qty</th><th>Price</th><th>Total</th>");
        sb.AppendLine("</tr></thead><tbody>");

        foreach (var item in sale.Items)
        {
            var lineTotal = item.UnitPrice * item.Quantity;
            sb.AppendLine("<tr>");
            sb.AppendLine($"<td>{WebUtility.HtmlEncode(item.Variant?.Product?.Name ?? "-")}</td>");
            sb.AppendLine($"<td>{item.Variant?.BaseType?.ToString() ?? "-"}</td>");
            sb.AppendLine($"<td>{item.Variant?.SizeValue} {item.Variant?.SizeUnit}</td>");
            sb.AppendLine($"<td>{item.Quantity}</td>");
            sb.AppendLine($"<td>{item.UnitPrice:F2}</td>");
            sb.AppendLine($"<td>{lineTotal:F2}</td>");
            sb.AppendLine("</tr>");
        }

        sb.AppendLine("</tbody></table>");
        sb.AppendLine($"<p class=\"total\">Subtotal: {subtotal:F2}</p>");
        sb.AppendLine($"<p class=\"total\">Discount: {sale.DiscountAmount:F2}</p>");
        sb.AppendLine($"<p class=\"total\">Total: {sale.TotalAmount:F2}</p>");
        sb.AppendLine("<p><em>Status: " + sale.Status + "</em></p>");
        sb.AppendLine("</body></html>");

        return sb.ToString();
    }
}
