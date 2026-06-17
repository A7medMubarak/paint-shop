using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PaintShop.Application.Services.Interfaces;

namespace PaintShop.API.Controllers;

[ApiController]
[Route("sales")]
[Authorize(Roles = "Owner,Employee")]
public class InvoiceController : ControllerBase
{
    private readonly IInvoiceService _invoiceService;

    public InvoiceController(IInvoiceService invoiceService)
    {
        _invoiceService = invoiceService;
    }

    [HttpGet("{saleId}/invoice")]
    public async Task<ActionResult<string>> GetInvoice(int saleId)
    {
        var html = await _invoiceService.GenerateHtmlAsync(saleId);
        return Content(html, "text/html");
    }
}
