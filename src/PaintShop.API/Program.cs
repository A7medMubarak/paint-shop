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

var frontendUrls = (builder.Configuration["Frontend:Urls"] ?? "http://localhost:5173")
    .Split(';', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
builder.Services.AddCors(options =>
{
    options.AddPolicy("Frontend", policy => policy
        .WithOrigins(frontendUrls)
        .AllowAnyHeader()
        .AllowAnyMethod());
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwagger();
    app.UseSwaggerUI();
    app.MapGet("/", () => Results.Redirect("http://localhost:5173"));
}

app.UseMiddleware<GlobalExceptionHandler>();
app.UseHttpsRedirection();
app.UseCors("Frontend");
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
