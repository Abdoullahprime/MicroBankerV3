// src/Microbanker.Infrastructure/Persistence/MicrobankerDbContext.cs
using Microbanker.Domain.Entities.Auth;
using Microbanker.Domain.Entities.Organization;
using Microbanker.Domain.Entities.Setup;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using OrgUnit = Microbanker.Domain.Entities.Organization.Unit;

namespace Microbanker.Infrastructure.Persistence;

public class MicrobankerDbContext : DbContext
{
    public MicrobankerDbContext(DbContextOptions<MicrobankerDbContext> options) : base(options) { }

    // ── Auth & Rights ──────────────────────────────────────────────
    public DbSet<AppUser> AppUsers { get; set; }
    public DbSet<Role> Roles { get; set; }
    public DbSet<UserRole> UserRoles { get; set; }
    public DbSet<AppModule> AppModules { get; set; }
    public DbSet<AppAction> AppActions { get; set; }
    public DbSet<RolePermission> RolePermissions { get; set; }

    // ── Organisation structure ─────────────────────────────────────
    public DbSet<Organization> Organizations { get; set; }
    public DbSet<Branch> Branches { get; set; }
    public DbSet<Bank> Banks { get; set; }
    public DbSet<Department> Departments { get; set; }
    public DbSet<OrgUnit> Units { get; set; }

    // ── Setup / Reference Data ─────────────────────────────────────
    public DbSet<Region>            Regions            { get; set; }
    public DbSet<District>          Districts          { get; set; }
    public DbSet<Area>              Areas              { get; set; }
    public DbSet<Sector>            Sectors            { get; set; }
    public DbSet<SubSector>         SubSectors         { get; set; }
    public DbSet<ProductType>       ProductTypes       { get; set; }
    public DbSet<ProductDefinition> ProductDefinitions { get; set; }
    public DbSet<TransactionCode>   TransactionCodes   { get; set; }
    public DbSet<GLAccount>         GLAccounts         { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.ConfigureWarnings(w =>
            w.Ignore(RelationalEventId.PendingModelChangesWarning));
    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        // ── AppUser ────────────────────────────────────────────────
        builder.Entity<AppUser>(e =>
        {
            e.ToTable("AppUsers");
            e.HasKey(x => x.Id);
            e.Property(x => x.Username).IsRequired().HasMaxLength(100);
            e.Property(x => x.Email).IsRequired().HasMaxLength(200);
            e.Property(x => x.PasswordHash).IsRequired();
            e.Property(x => x.FirstName).IsRequired().HasMaxLength(100);
            e.Property(x => x.LastName).IsRequired().HasMaxLength(100);
            e.Property(x => x.Phone).HasMaxLength(50);
            e.HasIndex(x => x.Username).IsUnique();
            e.HasIndex(x => x.Email).IsUnique();
            e.Ignore(x => x.FullName);
            e.Ignore(x => x.Initials);
        });

        // ── Role ───────────────────────────────────────────────────
        builder.Entity<Role>(e =>
        {
            e.ToTable("Roles");
            e.HasKey(x => x.Id);
            e.Property(x => x.Name).IsRequired().HasMaxLength(100);
            e.Property(x => x.Code).IsRequired().HasMaxLength(50);
            e.Property(x => x.Description).HasMaxLength(500);
            e.HasIndex(x => new { x.OrganizationId, x.Code }).IsUnique();
        });

        // ── UserRole ───────────────────────────────────────────────
        builder.Entity<UserRole>(e =>
        {
            e.ToTable("UserRoles");
            e.HasKey(x => x.Id);
            e.HasIndex(x => new { x.UserId, x.RoleId }).IsUnique();
            e.HasOne(x => x.User).WithMany().HasForeignKey(x => x.UserId).OnDelete(DeleteBehavior.Cascade);
            e.HasOne(x => x.Role).WithMany().HasForeignKey(x => x.RoleId).OnDelete(DeleteBehavior.Cascade);
        });

        // ── AppModule ──────────────────────────────────────────────
        builder.Entity<AppModule>(e =>
        {
            e.ToTable("AppModules");
            e.HasKey(x => x.Id);
            e.Property(x => x.Name).IsRequired().HasMaxLength(100);
            e.Property(x => x.Code).IsRequired().HasMaxLength(50);
            e.Property(x => x.Description).HasMaxLength(500);
            e.Property(x => x.Icon).HasMaxLength(100);
            e.Property(x => x.Group).HasMaxLength(100);
            e.HasIndex(x => x.Code).IsUnique();
        });

        // ── AppAction ──────────────────────────────────────────────
        builder.Entity<AppAction>(e =>
        {
            e.ToTable("AppActions");
            e.HasKey(x => x.Id);
            e.Property(x => x.Name).IsRequired().HasMaxLength(100);
            e.Property(x => x.Code).IsRequired().HasMaxLength(50);
            e.HasIndex(x => new { x.AppModuleId, x.Code }).IsUnique();
            e.HasOne(x => x.AppModule).WithMany().HasForeignKey(x => x.AppModuleId).OnDelete(DeleteBehavior.Cascade);
        });

        // ── RolePermission ─────────────────────────────────────────
        builder.Entity<RolePermission>(e =>
        {
            e.ToTable("RolePermissions");
            e.HasKey(x => x.Id);
            e.HasIndex(x => new { x.RoleId, x.AppModuleId, x.AppActionId }).IsUnique();
            e.HasOne(x => x.Role).WithMany().HasForeignKey(x => x.RoleId).OnDelete(DeleteBehavior.Cascade);
            e.HasOne(x => x.AppModule).WithMany().HasForeignKey(x => x.AppModuleId).OnDelete(DeleteBehavior.NoAction);
            e.HasOne(x => x.AppAction).WithMany().HasForeignKey(x => x.AppActionId).OnDelete(DeleteBehavior.NoAction);
        });

        // ── Organization ───────────────────────────────────────────
        builder.Entity<Organization>(e =>
        {
            e.ToTable("Organizations");
            e.HasKey(x => x.Id);
            e.Property(x => x.Name).IsRequired().HasMaxLength(200);
            e.Property(x => x.Code).IsRequired().HasMaxLength(20);
            e.Property(x => x.ShortName).HasMaxLength(50);
            e.Property(x => x.Phone).HasMaxLength(50);
            e.Property(x => x.Website).HasMaxLength(200);
            e.Property(x => x.RegistrationNumber).HasMaxLength(100);
            e.Property(x => x.TaxNumber).HasMaxLength(100);
            e.Property(x => x.Industry).HasMaxLength(100);
            e.HasIndex(x => x.Code).IsUnique();
        });

        // ── Bank ───────────────────────────────────────────────────
        builder.Entity<Bank>(e =>
        {
            e.ToTable("Banks");
            e.HasKey(x => x.Id);
            e.Property(x => x.Name).IsRequired().HasMaxLength(200);
            e.Property(x => x.Code).HasMaxLength(20);
            e.Property(x => x.BranchCode).HasMaxLength(20);
            e.Property(x => x.SwiftCode).HasMaxLength(20);
            e.HasOne<Organization>()
             .WithMany()
             .HasForeignKey(x => x.OrganizationId)
             .OnDelete(DeleteBehavior.Restrict);
        });

        // ── Branch ─────────────────────────────────────────────────
        builder.Entity<Branch>(e =>
        {
            e.ToTable("Branches");
            e.HasKey(x => x.Id);
            e.Property(x => x.Name).IsRequired().HasMaxLength(200);
            e.Property(x => x.Code).IsRequired().HasMaxLength(20);
            e.Property(x => x.Location).HasMaxLength(200);
            e.Property(x => x.Phone).HasMaxLength(50);
            e.HasOne<Organization>()
             .WithMany()
             .HasForeignKey(x => x.OrganizationId)
             .OnDelete(DeleteBehavior.Restrict);
        });

        // ── Department ─────────────────────────────────────────────
        builder.Entity<Department>(e =>
        {
            e.ToTable("Departments");
            e.HasKey(x => x.Id);
            e.Property(x => x.Name).IsRequired().HasMaxLength(200);
            e.Property(x => x.Code).IsRequired().HasMaxLength(20);
            e.Property(x => x.Description).HasMaxLength(500);
            e.HasOne(x => x.Branch)
             .WithMany()
             .HasForeignKey(x => x.BranchId)
             .OnDelete(DeleteBehavior.Restrict);
        });

        // ── Unit ───────────────────────────────────────────────────
        builder.Entity<OrgUnit>(e =>
        {
            e.ToTable("Units");
            e.HasKey(x => x.Id);
            e.Property(x => x.Name).IsRequired().HasMaxLength(200);
            e.Property(x => x.Code).HasMaxLength(20);
            e.Property(x => x.Description).HasMaxLength(500);
            e.HasOne(x => x.Department)
             .WithMany()
             .HasForeignKey(x => x.DepartmentId)
             .OnDelete(DeleteBehavior.Restrict);
        });

        // ═══ Setup / Reference Data ════════════════════════════════
        // Codes are unique per organisation among non-deleted rows.
        builder.Entity<Region>(e =>
        {
            e.ToTable("Regions");
            e.HasKey(x => x.Id);
            e.Property(x => x.Name).IsRequired().HasMaxLength(200);
            e.Property(x => x.Code).HasMaxLength(20);
            e.HasOne<Organization>().WithMany().HasForeignKey(x => x.OrganizationId).OnDelete(DeleteBehavior.Restrict);
            e.HasIndex(x => new { x.OrganizationId, x.Code }).IsUnique()
             .HasFilter("[IsDeleted] = 0 AND [Code] IS NOT NULL");
        });

        builder.Entity<District>(e =>
        {
            e.ToTable("Districts");
            e.HasKey(x => x.Id);
            e.Property(x => x.Name).IsRequired().HasMaxLength(200);
            e.Property(x => x.Code).HasMaxLength(20);
            e.HasOne<Organization>().WithMany().HasForeignKey(x => x.OrganizationId).OnDelete(DeleteBehavior.Restrict);
            e.HasOne(x => x.Region).WithMany().HasForeignKey(x => x.RegionId).OnDelete(DeleteBehavior.Restrict);
            e.HasIndex(x => new { x.OrganizationId, x.Code }).IsUnique()
             .HasFilter("[IsDeleted] = 0 AND [Code] IS NOT NULL");
        });

        builder.Entity<Area>(e =>
        {
            e.ToTable("Areas");
            e.HasKey(x => x.Id);
            e.Property(x => x.Name).IsRequired().HasMaxLength(200);
            e.Property(x => x.Code).HasMaxLength(20);
            e.HasOne<Organization>().WithMany().HasForeignKey(x => x.OrganizationId).OnDelete(DeleteBehavior.Restrict);
            e.HasOne(x => x.District).WithMany().HasForeignKey(x => x.DistrictId).OnDelete(DeleteBehavior.Restrict);
            e.HasIndex(x => new { x.OrganizationId, x.Code }).IsUnique()
             .HasFilter("[IsDeleted] = 0 AND [Code] IS NOT NULL");
        });

        builder.Entity<Sector>(e =>
        {
            e.ToTable("Sectors");
            e.HasKey(x => x.Id);
            e.Property(x => x.Name).IsRequired().HasMaxLength(200);
            e.Property(x => x.Code).HasMaxLength(20);
            e.HasOne<Organization>().WithMany().HasForeignKey(x => x.OrganizationId).OnDelete(DeleteBehavior.Restrict);
            e.HasIndex(x => new { x.OrganizationId, x.Code }).IsUnique()
             .HasFilter("[IsDeleted] = 0 AND [Code] IS NOT NULL");
        });

        builder.Entity<SubSector>(e =>
        {
            e.ToTable("SubSectors");
            e.HasKey(x => x.Id);
            e.Property(x => x.Name).IsRequired().HasMaxLength(200);
            e.Property(x => x.Code).HasMaxLength(20);
            e.HasOne<Organization>().WithMany().HasForeignKey(x => x.OrganizationId).OnDelete(DeleteBehavior.Restrict);
            e.HasOne(x => x.Sector).WithMany().HasForeignKey(x => x.SectorId).OnDelete(DeleteBehavior.Restrict);
            e.HasIndex(x => new { x.OrganizationId, x.Code }).IsUnique()
             .HasFilter("[IsDeleted] = 0 AND [Code] IS NOT NULL");
        });

        builder.Entity<ProductType>(e =>
        {
            e.ToTable("ProductTypes");
            e.HasKey(x => x.Id);
            e.Property(x => x.Name).IsRequired().HasMaxLength(200);
            e.Property(x => x.Code).IsRequired().HasMaxLength(20);
            e.Property(x => x.Description).HasMaxLength(500);
            e.HasOne<Organization>().WithMany().HasForeignKey(x => x.OrganizationId).OnDelete(DeleteBehavior.Restrict);
            e.HasIndex(x => new { x.OrganizationId, x.Code }).IsUnique().HasFilter("[IsDeleted] = 0");
        });

        builder.Entity<ProductDefinition>(e =>
        {
            e.ToTable("ProductDefinitions");
            e.HasKey(x => x.Id);
            e.Property(x => x.Name).IsRequired().HasMaxLength(200);
            e.Property(x => x.Code).IsRequired().HasMaxLength(20);
            e.Property(x => x.MinBalance).HasColumnType("decimal(18,2)");
            e.Property(x => x.InterestRate).HasColumnType("decimal(9,4)");
            e.Property(x => x.InterestCalcMethod).HasMaxLength(30);
            e.Property(x => x.Description).HasMaxLength(500);
            e.HasOne<Organization>().WithMany().HasForeignKey(x => x.OrganizationId).OnDelete(DeleteBehavior.Restrict);
            e.HasOne(x => x.ProductType).WithMany().HasForeignKey(x => x.ProductTypeId).OnDelete(DeleteBehavior.Restrict);
            e.HasIndex(x => new { x.OrganizationId, x.Code }).IsUnique().HasFilter("[IsDeleted] = 0");
        });

        builder.Entity<TransactionCode>(e =>
        {
            e.ToTable("TransactionCodes", t => t.HasCheckConstraint("CK_TransactionCodes_DrCr", "[DrCr] IN ('DR','CR')"));
            e.HasKey(x => x.Id);
            e.Property(x => x.Code).IsRequired().HasMaxLength(20);
            e.Property(x => x.Name).IsRequired().HasMaxLength(200);
            e.Property(x => x.DrCr).IsRequired().HasMaxLength(2);
            e.Property(x => x.Description).HasMaxLength(500);
            e.HasOne<Organization>().WithMany().HasForeignKey(x => x.OrganizationId).OnDelete(DeleteBehavior.Restrict);
            e.HasIndex(x => new { x.OrganizationId, x.Code }).IsUnique().HasFilter("[IsDeleted] = 0");
        });

        builder.Entity<GLAccount>(e =>
        {
            e.ToTable("GLAccounts", t => t.HasCheckConstraint("CK_GLAccounts_AccountType",
                "[AccountType] IN ('ASSET','LIABILITY','INCOME','EXPENSE','EQUITY')"));
            e.HasKey(x => x.Id);
            e.Property(x => x.AccountNumber).IsRequired().HasMaxLength(30);
            e.Property(x => x.AccountName).IsRequired().HasMaxLength(200);
            e.Property(x => x.AccountType).IsRequired().HasMaxLength(20);
            e.Property(x => x.Description).HasMaxLength(500);
            e.HasOne<Organization>().WithMany().HasForeignKey(x => x.OrganizationId).OnDelete(DeleteBehavior.Restrict);
            e.HasOne(x => x.Parent).WithMany().HasForeignKey(x => x.ParentId).OnDelete(DeleteBehavior.Restrict);
            e.HasIndex(x => new { x.OrganizationId, x.AccountNumber }).IsUnique().HasFilter("[IsDeleted] = 0");
        });
    }
}
