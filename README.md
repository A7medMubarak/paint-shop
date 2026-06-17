# Paint Shop Manager

A full-stack paint shop management system built with **.NET 9** (Clean Architecture) and **React 19** (Vite + Tailwind CSS). Manages products, inventory (two-location), sales, customers, reports, and users with role-based access control.

## Tech Stack

| Layer | Technology |
|---|---|
| **Backend** | .NET 9, ASP.NET Core, EF Core 9, FluentValidation 11 |
| **Frontend** | React 19, TypeScript, Vite 8, Tailwind CSS 4 |
| **Database** | SQL Server Express (LocalDB via `localhost\SQLEXPRESS`) |
| **Auth** | JWT (24h expiry), BCrypt password hashing, role-based (Owner/Employee) |
| **Testing** | xUnit, Moq, FluentAssertions, EF Core InMemory |
| **Logging** | Built-in ASP.NET Core logging |

## Features

- **Dashboard** — Today's stats (sales count, revenue, low stock alerts, recent sales)
- **Products** — Create/edit products and variants with paint categories (GlcPlastic, GlcDecore, GlcOilBased) and base types (BaseA, BaseB, BaseC, White, Silver, Gold)
- **Inventory** — Two-location tracking (Shop + Warehouse), add/adjust/transfer stock, low stock alerts
- **Sales** — Create sales with per-line discount tracking, search/filter, cancel with auto stock restoration
- **Customers** — CRUD with search, purchase history view
- **Reports** — Daily, Period, Top Selling, Low Stock, Inventory Valuation — all with CSV export and Print
- **Users** — Employee management with password reset and deactivation

## API Endpoints

### Auth
| Method | Route | Auth | Description |
|---|---|---|---|
| POST | `/auth/login` | Public | Login with username/password |
| GET | `/auth/current-user` | Owner, Employee | Get current user info |

### Products
| Method | Route | Auth | Description |
|---|---|---|---|
| GET | `/products` | Owner, Employee | Get all products (for dropdowns) |
| GET | `/products/filtered` | Owner, Employee | Paginated list with search |
| GET | `/products/{id}` | Owner, Employee | Get by ID |
| GET | `/products/search?q=` | Owner, Employee | Search by name |
| POST | `/products` | Owner | Create product |
| PUT | `/products/{id}` | Owner | Update product |
| PATCH | `/products/{id}/toggle` | Owner | Toggle active status |
| POST | `/{id}/variants` | Owner | Create variant |
| PUT | `/variants/{id}` | Owner | Update variant |
| PATCH | `/variants/{id}/toggle` | Owner | Toggle variant active |

### Customers
| Method | Route | Auth | Description |
|---|---|---|---|
| GET | `/customers` | Owner, Employee | Get all customers (for dropdowns) |
| GET | `/customers/filtered` | Owner, Employee | Paginated list with search |
| GET | `/customers/{id}` | Owner, Employee | Customer detail with recent sales |
| POST | `/customers` | Owner, Employee | Create customer |
| PUT | `/customers/{id}` | Owner, Employee | Update customer |

### Sales
| Method | Route | Auth | Description |
|---|---|---|---|
| GET | `/sales/filtered` | Owner, Employee | Paginated list with date/status/search |
| GET | `/sales/{id}` | Owner, Employee | Sale detail with items |
| POST | `/sales` | Owner, Employee | Create sale |
| PATCH | `/{id}/cancel` | Owner | Cancel sale (restores stock) |

### Inventory
| Method | Route | Auth | Description |
|---|---|---|---|
| GET | `/inventory/filtered` | Owner, Employee | Paginated list with location/search |
| GET | `/inventory/low-stock` | Owner, Employee | Low stock report |
| POST | `/inventory/add` | Owner | Add stock |
| POST | `/inventory/adjust` | Owner | Adjust stock |
| POST | `/inventory/transfer` | Owner | Transfer between locations |
| GET | `/{variantId}/movements` | Owner | Stock movement history |

### Reports
| Method | Route | Auth | Description |
|---|---|---|---|
| GET | `/reports/daily?date=` | Owner, Employee | Daily report |
| GET | `/reports/period?from=&to=` | Owner, Employee | Period report |
| GET | `/reports/top-selling?from=&to=` | Owner, Employee | Top selling products |
| GET | `/reports/inventory-valuation?location=` | Owner | Inventory valuation |

### Users
| Method | Route | Auth | Description |
|---|---|---|---|
| GET | `/users` | Owner | List all users |
| POST | `/users` | Owner | Create employee |
| PUT | `/users/{id}` | Owner | Update user |
| PATCH | `/{id}/deactivate` | Owner | Deactivate user |
| PATCH | `/{id}/password` | Owner | Reset password |

### Invoice
| Method | Route | Auth | Description |
|---|---|---|---|
| GET | `/sales/{saleId}/invoice` | Owner, Employee | Generate invoice |

## Domain Rules

- **Two-location inventory**: Each variant has Shop + Warehouse stock tracked separately
- **Quantity precision**: All quantities are `DECIMAL` (not `INT`)
- **Paint categories**: `GlcPlastic` (0), `GlcDecore` (1), `GlcOilBased` (2)
- **Base types**: `BaseA`, `BaseB`, `BaseC`, `White`, `Silver`, `Gold` — nullable for non-paint products
- **Invoice discount**: Invoice-level `DiscountAmount` + per-line tracking (`OriginalPrice - UnitPrice`)
- **Low stock**: Alert when shop stock drops below variant threshold or system default (5)
- **Negative inventory**: Allowed only with explicit `ForceNegativeInventory` flag

## Quick Start

1. **Clone the repository**
   ```
   git clone https://github.com/A7medMubarak/paint-shop.git
   cd paint-shop
   ```

2. **Set up the database**
   - Ensure SQL Server Express is running on `localhost\SQLEXPRESS`
   - Update connection string in `src/PaintShop.API/appsettings.json` if needed
   ```
   dotnet ef database update --project src/PaintShop.Infrastructure --startup-project src/PaintShop.API
   ```
   Or run the API — migrations apply automatically on startup.

3. **Run the backend**
   ```
   cd src/PaintShop.API
   dotnet run
   ```
   API runs at `http://localhost:5000`. Swagger at `http://localhost:5000/swagger`.

4. **Run the frontend**
   ```
   cd frontend
   npm install
   npm run dev
   ```
   Frontend runs at `http://localhost:5173` and proxies `/api` to `http://localhost:5000`.

5. **Login**
   - Username: `admin`
   - Password: `admin123`

6. **Run tests**
   ```
   dotnet test tests/PaintShop.Application.Tests
   ```

## Project Structure

```
PaintShop/
├── frontend/                      # React 19 + Vite + Tailwind
│   ├── src/
│   │   ├── components/            # Shared UI components
│   │   ├── pages/                 # 17 page components
│   │   ├── services/              # API client + auth helpers
│   │   ├── context/               # Auth context + provider
│   │   ├── hooks/                 # Custom hooks
│   │   ├── types/                 # TypeScript interfaces
│   │   └── utils/                 # CSV export utility
│   └── ...
├── src/
│   ├── PaintShop.Domain/          # Entities, Enums, Interfaces
│   ├── PaintShop.Application/     # Services, DTOs, Validators
│   ├── PaintShop.Infrastructure/  # EF Core, JWT, Persistence
│   └── PaintShop.API/             # Controllers, Middleware
├── tests/
│   └── PaintShop.Application.Tests/  # 72 xUnit tests
├── .gitignore
├── AGENTS.md                      # Architecture conventions
├── README.md
└── PaintShop.sln
```

## Architecture

Clean Architecture with strict dependency layering:

```
PaintShop.Domain       (no dependencies)
       ↑
PaintShop.Application  (depends on Domain)
       ↑
PaintShop.Infrastructure (depends on Application + Domain)
       ↑
PaintShop.API         (depends on Infrastructure)
```

- Controllers are thin — they validate, delegate to services, return results
- Services contain business logic, throw standard .NET exceptions
- FluentValidation validators auto-register and auto-apply to all requests
- Global exception handler converts exceptions to ProblemDetails (RFC 7807)
- JWT authentication with Owner/Employee roles on every endpoint

## Configuration

Key settings in `src/PaintShop.API/appsettings.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost\\SQLEXPRESS;Database=PaintShop;Trusted_Connection=True;TrustServerCertificate=True;MultipleActiveResultSets=true"
  },
  "Jwt": {
    "Secret": "your-secret-key-at-least-32-characters",
    "Issuer": "PaintShop",
    "Audience": "PaintShop",
    "ExpiryHours": 24
  }
}
```

## Testing

72 tests across 3 categories:

- **Common (10)**: `SaleCalculator`, `StockWarningHelper`
- **Services (46)**: Product, Customer, Sale, Inventory, User, Auth
- **Validators (16)**: Login, Customer, Product, Variant, Stock, Sale, Employee, Password

Pattern: xUnit + Moq + FluentAssertions + EF Core InMemory with shared `MockDbContext` helper.
