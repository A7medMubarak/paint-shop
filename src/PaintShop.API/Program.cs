using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.EntityFrameworkCore;
using PaintShop.API.Extensions;
using PaintShop.API.Middleware;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddOpenApi();
builder.Services.AddAppSwagger();
builder.Services.AddAppAuth(builder.Configuration);
builder.Services.AddAppServices(builder.Configuration);
builder.Services.AddValidatorsFromAssemblyContaining<PaintShop.Application.Validators.LoginRequestValidator>();
builder.Services.AddFluentValidationAutoValidation();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseMiddleware<GlobalExceptionHandler>();
app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<PaintShop.Infrastructure.Persistence.ApplicationDbContext>();
    context.Database.Migrate();
    await PaintShop.Infrastructure.Persistence.SeedData.SeedAsync(context);
}

app.Run();
