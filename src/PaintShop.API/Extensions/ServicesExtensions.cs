using Microsoft.EntityFrameworkCore;
using PaintShop.Application.Common;
using PaintShop.Application.Services.Implementations;
using PaintShop.Application.Services.Interfaces;
using PaintShop.Domain.Interfaces;
using PaintShop.Infrastructure.Persistence;
using PaintShop.Application.Common.Interfaces;
using PaintShop.Infrastructure.Security;

namespace PaintShop.API.Extensions;

public static class ServicesExtensions
{
    public static IServiceCollection AddAppServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseSqlServer(configuration.GetConnectionString("DefaultConnection")));

        services.AddScoped<IApplicationDbContext>(sp =>
            sp.GetRequiredService<ApplicationDbContext>());

        services.Configure<InventoryOptions>(configuration.GetSection(InventoryOptions.SectionName));
        services.AddSingleton(sp => sp.GetRequiredService<Microsoft.Extensions.Options.IOptions<InventoryOptions>>().Value);

        services.AddScoped<IJwtTokenService, JwtTokenService>();
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IUserService, UserService>();
        services.AddScoped<IProductService, ProductService>();
        services.AddScoped<IInventoryService, InventoryService>();
        services.AddScoped<ISaleService, SaleService>();
        services.AddScoped<ICustomerService, CustomerService>();
        services.AddScoped<IInvoiceService, InvoiceService>();
        services.AddScoped<IReportService, ReportService>();

        return services;
    }
}
