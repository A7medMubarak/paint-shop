# Paint Shop Manager — Architecture & Conventions

## Project Structure

```
PaintShop.sln
├── src/
│   ├── PaintShop.Domain/           # Innermost layer — zero dependencies
│   │   ├── Entities/               # 9 entity classes
│   │   ├── Enums/                  # 6 enums (stored as int in DB)
│   │   └── Interfaces/             # IApplicationDbContext
│   ├── PaintShop.Application/      # Business logic — depends only on Domain
│   │   ├── Common/                 # SaleCalculator, StockWarningHelper, InventoryOptions
│   │   ├── DTOs/                   # Request/response DTOs organized by feature
│   │   ├── Services/
│   │   │   ├── Interfaces/         # 8 service interfaces
│   │   │   └── Implementations/    # Service implementations
│   │   └── Validators/             # FluentValidation validators
│   ├── PaintShop.Infrastructure/   # Persistence, JWT — depends on Application + Domain
│   │   ├── Persistence/
│   │   │   ├── Configurations/     # EF Core IEntityTypeConfiguration (9 files)
│   │   │   └── Migrations/         # EF Core migrations
│   │   └── Security/               # JwtTokenService
│   └── PaintShop.API/             # HTTP entry point — depends on Infrastructure
│       ├── Controllers/            # 8 API controllers
│       ├── Extensions/             # Service registration, auth, swagger
│       └── Middleware/             # GlobalExceptionHandler
├── tests/
│   └── PaintShop.Application.Tests/  # xUnit tests
└── frontend/                         # React 19 SPA
```

**Dependency rules** (enforced by project references):
- Domain → (nothing)
- Application → Domain
- Infrastructure → Application, Domain
- API → Infrastructure

Never reference a layer from a layer it doesn't depend on (e.g., API must not reference Domain directly).

## Naming Conventions

| Language | Convention | Examples |
|---|---|---|
| **C#** | PascalCase for classes, methods, properties, namespaces, public fields | `ProductService`, `GetAllAsync`, `ProductDto` |
| **C#** | `_camelCase` for private fields | `_context`, `_options` |
| **C#** | `camelCase` for method parameters, local variables | `request`, `filter`, `totalCount` |
| **C#** | File-scoped namespaces | `namespace PaintShop.Application.Services;` |
| **C#** | Interfaces prefixed with `I` | `IProductService`, `IApplicationDbContext` |
| **TypeScript** | PascalCase for types/interfaces/components | `ProductDto`, `Pagination`, `PagedResult<T>` |
| **TypeScript** | camelCase for variables, functions, hooks | `fetchData`, `handleSearch`, `useAuth` |
| **TypeScript** | Files match default export name | `ProductsPage.tsx`, `api.ts` |
| **Routes** | kebab-case | `/products/filtered`, `/inventory/low-stock` |
| **DB tables** | PascalCase plural | `Products`, `InventoryItems`, `StockMovements` |

## Controller Patterns

```csharp
[ApiController]
[Route("products")]
[Authorize(Roles = "Owner,Employee")]
public class ProductsController : ControllerBase
{
    private readonly IProductService _productService;

    public ProductsController(IProductService productService)
    {
        _productService = productService;
    }

    [HttpGet("filtered")]
    public async Task<ActionResult<PagedResult<ProductDto>>> GetFiltered(
        [FromQuery] ProductFilterRequest filter)
    {
        return Ok(await _productService.GetFilteredAsync(filter));
    }

    [HttpPost]
    [Authorize(Roles = "Owner")]
    public async Task<ActionResult<ProductDto>> Create(
        [FromBody] CreateProductRequest request)
    {
        var result = await _productService.CreateAsync(request);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }
}
```

**Rules:**
- Controller actions are thin — validate, call service, return result
- Use `[Authorize(Roles = "...")]` from most permissive to most restrictive
- Routes are plural nouns, no verbs (`/products` not `/getProducts`)
- Return `CreatedAtAction` for POST, `NoContent` for successful deletes/patches
- Use `[FromQuery]` for filter DTOs, `[FromBody]` for command DTOs
- Never put business logic in controllers

## Service Patterns

```csharp
public class ProductService : IProductService
{
    private readonly IApplicationDbContext _context;

    public ProductService(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<PagedResult<ProductDto>> GetFilteredAsync(
        ProductFilterRequest filter)
    {
        // 1. Validate/constrain inputs
        if (filter.Page < 1) filter.Page = 1;

        // 2. Build query with filters
        var query = _context.Products.Include(p => p.Variants).AsQueryable();
        if (!string.IsNullOrWhiteSpace(filter.Search))
            query = query.Where(p => p.Name.Contains(filter.Search));

        // 3. Count + paginate
        var totalCount = await query.CountAsync();
        var items = await query
            .OrderBy(p => p.Name)
            .Skip((filter.Page - 1) * filter.PageSize)
            .Take(filter.PageSize)
            .ToListAsync();

        // 4. Map and return
        return new PagedResult<ProductDto> { Items = ..., TotalCount = ... };
    }
}
```

**Rules:**
- Services are stateless and scoped (one instance per request)
- All public methods are `async Task<T>`
- Throw standard .NET exceptions: `KeyNotFoundException`, `InvalidOperationException`, `UnauthorizedAccessException`
- Never return `IQueryable` from a service method
- Use `IApplicationDbContext` interface (not the concrete `DbContext`)
- Services accept DTOs and return DTOs

## DTO Patterns

- Request DTOs: Plain objects with properties matching the input fields
- Response DTOs: Plain objects with properties matching the output fields
- Filter DTOs: Contain `Page = 1`, `PageSize = 20`, and optional filter fields
- No behavior, no validation, no business logic in DTOs
- Use file-scoped namespaces matching the feature folder

## Validator Patterns

```csharp
public class CreateProductRequestValidator : AbstractValidator<CreateProductRequest>
{
    public CreateProductRequestValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .MaximumLength(100);

        RuleFor(x => x.ProductCategory)
            .InclusiveBetween(0, 2);
    }
}
```

**Rules:**
- One validator per request DTO
- Validators auto-register via `AddValidatorsFromAssemblyContaining<>()` in `Program.cs`
- Validators auto-apply via `AddFluentValidationAutoValidation()`
- Password validators require: min 8 chars, uppercase, lowercase, digit
- Use `When()` for conditional rules on nullable properties

## Database Patterns

- EF Core code-first with `IEntityTypeConfiguration` classes (one per entity)
- All configurations applied via `ApplyConfigurationsFromAssembly` in `ApplicationDbContext`
- Enums stored as integers with `.HasConversion<int>()` (implicit via `SaveChanges`)
- Decimal properties use `.HasPrecision(18, 2)` or `.HasPrecision(10, 2)` as appropriate
- String properties use `.HasMaxLength(n)` constraints
- Relationships use `DeleteBehavior.Restrict` to prevent accidental cascade deletes
- Unique constraints via `.HasIndex(...).IsUnique()`

### Entity Configuration Template

```csharp
public class ProductConfiguration : IEntityTypeConfiguration<Product>
{
    public void Configure(EntityTypeBuilder<Product> builder)
    {
        builder.ToTable("Products");
        builder.HasKey(p => p.Id);
        builder.Property(p => p.Name).HasMaxLength(100).IsRequired();
        builder.Property(p => p.ProductCategory).IsRequired();
        builder.Property(p => p.IsActive).IsRequired().HasDefaultValue(true);
        builder.Property(p => p.CreatedAt).IsRequired();
    }
}
```

## Testing Patterns

- **Framework**: xUnit + Moq + FluentAssertions + EF Core InMemory
- **MockDbContext**: Creates InMemory `ApplicationDbContext` with seed data
- **Naming**: `MethodName_Scenario_ExpectedResult`

```csharp
[Fact]
public async Task GetFilteredAsync_WithSearch_FiltersByName()
{
    // Arrange
    var ctx = MockDbContext.Create(products: products, variants: variants);
    var service = new ProductService(ctx, _options);

    // Act
    var result = await service.GetFilteredAsync(
        new ProductFilterRequest { Search = "Blue", Page = 1, PageSize = 20 });

    // Assert
    result.Items.Should().ContainSingle();
    result.Items[0].Name.Should().Be("Blue Paint");
}
```

- Service tests: Use `MockDbContext.Create()` with seed data, instantiate service directly
- Validator tests: Instantiate validator directly, call `TestValidate()`
- Auth tests: Mock `IJwtTokenService` with Moq
- Use `Theory` + `MemberData` for parameterized tests with nullable parameters

## Exception Handling

The `GlobalExceptionHandler` middleware converts exceptions to ProblemDetails (RFC 7807):

| Exception | HTTP Status | Example |
|---|---|---|
| `KeyNotFoundException` | 404 Not Found | Entity not found |
| `InvalidOperationException` | 400 Bad Request | Insufficient stock, duplicate username |
| `UnauthorizedAccessException` | 401 Unauthorized | Invalid login |
| `ArgumentException` | 400 Bad Request | Invalid input |
| `ValidationException` | 400 Bad Request | FluentValidation auto-handled |
| Unhandled | 500 Internal Server Error | Unexpected errors |

## Authentication Flow

1. Client sends `POST /auth/login` with `{ username, password }`
2. Server looks up user by username (case-sensitive)
3. Server verifies password with `BCrypt.Net.BCrypt.Verify()`
4. Server generates JWT with user ID, username, role claims (24h expiry)
5. Client stores token in `localStorage`
6. Client sends token as `Authorization: Bearer <token>` header
7. Server validates token on every request via `AddJwtBearer`
8. `[Authorize(Roles = "Owner,Employee")]` enforces role-based access

## How to Add a New Feature

### Backend (12 steps)
1. Add entity to `PaintShop.Domain/Entities/` (if new table)
2. Add/update enum in `PaintShop.Domain/Enums/` (if new enum)
3. Add `DbSet<T>` to `IApplicationDbContext` and `ApplicationDbContext`
4. Add EF Core configuration in `PaintShop.Infrastructure/Persistence/Configurations/`
5. Create migration: `dotnet ef migrations add <Name>`
6. Add DTOs in `PaintShop.Application/DTOs/<Feature>/`
7. Add FluentValidation validator in `PaintShop.Application/Validators/`
8. Add service interface in `PaintShop.Application/Services/Interfaces/`
9. Implement service in `PaintShop.Application/Services/Implementations/`
10. Register service in `PaintShop.API/Extensions/ServicesExtensions.cs`
11. Add controller in `PaintShop.API/Controllers/`
12. Write tests in `tests/PaintShop.Application.Tests/`

### Frontend (3 steps)
1. Add API types to `frontend/src/types/index.ts`
2. Add page component in `frontend/src/pages/`
3. Add route in `frontend/src/App.tsx` and sidebar link in `frontend/src/components/Sidebar.tsx`

## Configuration

`src/PaintShop.API/appsettings.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost\\SQLEXPRESS;Database=PaintShop;Trusted_Connection=True;TrustServerCertificate=True;MultipleActiveResultSets=true"
  },
  "Jwt": {
    "Secret": "dev-secret-key-change-in-production",
    "Issuer": "PaintShop",
    "Audience": "PaintShop",
    "ExpiryHours": 24
  },
  "Inventory": {
    "DefaultLowStockThreshold": 5.0
  }
}
```

**Security notes:**
- Never commit real secrets to git
- Use `appsettings.Development.json` with `{ "Jwt": { "Secret": "dev-key" } }` for local dev
- In production, use environment variables or Azure Key Vault
