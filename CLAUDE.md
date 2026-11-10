# CLAUDE.md — Matrix Microbanker V3

## Project Overview

**Company:** Matrix Solutions Company Limited (Gambia, est. 2007) — Oracle GOLD Partner
**Role:** Abdoulie Conteh, CTO
**Product:** Matrix Microbanker V3 — microfinance / core banking system
**Architecture:** Clean Architecture, .NET 10, SQL Server, EF Core, Razor Views (MVC), Falcon v3.18.0 Bootstrap 5

> This codebase started life as an HRMIS product. All HR, payroll, leave, loans,
> employee and self-service modules were stripped out in September 2026, leaving a
> reusable foundation (auth/rights, organisation structure, UI shell, framework
> plumbing). That foundation was briefly branded "AccountingPlusV3" and was renamed
> again to **Microbanker V3** on 2026-09-08. Microbanking modules are built on top of it.

---

## Solution Structure

```
MicrobankerV3.slnx
src/
  Microbanker.Domain/          ← BaseEntity, Auth entities, Organization entities
  Microbanker.Application/     ← EMPTY. MediatR referenced, no handlers yet.
  Microbanker.Infrastructure/  ← MicrobankerDbContext, DbSeeder, Migrations
  Microbanker.Web/             ← MVC web app (Controllers, Views, Models, Services)
tests/
  Microbanker.Tests/           ← xunit, stub only
```

---

## URLs & Ports

| App | HTTPS | HTTP |
|-----|-------|------|
| Microbanker.Web | https://localhost:7290 | http://localhost:5290 |

Run it via the Browser pane (`.claude/launch.json` → profile `microbanker-web`), never `dotnet run` in Bash.

---

## Database

| Database | Server |
|----------|--------|
| `MicrobankerV3` | `DESKTOP-MA2BKMN` |

Connection string key: **`MicrobankerConnection`** in `appsettings.json`.

Schema is created by a single migration, `InitialCreate` (11 tables). `DbSeeder.SeedAsync`
runs `MigrateAsync()` on every startup, so the DB self-creates on first run.

### Seeded data
- Organisation `aaaaaaaa-…` — placeholder "My Organisation" / code `ORG`
- Roles: `bbbbbbbb-…` SUPER_ADMIN (system), `cccccccc-…` ADMIN
- Admin user `ffffffff-…` — **admin / Admin@1234**, `MustChangePassword = true`
- 9 modules × 6 actions = 54 SUPER_ADMIN role permissions

### Regenerate schema
```bash
dotnet ef migrations add <Name> --project src/Microbanker.Infrastructure --startup-project src/Microbanker.Web
```

---

## Domain Entities

### CRITICAL: always use sub-namespaces — flat `Microbanker.Domain.Entities` holds only `BaseEntity`

| Namespace | Entities |
|-----------|----------|
| `Microbanker.Domain.Entities.Auth` | AppUser, Role, UserRole, AppModule, AppAction, RolePermission |
| `Microbanker.Domain.Entities.Organization` | Organization, Branch, Department, Unit, Bank |

### OrgUnit alias (REQUIRED everywhere `Unit` is used)
```csharp
using OrgUnit = Microbanker.Domain.Entities.Organization.Unit;
```
Avoids ambiguity with `MediatR.Unit`. (This was `HrisUnit` before the rename.)

### BaseEntity (`Microbanker.Domain/Entities/BaseEntity.cs`)
```csharp
public abstract class BaseEntity
{
    public Guid Id { get; protected set; } = Guid.NewGuid();
    public DateTime CreatedAt { get; protected set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; protected set; }
    public bool IsDeleted { get; protected set; } = false;

    protected void SetUpdatedAt();
    protected void SetDeleted();
}
```

> ⚠️ `SetUpdatedAt()` / `SetDeleted()` are **protected**. Controllers MUST call the
> entity's `Update()` / `SoftDelete()` methods, never set properties directly.

### Organisation hierarchy
```
Organization (one record per installation)
  └─ Branch
       └─ Department
            └─ Unit
Bank (flat, org-scoped)
```

---

## Auth & Rights

### Model
`AppModule` (securable feature area) × `AppAction` (VIEW/CREATE/EDIT/DELETE/APPROVE/EXPORT)
→ granted to a `Role` via `RolePermission`. Users get roles through `UserRole`.

### ⚠️ Permissions are NOT enforced at runtime
The schema, the seeder and the `/Permissions` editor all work, but **nothing checks them**.
`BaseController` carries only `[Authorize]`; there is no authorization filter, policy
handler or `HasPermission()` helper. Any authenticated user can reach any action.
**Building that enforcement layer is outstanding work** — see Next Steps.

### Login flow
Local cookie auth. `AuthController.Login` verifies BCrypt, loads the user's *first*
`UserRole`, and issues claims: `userId`, `companyId` (= `Organization.Id`), `fullName`,
`initials`, `role`, `roleCode`, `roleId`, plus `ClaimTypes.Name/Email/Role`.
`MustChangePassword` forces `/Auth/ChangePassword` on first login.

> There is no SSO. The old MOS portal, JWT `AppLaunchKey` handshake and
> `/Auth/Entry?t=` endpoint are gone — don't reintroduce references to them.

---

## Web Layer

| Controller | Route | Views |
|------------|-------|-------|
| HomeController | /Home | Dashboard, Error |
| AuthController | /Auth | Login, ChangePassword, Logout |
| OrganizationController | /Organization | Index, Create, Edit |
| BranchController | /Branch | Index, Create, Edit |
| DepartmentController | /Department | Index, Create, Edit |
| UnitController | /Unit | Index, Create, Edit |
| BankController | /Bank | Index, Create, Edit |
| UsersController | /Users | Index, Create, Edit, _UserForm |
| RolesController | /Roles | Index, Create, Edit, _RoleForm |
| PermissionsController | /Permissions | Index (role × module × action matrix) |

### ViewModels (`Models/`)
`AuthViewModels.cs`, `DashboardViewModels.cs`, `ErrorViewModel.cs`,
`OrganizationViewModels.cs` (Organization / Branch / Department / Unit / Bank).
All view models use **flat primitives only** — never import domain entities into a view model.

### Services (`Services/`)
`IEncryptionService` / `AesEncryptionService` — AES-256-CBC + SHA-256 key derivation,
Base64Url (`+→-`, `/→_`, `=→~`). Config key `EncryptionKey`. Used to encrypt route IDs.

---

## Layout (`Views/Shared/_Layout.cshtml`)

Falcon vertical sidebar. Two nav groups only:

```
Dashboard                        → Home/Index
Maintenance
  └─ Organisation (collapsible, ActiveGroup = "Org")
       Organization  → Organization/Index
       Branches      → Branch/Index
       Departments   → Department/Index
       Units         → Unit/Index
       Banks         → Bank/Index
Administration
  Users        → Users/Index
  Roles        → Roles/Index
  Permissions  → Permissions/Index
```

Each action sets `ViewData["ActivePage"]` and (for grouped pages) `ViewData["ActiveGroup"]`.
`TempData["Success"]` carries success notifications across redirects.

---

## Development Patterns

### DDD encapsulation
```csharp
// ✅ Correct
entity.Update(param1, param2, ...);
entity.SoftDelete();

// ❌ Wrong
entity.Name = model.Name;
entity.IsDeleted = true;
```

### Standard controller shape
```csharp
using Microbanker.Domain.Entities.Organization;   // sub-namespace, never flat

private async Task<Organization?> GetOrgAsync() =>
    await _db.Organizations.Where(o => !o.IsDeleted).FirstOrDefaultAsync();

// Create: constructor, then Update() for the optional fields
var entity = new Branch(org.Id, model.Name, code);
entity.Update(model.Name, code, ...);

// Edit: Update() only.  Delete: SoftDelete().
```

### Encrypted route IDs
```csharp
_enc.Encrypt(entity.Id.ToString())          // for the URL
Guid.Parse(_enc.Decrypt(id))                // from the route param
```

### Soft delete
Never hard-delete. Always filter `.Where(e => !e.IsDeleted)`.

---

## Known Error Patterns

| Error | Cause | Fix |
|-------|-------|-----|
| CS0234 — type not in namespace | `using Microbanker.Domain.Entities;` (flat) | Use the sub-namespace |
| CS0101 / CS0229 — duplicate class | Two files define the same class | Delete the stale file |
| Ambiguous `Unit` | MediatR.Unit vs Organization.Unit | Add the `OrgUnit` alias |
| `SetDeleted()` inaccessible | It's `protected` on BaseEntity | Call `SoftDelete()` |
| Build error on `Include(d => d.Branch)` | Nav property missing | Add it to the entity |

---

## Current Status

### ✅ Done
- Full rename HRIS/HRMIS → Accounting → **Microbanker / MicrobankerV3**
  (solution, project folders, csproj, namespaces, DbContext, connection string, DB name,
  encryption key, launch profile, publish profile, UI branding)
- HR, payroll, leave, loans, employee and self-service portal removed
- Auth + rights schema, users/roles/permissions CRUD
- Organisation structure CRUD (Organization, Branch, Department, Unit, Bank)
- Generic dashboard (org structure + security counts)
- Single `InitialCreate` migration, 11 tables; idempotent seeder
- Falcon theme shell, encrypted route IDs, soft delete throughout

### 📋 Next Steps
1. **Enforce permissions** — authorization filter / policy handler reading `RolePermission`,
   plus a view helper to hide unavailable actions
2. Core banking domain — clients/members, savings & deposit accounts, loan products,
   transactions/tellering, plus the GL underneath (chart of accounts, journals,
   ledgers, fiscal periods)
3. Register the new banking modules in `DbSeeder.Modules[]` so they appear in the
   permissions matrix and sidebar
4. Populate `Microbanker.Application` with MediatR handlers, or drop the project
5. Replace the stub in `tests/Microbanker.Tests`
6. Audit log
