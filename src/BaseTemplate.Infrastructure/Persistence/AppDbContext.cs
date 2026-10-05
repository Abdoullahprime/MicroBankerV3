// src/BaseTemplate.Infrastructure/Persistence/AppDbContext.cs
using BaseTemplate.Domain.Entities.Calendar;
using BaseTemplate.Domain.Entities.Auth;
using BaseTemplate.Domain.Entities.Notifications;
using BaseTemplate.Domain.Entities.Organization;
using BaseTemplate.Domain.Entities.Settings;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using OrgUnit = BaseTemplate.Domain.Entities.Organization.Unit;

namespace BaseTemplate.Infrastructure.Persistence;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    // ── Auth & Rights ──────────────────────────────────────────────
    public DbSet<AppUser> AppUsers { get; set; }
    public DbSet<Role> Roles { get; set; }
    public DbSet<UserRole> UserRoles { get; set; }
    public DbSet<AppModule> AppModules { get; set; }
    public DbSet<AppAction> AppActions { get; set; }
    public DbSet<RolePermission> RolePermissions { get; set; }

    // ── Organization ───────────────────────────────────────────────
    public DbSet<Organization>       Organizations       { get; set; }
    public DbSet<Branch>             Branches            { get; set; }
    public DbSet<Department>         Departments         { get; set; }
    public DbSet<OrgUnit>            Units               { get; set; }
    public DbSet<Position>           Positions           { get; set; }
    public DbSet<Bank>               Banks               { get; set; }
    public DbSet<EmailConfiguration> EmailConfigurations { get; set; }

    // ── Settings, calendar, notifications ──────────────────────────
    public DbSet<SystemSetting>     SystemSettings     { get; set; }
    public DbSet<WorkCalendar>      WorkCalendars      { get; set; }
    public DbSet<PublicHoliday>     PublicHolidays     { get; set; }
    public DbSet<PushSubscription>  PushSubscriptions  { get; set; }
    public DbSet<AppNotification>   AppNotifications   { get; set; }

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

        // ── Position ───────────────────────────────────────────────
        builder.Entity<Position>(e =>
        {
            e.ToTable("Positions");
            e.HasKey(x => x.Id);
            e.Property(x => x.Name).IsRequired().HasMaxLength(150);
            e.Property(x => x.Code).HasMaxLength(20);
            e.Property(x => x.Description).HasMaxLength(500);
        });

        // ── WorkCalendar ───────────────────────────────────────────
        builder.Entity<WorkCalendar>(e =>
        {
            e.ToTable("WorkCalendars");
            e.HasKey(x => x.Id);
            e.HasIndex(x => new { x.OrganizationId, x.Month, x.Year }).IsUnique();
        });

        // ── PushSubscription ───────────────────────────────────────
        builder.Entity<PushSubscription>(e =>
        {
            e.ToTable("PushSubscriptions");
            e.HasKey(x => x.Id);
            e.Property(x => x.Audience).HasConversion<int>();
            e.Property(x => x.Endpoint).IsRequired().HasMaxLength(800);
            e.Property(x => x.P256dh).IsRequired().HasMaxLength(200);
            e.Property(x => x.Auth).IsRequired().HasMaxLength(100);
            e.Property(x => x.UserAgent).HasMaxLength(400);
            e.HasIndex(x => x.Endpoint).IsUnique();
            e.HasIndex(x => new { x.Audience, x.UserId });
        });

        // ── AppNotification ────────────────────────────────────────
        builder.Entity<AppNotification>(e =>
        {
            e.ToTable("AppNotifications");
            e.HasKey(x => x.Id);
            e.Property(x => x.Audience).HasConversion<int>();
            e.Property(x => x.Title).IsRequired().HasMaxLength(200);
            e.Property(x => x.Body).IsRequired().HasMaxLength(1000);
            e.Property(x => x.Url).HasMaxLength(500);
            e.Property(x => x.Category).IsRequired().HasMaxLength(50);
            e.HasIndex(x => new { x.Audience, x.UserId, x.IsRead });
        });

        builder.Entity<EmailConfiguration>(e =>
        {
            e.Property(x => x.Channel).HasConversion<int>();
            e.HasIndex(x => new { x.OrganizationId, x.Channel });
        });
    }
}
