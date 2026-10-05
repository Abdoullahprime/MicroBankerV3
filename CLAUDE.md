# CLAUDE.md — BaseTemplate

## Purpose

A **starter template** for new .NET web applications. It is meant to be **copied and pasted** into a new project, then renamed. It ships the generic building blocks every business app needs and **no domain-specific code**:

- Authentication (cookie, BCrypt) with forced first-login password change
- Users, Roles and a **permission matrix that is actually enforced** (`[RequirePermission]`)
- Organization → Branch → Department → Unit, plus Positions and Banks
- Work Calendar and Public Holidays
- Email (SMTP) configuration, key/value System Settings
- In-app notifications and Web Push (PWA)
- Falcon v3.18.0 (Bootstrap 5) admin UI with a permission-aware sidebar

**Stack:** .NET 10, EF Core 10 (SQL Server), Razor MVC, Clean Architecture layering. Origin: stripped from the Matrix HRIS; all HR code (employees, leave, payroll, loans, attendance, appraisal, recruitment, portal) was intentionally removed.

---

## Solution structure

```
BaseTemplate.slnx
.config/dotnet-tools.json            ← dotnet-ef 10.0.5 (run `dotnet tool restore`)
src/
  BaseTemplate.Domain/               ← Entities + permission constants (no dependencies)
    Entities/{Auth,Organization,Calendar,Notifications,Settings}, BaseEntity.cs
    Security/Modules.cs              ← Modules, PermissionActions, SystemRoles constants
  BaseTemplate.Application/          ← Empty on purpose (use-cases/DTOs when needed); references Domain only
  BaseTemplate.Infrastructure/       ← AppDbContext, DbSeeder, Migrations, Email/Notification services
  BaseTemplate.Web/                  ← Controllers, Views, Models, Services, Security, wwwroot
tests/
  BaseTemplate.Tests/                ← xUnit (PermissionSet tests)
```

Dependencies: `Web → Infrastructure → Application → Domain` (Infrastructure also → Domain).

---

## Run it

| | |
|---|---|
| HTTPS / HTTP | https://localhost:7226 / http://localhost:5279 |
| Connection string | `ConnectionStrings:DefaultConnection` (default `Server=.;Database=BaseTemplate;Trusted_Connection=True`) |
| First login | `Seed:AdminUsername` / `Seed:AdminPassword` from appsettings (default `admin` / `Admin@1234`) — **forced to change on first sign-in** |

On startup `Program.cs` calls `DbSeeder.SeedAsync`, which **applies migrations** and seeds: placeholder Organization, permission modules + actions, roles (Super Administrator, Administrator, User) and the admin user. Everything is idempotent.

```bash
dotnet run --project src/BaseTemplate.Web --launch-profile https
dotnet test BaseTemplate.slnx
```

### Migrations

```bash
dotnet ef migrations add <Name> --project src/BaseTemplate.Infrastructure --startup-project src/BaseTemplate.Web
dotnet ef database update      --project src/BaseTemplate.Infrastructure --startup-project src/BaseTemplate.Web
```

The repo ships one clean `InitialCreate` migration (18 tables). Because migrations are applied at startup, new migrations take effect on next run.

---

## Starting a new project from this template

1. Copy the folder; delete `.git` and `bin/obj`.
2. Global find-and-replace `BaseTemplate` → `YourApp` in file **contents**, then rename the `BaseTemplate.*` folders/files/`.slnx`.
3. `appsettings.json`: set `DefaultConnection`, **generate a new `EncryptionKey`**, change `Seed:*` credentials.
4. `Program.cs`: rename cookie names (`.BaseTemplate.Auth`, `.BaseTemplate.Antiforgery`) — cookies are shared across ports on the same host.
5. `_Layout.cshtml`: change `appName` and the footer company name; replace `wwwroot/assets/img/logo.png`, favicons, `manifest.json`, `offline.html`.
6. Optionally delete the `Migrations` folder and regenerate `InitialCreate` after adding your first domain entities.

---

## Security model (important)

- **Cookie auth**, 8h sliding. Claims: `userId`, `companyId` (= Organization id), `fullName`, `initials`, `role`, `roleCode`, `roleId`, `mustChangePassword`.
- `BaseController` is `[Authorize]`, exposes `GetCompanyId()/GetUserId()/GetFullName()`, and **redirects users flagged `MustChangePassword` to `/Auth/ChangePassword`** until they change it (they are then signed out and must log in again).
- **Permissions are enforced** via `[RequirePermission(Modules.X)]` on a controller (or action). The required action is derived from the action name:

  | Action name starts with | Required action |
  |---|---|
  | `Create`, `Add` | CREATE |
  | `Edit`, `Update`, `Save`, `SendTest` | EDIT |
  | `Delete`, `Remove` | DELETE |
  | `Approve` / `Export` | APPROVE / EXPORT |
  | anything else | VIEW |

  Override per action: `[RequirePermission(Modules.Users, PermissionActions.Export)]` (action-level wins over class-level). Denied → redirect to `/Auth/Unauthorised`.
- `PermissionService` (scoped, cached per request) unions permissions across **all** of a user's roles. The role with code `SUPER_ADMIN` bypasses every check.
- The sidebar hides links the user cannot VIEW (`_NavLink` partial + `PermissionSet.CanAny`). **Hiding is cosmetic — the attribute is the real gate.**
- Controllers that intentionally skip the attribute: `Auth`, `Account` (own signature), `Notifications`, `Home`.

### Adding a new secured feature

1. Add a constant to `Domain/Security/Modules.cs`.
2. Add a row to `ModuleSeed` in `DbSeeder` (icon, group, sort). It is created on next startup; SuperAdmin gets all actions automatically.
3. Put `[RequirePermission(Modules.YourModule)]` on the controller.
4. Add `<partial name="_NavLink" model='new NavLinkModel(Modules.YourModule, "Ctrl", "Index", "fas fa-icon", "Text", "PageKey")' />` in `_Layout.cshtml` (and wrap in a group `@if (perms.CanAny(...))` if grouped).
5. Set `ViewData["ActivePage"]` (= the `PageKey`) and `ViewData["ActiveGroup"]` in the controller's `OnActionExecuting`.
6. Grant the module to roles through **Permissions** in the UI.

---

## Domain

### BaseEntity (`Domain/Entities/BaseEntity.cs`)
`Id` (Guid), `CreatedAt`, `UpdatedAt?`, `IsDeleted`, protected `SetUpdatedAt()` / `SetDeleted()`.

> `SetUpdatedAt()` / `SetDeleted()` are **protected** — only callable inside the entity. Controllers must call the entity's public `Update(...)` / `SoftDelete()`, never set properties.

### Entities (namespaces — no flat `Entities` namespace)

| Namespace | Entities |
|---|---|
| `Domain.Entities.Auth` | AppUser, Role, UserRole, AppModule, AppAction, RolePermission |
| `Domain.Entities.Organization` | Organization, Branch, Department, Unit, Position, Bank, EmailConfiguration |
| `Domain.Entities.Calendar` | WorkCalendar, PublicHoliday |
| `Domain.Entities.Notifications` | AppNotification, PushSubscription |
| `Domain.Entities.Settings` | SystemSetting |

- **`Unit` clashes with other `Unit` types** — use the alias `using OrgUnit = BaseTemplate.Domain.Entities.Organization.Unit;`.
- Hierarchy: `Organization (single, one row) → Branch → Department → Unit`. `Position` and `Bank` hang off the Organization. Single-organization model: controllers resolve it with `_db.Organizations.Where(o => !o.IsDeleted).FirstOrDefaultAsync()`; `OrganizationId` columns exist so multi-tenancy can be added later.
- `EmailConfiguration` has one SMTP config per `EmailChannel` (`Core`, `Portal`, `External`); channels without config fall back to Core. `NotificationAudience` is `Admin` / `Portal` (the second audience is for a future self-service app).
- `SystemSetting` is a key/value table. Keys prefixed `WebPush.` hold auto-generated VAPID keys and are hidden from the Settings UI.

---

## Patterns

### DDD encapsulation
```csharp
// ✅ entity.Update(...); entity.SoftDelete();
// ❌ entity.Name = model.Name; entity.IsDeleted = true;
```

### Soft delete
Never hard-delete. Always filter with `.Where(e => !e.IsDeleted)`.

### CRUD controller shape (Branch/Department/Unit/Position/Bank)
`Index` (list) · `Create` GET/POST (`new Entity(...)` then `Update(...)`) · `Edit(id)` GET/POST · `Delete(id)` POST (`SoftDelete()`), `TempData["Success"]` on success, flat view-models in `Web/Models` (no entity types in view models).

### Encrypted route IDs
IDs in URLs are AES-encrypted (`IEncryptionService`, SHA-256-derived key, AES-256-CBC, config key `EncryptionKey`):
```csharp
_enc.Encrypt(entity.Id.ToString());           // when building links/rows
var id = Guid.Parse(_enc.Decrypt(encryptedId)); // when receiving
```

### Raw SQL in the seeder
`DbSeeder` uses `db.Database.ExecuteSqlAsync($"...{value}...")` — every `{hole}` is auto-parameterised; **never add quotes around holes**.

### UI conventions
- Falcon theme under `wwwroot/assets` + `wwwroot/vendors`; icons are Font Awesome (`fas fa-*`).
- `ViewData["ActivePage"]`/`["ActiveGroup"]` drive the sidebar highlight; `TempData["Success"|"Error"|"Info"]` are shown by `_Alerts`.
- Group keys currently used: `Org` (Setup), `Calendar`, `Administration`.

---

## Web layer map

| Controller | Module (permission) | Notes |
|---|---|---|
| Auth | — | Login, ChangePassword, Unauthorised, Logout |
| Account | — | Own signature upload |
| Home | — | Dashboard (counts + recent sign-ins) |
| Users / Roles / Permissions | USERS / ROLES / PERMISSIONS | Matrix UI saves `RolePermission` rows |
| Organization / Branch / Department / Unit / Position / Bank | matching module | CRUD, soft delete |
| Calendar / PublicHoliday | WORK_CALENDAR / PUBLIC_HOLIDAYS | |
| EmailConfig | EMAIL_CONFIG | Per-channel SMTP + test email |
| SystemSettings | SYSTEM_SETTINGS | Generic key/value editor |
| Notifications | — | Feed, mark-read, push subscribe |

Services: `AesEncryptionService`, `PermissionService` (Web); `EmailService`, `NotificationService`, `DbSeeder` (Infrastructure). View component: `OrgLogo` (cached in `IMemoryCache`; clear `OrgLogoViewComponent.CacheKey` when the logo changes).

---

## Known gaps / ideas (not implemented)

- No login throttling/lockout and no password-reset flow; `Logout` is a GET.
- No audit log (who changed what/when).
- Single-organization only; no tenant query filters.
- No API/JWT surface (cookie auth only); `Application` layer is an empty shell.
- Passwords: BCrypt; minimum length 8 is the only rule.
- `EmailChannel.Portal/External` and `NotificationAudience.Portal` are placeholders for a second app.

---

## Error patterns seen in this codebase's history

- **CS0101 / CS0229** — duplicate class definitions: delete the stale file.
- **CS0234** — wrong/flat `using` for entities: use the sub-namespaces above.
- **`SetDeleted()` inaccessible** — call the entity's `SoftDelete()`.
- **`OnActionExecuting` signature** — must be `public override` in controllers.
