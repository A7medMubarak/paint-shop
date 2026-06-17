using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

namespace PaintShop.API.Extensions;

public static class AuthExtensions
{
    public static IServiceCollection AddAppAuth(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = configuration["Jwt:Issuer"] ?? "PaintShop.API",
                    ValidAudience = configuration["Jwt:Audience"] ?? "PaintShop.App",
                    IssuerSigningKey = new SymmetricSecurityKey(
                        Encoding.UTF8.GetBytes(configuration["Jwt:Key"] ?? "PaintShopDefaultSecretKey_ChangeInProduction!"))
                };
            });

        services.AddAuthorization();
        return services;
    }
}
