# Paint Shop Manager — Engineering Case Study

> How 5 years behind a GLC tinting counter shaped a Clean Architecture system in ASP.NET Core 9 + React 19.

---

## 1. Business Analysis

A paint shop counter runs on five workflows: **catalog** (what we sell), **inventory** (where it sits), **sales** (what leaves), **customers** (who buys), and **oversight** (what the owner reviews). The domain model mirrors them 1:1:

| Entity | Counter meaning | Key design |
|---|---|---|
| `Product` | A paint line (GLC Plastic…) | `ProductCategory` enum, `IsActive` soft-toggle |
| `ProductVariant` | A sellable can: base + size + price | Nullable `BaseType`, decimal `SizeValue` + `SizeUnit`, nullable prices |
| `InventoryItem` | Cans on a shelf | One row per (variant, location); decimal `Quantity` |
| `StockMovement` | Every can that moved, and why | `QuantityChange` ±, `Reason` enum, links to sale/transfer/user |
| `StockTransfer` | Shelf-to-store restocking trip | Header + paired ± movements |
| `Sale` / `SaleItem` | The invoice | Header discount + per-line price snapshot + tint `ColorCode` |
| `Customer` | Contractor or walk-in | "Walk-in" default seed for cash sales |
| `User` | Owner / employee | BCrypt hash, `IsActive` gate |

The single most consequential modeling decision: **inventory is two-location from day one** (`Shop` / `Warehouse`), because the counter question is always "do we have it *in the Shop*?" — Warehouse stock is storage, never sold, never low-stock-flagged.

---

## 2. Architecture

Strict Clean Architecture, enforced by project references:

```
PaintShop.Domain          → nothing
PaintShop.Application     → Domain
PaintShop.Infrastructure  → Application, Domain
PaintShop.API             → Infrastructure
```

- **Controllers are thin**: validate (via auto-applied FluentValidation), call one service method, return the result. No business logic, no direct Domain references from API.
- **Services own the rules**, accept DTOs, return DTOs, never leak `IQueryable`. Failures are standard .NET exceptions (`KeyNotFoundException` → 404, `InvalidOperationException`/`ArgumentException` → 400, `UnauthorizedAccessException` → 401) translated by `GlobalExceptionHandler` into RFC 7807 ProblemDetails.
- **14 validators** (one per request shape) auto-register and auto-apply — password policy (8+, upper/lower/digit) lives here, not in controllers.
- **Frontend** is a React 19 SPA (17 pages) talking to the API over a thin fetch client; all lists are server-paginated and filterable.

---

## 3. Request Lifecycle

`POST /sales` as the representative path:

```
Browser → JWT Bearer → [Auth] → FluentValidation → SalesController
  → SaleService (validate variants, check Shop stock, aggregate lines,
     compute totals, write sale + items + movements in ONE SaveChanges)
  → 201 Created + populated response DTO
```

Validation failures short-circuit before the controller body runs. Auth failures short-circuit before routing logic. The service is the transaction boundary.

---

## 4. Authentication & Authorization

- Login verifies BCrypt hash **and** `IsActive` (deactivated users are rejected — a real bug found and fixed pre-release).
- JWT (24h) carries id, username, role. Employees read and sell; Owners additionally manage stock, products, users, cancellations, and reports — enforced by `[Authorize(Roles=...)]` per action.
- Two password flows, deliberately separated: self-service change (requires current password) vs Owner reset (no current password — an owner can't know an employee's password). The original single-endpoint design made Owner resets impossible; splitting it was a domain-driven correction.
- Prod secrets (`Jwt:Key`, connection string, frontend origins) come from environment variables only — the committed default key was removed pre-release.

---

## 5. Key Business Rules (and why)

- **Shop-only deduction.** Sales, low-stock, and availability checks touch Shop inventory exclusively. Matches the counter; prevents selling storage stock.
- **`ForceNegativeInventory`.** Selling before stock arrives is legitimate (backorders). The flag permits it but returns `InventoryWarning` + variant ids — the system records the exception instead of silently allowing it.
- **Price snapshotting.** `SaleItem.OriginalPrice` freezes the variant price at sale time; `UnitPrice` is what was charged. Their difference is the auditable per-line discount, computed by `SaleCalculator.TotalPerLineDiscount`. Editing a variant price never rewrites history.
- **Cancellation restores stock.** Cancel isn't a status flip — it adds quantities back and writes `Cancellation` movements, so the audit trail balances.
- **Auto-provisioning.** Creating a variant creates zero-quantity Shop + Warehouse rows. Inventory slots always exist; stock operations never 404 on missing rows.
- **Atomic sale creation.** One `SaveChangesAsync` for sale + items + movements + deductions (navigation properties, so EF fixes up temp keys). The earlier two-save version could leave half-written sales.
- **Last-Owner protection.** Deactivating the final active Owner is rejected — you can't lock yourself out of your own shop.

---

## 6. Reporting Design

Five reports, one philosophy: **cancelled sales are events, not revenue.** Daily/period/top-selling filter `Status == Active` for money math while still counting cancellations for oversight. Valuation prefers `CostPrice`, falls back to `SellingPrice` (margin vs shelf-value views). Every report page exports CSV and prints — because the actual consumer is an owner reviewing paper at closing time.

---

## 7. Testing Strategy

99 xUnit tests (xUnit + Moq + FluentAssertions + EF Core InMemory via a shared `MockDbContext` factory):

- **Service tests** — including workflow tests: sale→cancel restores stock, transfer writes paired movements, force-negative warns, response items populated (regression for a real empty-items bug).
- **Validator tests** — all 14 request shapes, including the Role allow-list added with role editing.
- **Common tests** — calculator math, stock thresholds.
- **Security tests** — invoice HTML-encoding against stored XSS (`<script>` → `&lt;script&gt;`), deactivated-login rejection.

Plus a 28-step live smoke suite (login → products → inventory math → sale → invoice → cancel → users lifecycle → all reports → guards) executed against production SQL Server before release: 28/28.

---

## 8. Deployment

- **API + SQL Server:** MonsterASP.NET via GitHub Actions — test gate → `dotnet publish` (framework-dependent `win-x86`) → `msdeploy` sync driven by a single `MONSTER_PUBLISH_PROFILE` secret. Migrations + seed run on startup, so first request provisions the database.
- **Frontend:** Vercel auto-deploy from GitHub, `VITE_API_URL` env pointing at the API, SPA rewrite fallback.
- **CORS:** explicit origin policy from `Frontend:Urls` config (the #1 production incident during rollout was a missing origin — diagnosed via preflight probing, fixed as a setting, no code change).

---

## 9. Engineering Decisions

| Decision | Rationale |
|---|---|
| Decimal quantities, not int | Paint sells in fractional liters; int would corrupt stock math |
| Dual-location over single stock column | The Shop/Warehouse split is the domain's central fact; retrofitting it later would rewrite every sale path |
| Server-rendered HTML invoices, browser print | Zero PDF dependencies; iframe + `sandbox` + print CSS does the job |
| Soft-toggles over deletes | Products, variants, users keep history; reports stay consistent |
| Monolith, not microservices | Single team, single deployable, shared transactions (sale atomicity demands it) |
| Tutoring-style 1-secret deploy | One paste, zero field-mapping errors — chosen over HotelManager's 4-secret style after comparing both |

---

## 10. Lessons Learned

1. **Generic error toasts hide everything.** The login page showed "Invalid credentials" for wrong passwords, CORS blocks, and wrong URLs alike — every production issue wore the same mask. Distinguish connectivity from credential failures in UI error handling.
2. **Your lockfile is platform-specific until proven otherwise.** A phantom `@emnapi` gap passed on Windows and failed on Linux CI across two npm majors; the fix was updating to dependency versions with correct upstream metadata (proven by a sibling project's green build), not hacking the lockfile.
3. **Test the response, not just the write.** The empty-sale-items bug passed all write-side assertions — only a response-shape test caught it.
4. **Seed data is a contract.** Smoke tests, demos, and onboarding all assume `admin/admin123` + Walk-in customer + GLC catalog. Treat seed changes like migrations.
5. **Deploy the docs with the code.** This file exists because the "why" behind force-negative inventory and shop-only stock evaporates within weeks — capture it while it's fresh.
