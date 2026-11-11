# Microbanker V3 — Replication Phase Plan

> **Purpose:** This document is the authoritative build guide for replicating the old
> VB.NET `MicroBankerAdmin` application (in `C:\Users\Prime\source\repos\Microbanker\Oldprojectfiles`)
> into the modern .NET 10 Clean Architecture V3 project
> (`C:\Users\Prime\source\repos\Microbanker V3`).
>
> Every phase is self-contained and ends in a working, committed, buildable state.
> Read **all of CLAUDE.md first** before starting any phase — it defines the mandatory
> patterns every file must follow.

---

## Project quick-reference

| Item | Value |
|------|-------|
| Solution file | `MicrobankerV3.slnx` |
| DB server | `DESKTOP-MA2BKMN` |
| DB name | `MicrobankerV3` |
| Connection string key | `MicrobankerConnection` |
| Default admin login | `admin` / `Admin@1234` |
| Seed org ID | `aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa` |

---

## Mandatory patterns (must be applied in every phase)

### Entity shape
```csharp
// src/Microbanker.Domain/Entities/<Namespace>/<EntityName>.cs
namespace Microbanker.Domain.Entities.<Namespace>;

public class MyEntity : BaseEntity
{
    public Guid OrganizationId { get; private set; }
    public string Name { get; private set; } = string.Empty;
    // ... all props private set

    private MyEntity() { }   // ← EF Core parameterless ctor (private)

    public MyEntity(Guid organizationId, string name /* required props */)
    {
        OrganizationId = organizationId;
        Name = name;
    }

    public void Update(string name /* all editable props */)
    {
        Name = name;
        SetUpdatedAt();     // ← inherited from BaseEntity (protected)
    }

    public void SoftDelete() => SetDeleted();  // ← inherited
}
```

### Controller shape
```csharp
// src/Microbanker.Web/Controllers/<Name>Controller.cs
using Microbanker.Domain.Entities.<Namespace>;  // sub-namespace, never flat
using Microbanker.Infrastructure.Persistence;
using Microbanker.Web.Services;
using Microbanker.Web.Models;

public class MyController : BaseController
{
    private readonly MicrobankerDbContext _db;
    private readonly IEncryptionService _enc;

    public MyController(MicrobankerDbContext db, IEncryptionService enc) { _db=db; _enc=enc; }

    private async Task<Organization?> GetOrgAsync() =>
        await _db.Organizations.Where(o => !o.IsDeleted).FirstOrDefaultAsync();

    public async Task<IActionResult> Index()
    {
        ViewData["ActiveGroup"] = "GroupKey";
        ViewData["ActivePage"]  = "PageKey";
        // query, map to VM, return View(vm)
    }

    // GET Create → return View(new FormModel())
    // POST Create → validate → new Entity(orgId, ...) → _db.Add → SaveChanges → TempData["Success"] → Redirect
    // GET Edit(string id) → Guid.Parse(_enc.Decrypt(id)) → FindAsync → map to FormModel → View
    // POST Edit → Decrypt → FindAsync → entity.Update(...) → SaveChanges → Redirect
    // POST Delete(string id) → Decrypt → FindAsync → entity.SoftDelete() → SaveChanges → Redirect
}
```

### ViewModel shape
```csharp
// src/Microbanker.Web/Models/<Area>ViewModels.cs
// Flat primitives only — never import Domain entities into ViewModels

public class MyEntityViewModel          // list row
{
    public string EncryptedId { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public bool IsActive { get; set; }
}

public class MyEntityFormModel          // create / edit form
{
    public string EncryptedId { get; set; } = string.Empty;
    [Required] public string Name { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
}
```

### DbContext additions (each phase adds to `MicrobankerDbContext.cs`)
```csharp
// Add DbSet<> properties in the matching region comment
public DbSet<MyEntity> MyEntities { get; set; }

// Add configuration in OnModelCreating
builder.Entity<MyEntity>(e =>
{
    e.ToTable("MyEntities");
    e.HasKey(x => x.Id);
    e.Property(x => x.Name).IsRequired().HasMaxLength(200);
    // FK: e.HasOne<Organization>().WithMany().HasForeignKey(x => x.OrganizationId).OnDelete(DeleteBehavior.Restrict);
});
```

### DbSeeder additions (each phase adds new modules)
```csharp
// Add entries to the Modules array in DbSeeder.cs
// Format: (Code, Name, Icon, Group, SortOrder)
("CUSTOMERS",   "Customers",   "fas fa-users",        "Banking",  20),
```
Group names already in use: `"Main"`, `"Setup"`, `"Administration"`.
New groups for banking: `"Banking"`, `"Lending"`, `"Teller"`, `"Reports"`.

### Migration command
```powershell
dotnet ef migrations add <MigrationName> `
  --project src/Microbanker.Infrastructure `
  --startup-project src/Microbanker.Web
```
Run from `C:\Users\Prime\source\repos\Microbanker V3`.

### Sidebar nav entry pattern
```cshtml
@* In Views/Shared/_Layout.cshtml — add inside the correct nav group *@
<li class="nav-item">
  <a class="nav-link @(ViewData["ActivePage"]?.ToString() == "PageKey" ? "active" : "")"
     asp-controller="MyController" asp-action="Index">
    <div class="d-flex align-items-center">
      <span class="nav-link-icon"><span class="fas fa-icon"></span></span>
      <span class="nav-link-text ps-1">Label</span>
    </div>
  </a>
</li>
```

### OrgUnit alias (required when Unit entity is used)
```csharp
using OrgUnit = Microbanker.Domain.Entities.Organization.Unit;
```

---

## Old application reference

| Old module folder / pages | Maps to V3 Phase |
|---------------------------|-----------------|
| `CreateOrganization`, `CreatBranch` | Already done (Phase 0 — foundation) |
| `CreateProduct`, `CreateProductDefinition`, `CreateTransactionCode`, `CreateGL`, `ChartOfAccounts_`, `CreateArea`, `CreateRegions`, `CreateDistrict`, `CreateSector`, `CreateSubSector` | Phase 1 |
| `CreateCustomer`, `EditCustomerDetails`, `ViewCustomer`, `CreateSelfReg`, `ApproveSelfReg`, `EditSelfReg`, `ViewSelfReg` | Phase 2 |
| `CreateAccount`, `CreateCurrentAccount`, `CreateFixedDepositAc`, `EditCustomerAccount`, `ViewAccount`, `ViewSavingAccount`, `BalanceEnquiry`, `MiniStatement`, `BlockAmount`, `UnBlockAmount`, `AccountApproval`, `AccountEditRequest`, `AccountMaintenance` | Phase 3 |
| `CreateLoanAccounts`, `EditLoanAccount`, `ViewLoans`, `LoanRequest`, `LoanDisbursement`, `LoanAdvRepayment`, `LoanAccountApproval`, `LoanAccountBalanceEnquiry`, `LoanAccountEditRequest` | Phase 4 |
| `CashDeposit`, `CashWithdrawal`, `RetailPosting`, `MixedPosting`, `BulkUploadPosting`, `ViewTransactions`, `WithdrawalRequest` | Phase 5 |
| `ProcessRoutineSOD/EOD/EOM/EOY`, `Reports`, `GeneralLedger`, `GLDetails`, `CashGL` | Phase 6 |

Old source location: `C:\Users\Prime\source\repos\Microbanker\Oldprojectfiles\MicroBankerAdmin\`
Old DB schema SQL: `microBankerscript.sql` (root of this repo — full table definitions)

---

---

## Phase 1 — System Parameters & Reference Data

**Goal:** Set up all lookup / reference tables the core banking domain depends on.

### New domain namespace
`Microbanker.Domain.Entities.Setup`

Create folder: `src/Microbanker.Domain/Entities/Setup/`

### Entities to create

#### 1. `Region.cs`
```
Namespace  : Microbanker.Domain.Entities.Setup
Properties : OrganizationId (Guid), Name (string), Code (string?)
Constructor: (Guid orgId, string name, string? code)
Update()   : (string name, string? code, bool isActive)
```

#### 2. `District.cs`
```
Properties : OrganizationId (Guid), RegionId (Guid), Name (string), Code (string?)
Nav        : public Region Region { get; private set; } = null!;
Constructor: (Guid orgId, Guid regionId, string name, string? code)
Update()   : (string name, string? code, bool isActive)
```

#### 3. `Area.cs`
```
Properties : OrganizationId (Guid), DistrictId (Guid), Name (string), Code (string?)
Nav        : public District District { get; private set; } = null!;
Constructor: (Guid orgId, Guid districtId, string name, string? code)
Update()   : (string name, string? code, bool isActive)
```

#### 4. `Sector.cs`
```
Properties : OrganizationId (Guid), Name (string), Code (string?)
Constructor: (Guid orgId, string name, string? code)
Update()   : (string name, string? code, bool isActive)
```

#### 5. `SubSector.cs`
```
Properties : OrganizationId (Guid), SectorId (Guid), Name (string), Code (string?)
Nav        : public Sector Sector { get; private set; } = null!;
Constructor: (Guid orgId, Guid sectorId, string name, string? code)
Update()   : (string name, string? code, bool isActive)
```

#### 6. `ProductType.cs`  *(e.g. Savings, Current, Fixed Deposit, Loan)*
```
Properties : OrganizationId (Guid), Name (string), Code (string), Description (string?)
Constructor: (Guid orgId, string name, string code, string? description)
Update()   : (string name, string code, string? description, bool isActive)
```

#### 7. `ProductDefinition.cs`
```
Properties : OrganizationId (Guid), ProductTypeId (Guid), Name (string), Code (string),
             MinBalance (decimal), InterestRate (decimal), InterestCalcMethod (string?),
             InterestFrequency (int), DormancyDays (int), MaxDebitPerMonth (int),
             Description (string?)
Nav        : public ProductType ProductType { get; private set; } = null!;
Constructor: (Guid orgId, Guid productTypeId, string name, string code)
Update()   : all editable props + bool isActive
```

#### 8. `TransactionCode.cs`
```
Properties : OrganizationId (Guid), Code (string), Name (string), DrCr (string),
             Description (string?), RequiresApproval (bool)
Constructor: (Guid orgId, string code, string name, string drCr)
Update()   : all props + bool isActive
DrCr values: "DR" or "CR"
```

#### 9. `GLAccount.cs`  *(Chart of Accounts)*
```
Properties : OrganizationId (Guid), AccountNumber (string), AccountName (string),
             AccountType (string), ParentId (Guid?), Level (int),
             IsControlAccount (bool), IsActive (bool), Description (string?)
Nav        : public GLAccount? Parent { get; private set; }
Constructor: (Guid orgId, string accountNumber, string accountName, string accountType, Guid? parentId, int level)
Update()   : all editable props
AccountType values: "ASSET", "LIABILITY", "INCOME", "EXPENSE", "EQUITY"
```

### Controllers to create (all in `src/Microbanker.Web/Controllers/`)

| Controller | Route | ActiveGroup | ActivePage |
|------------|-------|-------------|------------|
| `RegionController` | `/Region` | `"Setup"` | `"Regions"` |
| `DistrictController` | `/District` | `"Setup"` | `"Districts"` |
| `AreaController` | `/Area` | `"Setup"` | `"Areas"` |
| `SectorController` | `/Sector` | `"Setup"` | `"Sectors"` |
| `SubSectorController` | `/SubSector` | `"Setup"` | `"SubSectors"` |
| `ProductTypeController` | `/ProductType` | `"Setup"` | `"ProductTypes"` |
| `ProductDefinitionController` | `/ProductDefinition` | `"Setup"` | `"ProductDefinitions"` |
| `TransactionCodeController` | `/TransactionCode` | `"Setup"` | `"TransactionCodes"` |
| `ChartOfAccountsController` | `/ChartOfAccounts` | `"Setup"` | `"ChartOfAccounts"` |

Each controller: `Index`, `Create` (GET/POST), `Edit` (GET/POST), `Delete` (POST).
`DistrictController.Create` needs a `SelectList` of Regions.
`AreaController.Create` needs SelectLists of Regions + Districts (filtered by region).
`SubSectorController.Create` needs a SelectList of Sectors.
`ProductDefinitionController.Create` needs a SelectList of ProductTypes.
`ChartOfAccountsController.Create` needs a SelectList of parent GL accounts (hierarchical).

### ViewModels file
Create: `src/Microbanker.Web/Models/SetupViewModels.cs`
One `ViewModel` (list row) and one `FormModel` (create/edit) per entity.

### DbContext changes
File: `src/Microbanker.Infrastructure/Persistence/MicrobankerDbContext.cs`

Add after the Organisation structure region:
```csharp
// ── Setup / Reference Data ─────────────────────────────────────────────
public DbSet<Region>             Regions             { get; set; }
public DbSet<District>           Districts           { get; set; }
public DbSet<Area>               Areas               { get; set; }
public DbSet<Sector>             Sectors             { get; set; }
public DbSet<SubSector>          SubSectors          { get; set; }
public DbSet<ProductType>        ProductTypes        { get; set; }
public DbSet<ProductDefinition>  ProductDefinitions  { get; set; }
public DbSet<TransactionCode>    TransactionCodes    { get; set; }
public DbSet<GLAccount>          GLAccounts          { get; set; }
```

Add using: `using Microbanker.Domain.Entities.Setup;`

Configure all 9 entities in `OnModelCreating`.
Key FK constraints:
- District → Region (Restrict)
- Area → District (Restrict)
- SubSector → Sector (Restrict)
- ProductDefinition → ProductType (Restrict)
- GLAccount → GLAccount (self-ref, ParentId nullable, Restrict or NoAction)
- All → Organization (Restrict)

### Migration
```powershell
dotnet ef migrations add Phase1_Setup `
  --project src/Microbanker.Infrastructure `
  --startup-project src/Microbanker.Web
```

### DbSeeder additions
File: `src/Microbanker.Infrastructure/Persistence/DbSeeder.cs`

Add to `Modules` array:
```csharp
("REGIONS",          "Regions",             "fas fa-map-marker-alt", "Setup",    15),
("DISTRICTS",        "Districts",           "fas fa-map",            "Setup",    16),
("AREAS",            "Areas",               "fas fa-map-pin",        "Setup",    17),
("SECTORS",          "Sectors",             "fas fa-industry",       "Setup",    18),
("SUBSECTORS",       "Sub-Sectors",         "fas fa-th",             "Setup",    19),
("PRODUCT_TYPES",    "Product Types",       "fas fa-box",            "Setup",    20),
("PRODUCT_DEFS",     "Product Definitions", "fas fa-cogs",           "Setup",    21),
("TRANS_CODES",      "Transaction Codes",   "fas fa-tags",           "Setup",    22),
("CHART_ACCOUNTS",   "Chart of Accounts",   "fas fa-chart-bar",      "Setup",    23),
```

### Sidebar additions
File: `src/Microbanker.Web/Views/Shared/_Layout.cshtml`

Add a new collapsible nav group **"Setup"** (after the existing Maintenance group):
```
Setup (collapsible, ActiveGroup = "Setup")
  Regions           → Region/Index
  Districts         → District/Index
  Areas             → Area/Index
  Sectors           → Sector/Index
  Sub-Sectors       → SubSector/Index
  Product Types     → ProductType/Index
  Product Defs      → ProductDefinition/Index
  Transaction Codes → TransactionCode/Index
  Chart of Accounts → ChartOfAccounts/Index
```

### Views folder structure
```
src/Microbanker.Web/Views/
  Region/           Index.cshtml, Create.cshtml, Edit.cshtml
  District/         Index.cshtml, Create.cshtml, Edit.cshtml
  Area/             Index.cshtml, Create.cshtml, Edit.cshtml
  Sector/           Index.cshtml, Create.cshtml, Edit.cshtml
  SubSector/        Index.cshtml, Create.cshtml, Edit.cshtml
  ProductType/      Index.cshtml, Create.cshtml, Edit.cshtml
  ProductDefinition/Index.cshtml, Create.cshtml, Edit.cshtml
  TransactionCode/  Index.cshtml, Create.cshtml, Edit.cshtml
  ChartOfAccounts/  Index.cshtml, Create.cshtml, Edit.cshtml
```
Follow existing view patterns from `Views/Bank/` exactly (table list, form card, Bootstrap 5).

### Done criteria
- `dotnet build` passes with zero errors
- All 9 Setup CRUD pages load and save records
- New modules visible in Permissions matrix
- Migration applied cleanly

---

---

## Phase 2 — Customer Management (Members / KYC)

**Goal:** Replicate CreateCustomer, ViewCustomer, EditCustomerDetails, CreateSelfReg, ApproveSelfReg flows.

### New domain namespace
`Microbanker.Domain.Entities.Customers`

### Entities to create

#### `Customer.cs`
```
Properties:
  OrganizationId (Guid)
  BranchId (Guid)
  CustomerNo (string)          ← system-generated, format: CUST-{year}-{seq:D6}
  CustomerType (string)        ← "INDIVIDUAL", "BUSINESS", "GROUP"
  Status (string)              ← "PENDING", "ACTIVE", "SUSPENDED", "CLOSED"

  -- Individual --
  Prefix (string?)             ← Mr, Mrs, Ms, Dr
  FirstName (string)
  MiddleName (string?)
  LastName (string)
  DateOfBirth (DateOnly?)
  Sex (string?)                ← "M", "F"
  MaritalStatus (string?)
  SpouseName (string?)
  Dependants (int)
  Nationality (string?)
  ResidenceStatus (string?)    ← "RESIDENT", "NON_RESIDENT"
  Language (string?)
  Occupation (string?)
  EmployerName (string?)
  NationalId (string?)
  PassportNo (string?)

  -- Business --
  BusinessName (string?)
  BusinessRegDate (DateOnly?)
  BusinessRegNo (string?)
  BusinessTelephone (string?)
  NatureOfBusiness (string?)
  DirectorName (string?)

  -- Group --
  GroupName (string?)
  NoOfMembers (int)
  NoOfMale (int)
  NoOfFemale (int)

  -- Contact (all types) --
  Telephone (string?)
  MobileNo (string?)
  Email (string?)
  Address1 (string?)
  Address2 (string?)
  Address3 (string?)
  RegionId (Guid?)
  DistrictId (Guid?)
  AreaId (Guid?)
  SectorId (Guid?)
  SubSectorId (Guid?)
  TinNo (string?)
  RelationshipOfficer (string?)

  -- Approval --
  IsApproved (bool)            ← default false
  ApprovedBy (Guid?)
  ApprovedAt (DateTime?)
  ApprovalRemarks (string?)

Nav: Branch, Region?, District?, Area?, Sector?, SubSector?

Constructor: (Guid orgId, Guid branchId, string customerNo, string customerType, string firstName, string lastName)
Update(): all editable props
Approve(Guid approvedBy, string? remarks): sets IsApproved=true, Status="ACTIVE"
Suspend(): Status="SUSPENDED"
SoftDelete(): SetDeleted()
```

#### `SelfRegistration.cs`
```
Properties:
  OrganizationId (Guid)
  BranchId (Guid)
  CustomerType (string)
  Status (string)       ← "PENDING", "APPROVED", "REJECTED"
  -- same personal/business/contact fields as Customer --
  RejectionReason (string?)

Constructor: (Guid orgId, Guid branchId, string customerType, ...)
Approve(): Status = "APPROVED"
Reject(string reason): Status = "REJECTED", RejectionReason = reason
SoftDelete(): SetDeleted()
```

### Controllers

| Controller | Route | ActiveGroup | ActivePage |
|------------|-------|-------------|------------|
| `CustomerController` | `/Customer` | `"Banking"` | `"Customers"` |
| `SelfRegistrationController` | `/SelfRegistration` | `"Banking"` | `"SelfReg"` |

`CustomerController` actions: `Index`, `Create` (GET/POST), `Edit` (GET/POST), `View` (GET), `Delete` (POST), `Approve` (POST).
`SelfRegistrationController` actions: `Index`, `Create` (GET/POST), `Edit` (GET/POST), `View` (GET), `Approve` (POST), `Reject` (POST), `Delete` (POST).

### Customer number generation
Add a service or static helper:
```csharp
// In CustomerController or a CustomerService
private async Task<string> GenerateCustomerNoAsync(Guid orgId)
{
    var year  = DateTime.UtcNow.Year;
    var count = await _db.Customers
        .CountAsync(c => c.OrganizationId == orgId && !c.IsDeleted);
    return $"CUST-{year}-{(count + 1):D6}";
}
```

### ViewModels file
`src/Microbanker.Web/Models/CustomerViewModels.cs`

Include: `CustomerListViewModel`, `CustomerFormModel`, `CustomerDetailViewModel`,
`SelfRegListViewModel`, `SelfRegFormModel`.

The form model must have a `CustomerType` property and use JavaScript on the view to show/hide
individual vs business vs group fields.

### DbContext additions
```csharp
public DbSet<Customer>          Customers          { get; set; }
public DbSet<SelfRegistration>  SelfRegistrations  { get; set; }
```
Add using: `using Microbanker.Domain.Entities.Customers;`

FK constraints: Customer → Branch (Restrict), Region/District/Area/Sector/SubSector (all nullable, NoAction).

### Migration
```powershell
dotnet ef migrations add Phase2_Customers `
  --project src/Microbanker.Infrastructure `
  --startup-project src/Microbanker.Web
```

### DbSeeder additions
```csharp
("CUSTOMERS",    "Customers",         "fas fa-users",        "Banking", 30),
("SELF_REG",     "Self Registration", "fas fa-user-plus",    "Banking", 31),
```

### Sidebar additions
Add a new collapsible nav group **"Banking"**:
```
Banking (collapsible, ActiveGroup = "Banking")
  Customers        → Customer/Index
  Self Registration→ SelfRegistration/Index
```

---

---

## Phase 3 — Account Management (Savings / Current / Fixed Deposit)

**Goal:** Replicate CreateAccount, EditCustomerAccount, ViewAccount, AccountApproval, BalanceEnquiry, BlockAmount flows.

### New domain namespace
`Microbanker.Domain.Entities.Accounts`

### Entities to create

#### `MemberAccount.cs`  *(polymorphic — discriminator = AccountType)*
```
Properties:
  OrganizationId (Guid)
  BranchId (Guid)
  CustomerId (Guid)
  ProductDefinitionId (Guid)
  AccountNo (string)           ← system-generated, format: {BranchCode}-{TypePrefix}-{seq:D7}
  AccountName (string)
  AccountType (string)         ← "SAVINGS", "CURRENT", "FIXED_DEPOSIT"
  Currency (string)            ← default "GMD"
  Status (string)              ← "PENDING", "ACTIVE", "DORMANT", "FROZEN", "CLOSED"
  OpeningBalance (decimal)
  BookBalance (decimal)
  AvailableBalance (decimal)
  BlockedAmount (decimal)
  UnauthorizedDR (decimal)
  UnauthorizedCR (decimal)
  MonthlyDRTurnover (decimal)
  MonthlyCRTurnover (decimal)
  InterestAccruedDR (decimal)
  InterestAccruedCR (decimal)
  LastDRDate (DateTime?)
  LastCRDate (DateTime?)
  LastDRActivityDate (DateTime?)
  LastCRActivityDate (DateTime?)
  AtmAccountNo (string?)
  AtmDailyLimit (decimal)
  SectorId (Guid?)
  SubSectorId (Guid?)
  GLAssetControl (string?)
  GLLiabilityControl (string?)
  GLIncomeAccount (string?)
  GLExpenseAccount (string?)
  GLChargeAccount (string?)
  GLFeeAccount (string?)
  IsApproved (bool)
  ApprovedBy (Guid?)
  ApprovedAt (DateTime?)
  EditRequestPending (bool)
  EditRequestReason (string?)
  OpenDate (DateOnly)

Nav: Customer, Branch, ProductDefinition

Constructor: (Guid orgId, Guid branchId, Guid customerId, Guid productDefId, string accountNo, string accountName, string accountType)
Update(): all editable props
Approve(Guid approvedBy): IsApproved = true, Status = "ACTIVE"
BlockAmount(decimal amount): BlockedAmount += amount; AvailableBalance -= amount
UnblockAmount(decimal amount): BlockedAmount -= amount; AvailableBalance += amount
SoftDelete()
```

#### `AccountEditRequest.cs`
```
Properties: AccountId (Guid), RequestedBy (Guid), RequestReason (string),
            Status (string), ReviewedBy (Guid?), ReviewedAt (DateTime?), ReviewRemarks (string?)
```

### Controllers

| Controller | Route | ActiveGroup | ActivePage |
|------------|-------|-------------|------------|
| `AccountController` | `/Account` | `"Banking"` | `"Accounts"` |

Actions: `Index`, `Create` (GET/POST — with CustomerNo lookup), `Edit` (GET/POST),
`View` (GET — full detail), `Approve` (POST), `Delete` (POST),
`BalanceEnquiry` (GET — search by AccountNo), `MiniStatement` (GET),
`BlockAmount` (GET/POST), `UnblockAmount` (POST).

### DbContext additions
```csharp
public DbSet<MemberAccount>       MemberAccounts      { get; set; }
public DbSet<AccountEditRequest>  AccountEditRequests { get; set; }
```

### Migration
```powershell
dotnet ef migrations add Phase3_Accounts `
  --project src/Microbanker.Infrastructure `
  --startup-project src/Microbanker.Web
```

### DbSeeder additions
```csharp
("ACCOUNTS",    "Accounts",           "fas fa-piggy-bank",   "Banking", 32),
("BAL_ENQUIRY", "Balance Enquiry",    "fas fa-search-dollar","Banking", 33),
```

### Sidebar additions (under Banking group)
```
  Accounts         → Account/Index
  Balance Enquiry  → Account/BalanceEnquiry
```

---

---

## Phase 4 — Loan Management

**Goal:** Replicate CreateLoanAccounts, LoanRequest, LoanDisbursement, LoanAdvRepayment, LoanAccountApproval.

### New domain namespace
`Microbanker.Domain.Entities.Loans`

### Entities to create

#### `LoanAccount.cs`
```
Properties:
  OrganizationId (Guid)
  BranchId (Guid)
  CustomerId (Guid)
  ProductDefinitionId (Guid)
  LoanNo (string)              ← format: LN-{year}-{seq:D6}
  LoanName (string)
  Currency (string)
  Status (string)              ← "PENDING","APPROVED","ACTIVE","CLOSED","WRITTEN_OFF"
  LoanAmount (decimal)
  OutstandingBalance (decimal)
  DisbursedAmount (decimal)
  InterestRate (decimal)
  InterestCalcMethod (string)  ← "FLAT", "REDUCING"
  GracePeriod (int)            ← months
  Tenure (int)                 ← months
  NoOfInstallments (int)
  RepaymentFrequency (string)  ← "DAILY","WEEKLY","MONTHLY","QUARTERLY"
  RepaymentMode (string)       ← "CASH","DEDUCTION","STANDING_ORDER"
  LoanStartDate (DateOnly?)
  LoanEndDate (DateOnly?)
  FirstEMIDate (DateOnly?)
  Charges (decimal)
  Collateral (string?)
  GuarantorsAccountNo (string?)
  SettlementAccountNo (string?)
  SkipInstallments (int)
  IsApproved (bool)
  ApprovedBy (Guid?)
  ApprovedAt (DateTime?)
  IsDisbursed (bool)
  DisbursedAt (DateTime?)
  DisbursedBy (Guid?)
  SectorId (Guid?)
  SubSectorId (Guid?)

Nav: Customer, Branch, ProductDefinition

Constructor: (Guid orgId, Guid branchId, Guid customerId, Guid productDefId, string loanNo, decimal loanAmount)
Update(): editable props
Approve(Guid approvedBy)
Disburse(Guid disbursedBy): IsDisbursed=true, Status="ACTIVE", DisbursedAt=DateTime.UtcNow
SoftDelete()
```

#### `LoanRepayment.cs`
```
Properties: LoanAccountId (Guid), InstallmentNo (int), DueDate (DateOnly),
            PrincipalDue (decimal), InterestDue (decimal), ChargesDue (decimal),
            PrincipalPaid (decimal), InterestPaid (decimal), ChargesPaid (decimal),
            PaidAt (DateTime?), Status (string), PostedBy (Guid?)
```

#### `LoanRequest.cs`  *(pre-approval request from member)*
```
Properties: OrganizationId, BranchId, CustomerId, ProductDefinitionId,
            RequestedAmount (decimal), Purpose (string?), Status (string),
            ReviewedBy (Guid?), ReviewedAt (DateTime?), ReviewRemarks (string?)
```

### Controllers

| Controller | Route | ActiveGroup | ActivePage |
|------------|-------|-------------|------------|
| `LoanController` | `/Loan` | `"Lending"` | `"Loans"` |

Actions: `Index`, `Create` (GET/POST), `Edit` (GET/POST), `View` (GET),
`Approve` (POST), `Disburse` (GET/POST), `Repayment` (GET/POST),
`BalanceEnquiry` (GET), `Delete` (POST).

### DbContext additions
```csharp
// ── Loans ──────────────────────────────────────────────────────────────
public DbSet<LoanAccount>   LoanAccounts  { get; set; }
public DbSet<LoanRepayment> LoanRepayments { get; set; }
public DbSet<LoanRequest>   LoanRequests   { get; set; }
```

### Migration
```powershell
dotnet ef migrations add Phase4_Loans `
  --project src/Microbanker.Infrastructure `
  --startup-project src/Microbanker.Web
```

### DbSeeder additions
```csharp
("LOANS",       "Loans",           "fas fa-hand-holding-usd", "Lending", 40),
("LOAN_REQ",    "Loan Requests",   "fas fa-file-invoice-dollar","Lending",41),
```

### Sidebar additions — new group "Lending"
```
Lending (collapsible, ActiveGroup = "Lending")
  Loans            → Loan/Index
  Loan Requests    → Loan/Requests
```

---

---

## Phase 5 — Tellering & Transactions

**Goal:** Replicate CashDeposit, CashWithdrawal, RetailPosting, MixedPosting, ViewTransactions.

### New domain namespace
`Microbanker.Domain.Entities.Transactions`

### Entities to create

#### `Transaction.cs`
```
Properties:
  OrganizationId (Guid)
  BranchId (Guid)
  TransactionRef (string)      ← format: TXN-{yyyyMMdd}-{seq:D6}
  TransactionCodeId (Guid)
  AccountId (Guid)             ← MemberAccount or LoanAccount
  AccountType (string)         ← "SAVINGS","CURRENT","FIXED_DEPOSIT","LOAN"
  DrCr (string)                ← "DR" or "CR"
  Amount (decimal)
  Narration (string)
  ValueDate (DateOnly)
  PostingDate (DateTime)
  PostedBy (Guid)
  TellerRef (string?)
  BatchRef (string?)
  Status (string)              ← "PENDING","POSTED","REVERSED"
  IsReversal (bool)
  ReversalRef (string?)
  GLAccountNo (string?)        ← GL account posted against

Constructor: (Guid orgId, Guid branchId, string txnRef, Guid txnCodeId, Guid accountId, string accountType, string drCr, decimal amount, string narration, DateOnly valueDate, Guid postedBy)
Reverse(string reversalRef): Status="REVERSED", IsReversal=true
```

#### `WithdrawalRequest.cs`
```
Properties: OrganizationId, BranchId, AccountId, Amount (decimal),
            RequestedBy (Guid), Purpose (string?), Status (string),
            ApprovedBy (Guid?), ApprovedAt (DateTime?)
```

### Controllers

| Controller | Route | ActiveGroup | ActivePage |
|------------|-------|-------------|------------|
| `TellerController` | `/Teller` | `"Teller"` | `"Teller"` |
| `TransactionController` | `/Transaction` | `"Teller"` | `"Transactions"` |

`TellerController` actions: `Deposit` (GET/POST), `Withdrawal` (GET/POST),
`RetailPosting` (GET/POST), `MixedPosting` (GET/POST), `BulkUpload` (GET/POST).
`TransactionController` actions: `Index` (search/filter), `View` (GET), `Reverse` (POST).

### Posting engine
Add `src/Microbanker.Web/Services/IPostingService.cs` and `PostingService.cs`:
```csharp
public interface IPostingService
{
    Task<Transaction> PostAsync(PostingRequest request);
    Task<bool> ReverseAsync(string transactionRef, Guid reversedBy);
}
```
The service: validates account is ACTIVE, has sufficient balance for DR,
creates the `Transaction` record, updates `MemberAccount` balances (BookBalance, AvailableBalance),
creates the GL entry in `GLEntry` (Phase 6).

Register in `Program.cs`:
```csharp
builder.Services.AddScoped<IPostingService, PostingService>();
```

### DbContext additions
```csharp
public DbSet<Transaction>        Transactions        { get; set; }
public DbSet<WithdrawalRequest>  WithdrawalRequests  { get; set; }
```

### Migration
```powershell
dotnet ef migrations add Phase5_Transactions `
  --project src/Microbanker.Infrastructure `
  --startup-project src/Microbanker.Web
```

### DbSeeder additions
```csharp
("TELLER",      "Teller",          "fas fa-cash-register",  "Teller",  50),
("TRANS_VIEW",  "Transactions",    "fas fa-list-alt",        "Teller",  51),
```

### Sidebar additions — new group "Teller"
```
Teller (collapsible, ActiveGroup = "Teller")
  Deposit          → Teller/Deposit
  Withdrawal       → Teller/Withdrawal
  Retail Posting   → Teller/RetailPosting
  Mixed Posting    → Teller/MixedPosting
  Transactions     → Transaction/Index
```

---

---

## Phase 6 — General Ledger & EOD/EOM/EOY Routines

**Goal:** Full double-entry GL, chart of accounts viewer/manager, batch processing routines, basic reports.

### New domain namespace
`Microbanker.Domain.Entities.GeneralLedger`

### Entities to create

#### `GLEntry.cs`  *(journal entry line)*
```
Properties: OrganizationId (Guid), JournalRef (string), GLAccountNo (string),
            DrCr (string), Amount (decimal), Narration (string),
            ValueDate (DateOnly), PostingDate (DateTime), PostedBy (Guid),
            SourceModule (string),    ← "TELLER","LOAN","SYSTEM"
            SourceRef (string),       ← TransactionRef or LoanRepaymentId
            FiscalPeriod (string),    ← "YYYY-MM"
            Status (string)           ← "POSTED","REVERSED"
```

#### `ProcessingDate.cs`  *(current SOD/EOD state)*
```
Properties: OrganizationId (Guid), CurrentDate (DateOnly), LastSODAt (DateTime?),
            LastEODAt (DateTime?), LastEOMAt (DateTime?), LastEOYAt (DateTime?),
            EODStatus (string)   ← "OPEN","EOD_RUNNING","CLOSED"
```

#### `EODLog.cs`
```
Properties: OrganizationId (Guid), ProcessDate (DateOnly), ProcessType (string),
            StartedAt (DateTime), CompletedAt (DateTime?), Status (string),
            RunBy (Guid), Notes (string?)
```

### Controllers

| Controller | Route | ActiveGroup | ActivePage |
|------------|-------|-------------|------------|
| `GLController` | `/GL` | `"Reports"` | `"GL"` |
| `ProcessingController` | `/Processing` | `"Reports"` | `"Processing"` |
| `ReportController` | `/Report` | `"Reports"` | `"Reports"` |

`GLController`: `Index` (GL listing), `AccountStatement` (GET — by GL account + date range).
`ProcessingController`: `SOD` (GET/POST), `EOD` (GET/POST), `EOM` (GET/POST), `EOY` (GET/POST).
`ReportController`: `AccountStatement`, `MiniStatement`, `LoanSchedule`, `TrialBalance`.

### EOD routine (minimal)
```
SOD: Set ProcessingDate.EODStatus = "OPEN", record LastSODAt
EOD: 1. Run interest accrual on active accounts
     2. Check dormancy (days since last activity > DormancyDays → flag DORMANT)
     3. Advance ProcessingDate.CurrentDate by 1
     4. Set EODStatus = "CLOSED", record LastEODAt, insert EODLog
EOM: 1. Post interest (accrued → income GL)
     2. Zero monthly turnover counters on MemberAccount
EOY: 1. Close fiscal period
     2. Post retained earnings
```

### DbContext additions
```csharp
// ── General Ledger ─────────────────────────────────────────────────────
public DbSet<GLEntry>         GLEntries      { get; set; }
public DbSet<ProcessingDate>  ProcessingDates { get; set; }
public DbSet<EODLog>          EODLogs         { get; set; }
```

### Migration
```powershell
dotnet ef migrations add Phase6_GL `
  --project src/Microbanker.Infrastructure `
  --startup-project src/Microbanker.Web
```

### DbSeeder additions
```csharp
("GL",          "General Ledger",  "fas fa-book",           "Reports", 70),
("PROCESSING",  "Processing",      "fas fa-cog",            "Reports", 71),
("REPORTS",     "Reports",         "fas fa-file-alt",       "Reports", 72),
```

### Sidebar additions — new group "Reports"
```
Reports (collapsible, ActiveGroup = "Reports")
  General Ledger   → GL/Index
  Processing       → Processing/SOD
  Reports          → Report/Index
```

---

---

## Cross-cutting work (do after Phase 2, before Phase 3)

### Permission enforcement
Currently `[Authorize]` only. Build a real check:

1. Add `IPermissionService` to `src/Microbanker.Web/Services/`:
```csharp
public interface IPermissionService
{
    Task<bool> HasPermissionAsync(Guid roleId, string moduleCode, string actionCode);
}
```

2. Add `AuthorizationFilter` attribute:
```csharp
[AttributeUsage(AttributeTargets.Method)]
public class RequiresPermissionAttribute : TypeFilterAttribute
{
    public RequiresPermissionAttribute(string module, string action)
        : base(typeof(PermissionFilter))
    { Arguments = new object[] { module, action }; }
}
```

3. In `BaseController`, add `HasPermissionAsync()` helper so views can hide/show buttons.

4. Register `IPermissionService` in `Program.cs`.

### Audit log
Add `AuditLog` entity to `Microbanker.Domain.Entities`:
```
Properties: UserId (Guid), UserName (string), Action (string), EntityName (string),
            EntityId (string), OldValues (string?), NewValues (string?),
            Timestamp (DateTime), IpAddress (string?)
```
Override `SaveChangesAsync` in `MicrobankerDbContext` to auto-capture changes.

---

## File locations summary

| What | Where |
|------|-------|
| Domain entities | `src/Microbanker.Domain/Entities/<Namespace>/` |
| DbContext | `src/Microbanker.Infrastructure/Persistence/MicrobankerDbContext.cs` |
| DbSeeder | `src/Microbanker.Infrastructure/Persistence/DbSeeder.cs` |
| Migrations | `src/Microbanker.Infrastructure/Migrations/` |
| Controllers | `src/Microbanker.Web/Controllers/` |
| ViewModels | `src/Microbanker.Web/Models/` |
| Views | `src/Microbanker.Web/Views/<ControllerName>/` |
| Shared layout | `src/Microbanker.Web/Views/Shared/_Layout.cshtml` |
| Services | `src/Microbanker.Web/Services/` |
| Program.cs | `src/Microbanker.Web/Program.cs` |
| Old VB source | `C:\Users\Prime\source\repos\Microbanker\Oldprojectfiles\MicroBankerAdmin\` |
| Old DB schema | `microBankerscript.sql` (repo root) |

---

*Last updated: 2026-11-11 — covers Phases 1–6 derived from MicroBankerAdmin VB.NET source.*
