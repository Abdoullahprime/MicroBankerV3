// src/Microbanker.Web/Controllers/BankingControllerBase.cs
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microbanker.Domain.Entities.Customers;
using Microbanker.Infrastructure.Persistence;
using Microbanker.Web.Models;
using Microbanker.Web.Services;

namespace Microbanker.Web.Controllers;

/// <summary>Shared logic for the customer and self-registration controllers.</summary>
public abstract class BankingControllerBase : SetupControllerBase
{
    protected BankingControllerBase(MicrobankerDbContext db, IEncryptionService enc) : base(db, enc) { }

    protected const int PageSize = 25;

    // ── Dropdown data ─────────────────────────────────────────────────────────

    protected async Task FillListsAsync(ProfileFormModel m, Guid orgId)
    {
        m.Branches = await _db.Branches
            .Where(b => b.OrganizationId == orgId && !b.IsDeleted && b.IsActive)
            .OrderBy(b => b.Name)
            .Select(b => new SelectListItem(b.Name + " (" + b.Code + ")", b.Id.ToString()))
            .ToListAsync();

        m.CustomerTypes     = Options(CustomerOptions.CustomerTypes);
        m.Prefixes          = Options(CustomerOptions.Prefixes);
        m.Sexes             = Options(CustomerOptions.Sexes);
        m.MaritalStatuses   = Options(CustomerOptions.MaritalStatuses);
        m.ResidenceStatuses = Options(CustomerOptions.ResidenceStatuses);

        m.Regions = await RegionItemsAsync(orgId);
        m.Sectors = await SectorItemsAsync(orgId);

        m.Districts = await _db.Districts
            .Where(x => x.OrganizationId == orgId && !x.IsDeleted && x.IsActive)
            .OrderBy(x => x.Name)
            .Select(x => new LocationOption { Id = x.Id, ParentId = x.RegionId, Name = x.Name }).ToListAsync();
        m.Areas = await _db.Areas
            .Where(x => x.OrganizationId == orgId && !x.IsDeleted && x.IsActive)
            .OrderBy(x => x.Name)
            .Select(x => new LocationOption { Id = x.Id, ParentId = x.DistrictId, Name = x.Name }).ToListAsync();
        m.SubSectors = await _db.SubSectors
            .Where(x => x.OrganizationId == orgId && !x.IsDeleted && x.IsActive)
            .OrderBy(x => x.Name)
            .Select(x => new LocationOption { Id = x.Id, ParentId = x.SectorId, Name = x.Name }).ToListAsync();
    }

    // ── Validation ────────────────────────────────────────────────────────────

    /// <summary>
    /// Server-side rules that depend on the customer type or on other records.
    /// Adds errors to ModelState; the caller checks ModelState.IsValid afterwards.
    /// </summary>
    protected async Task ValidateProfileAsync(ProfileFormModel m, Guid orgId)
    {
        if (!CustomerOptions.CustomerTypes.Contains(m.CustomerType ?? ""))
            ModelState.AddModelError(nameof(m.CustomerType), "Please select a valid customer type.");

        static bool Blank(string? s) => string.IsNullOrWhiteSpace(s);

        switch (m.CustomerType)
        {
            case CustomerOptions.Individual:
                if (Blank(m.FirstName)) ModelState.AddModelError(nameof(m.FirstName), "First name is required.");
                if (Blank(m.LastName))  ModelState.AddModelError(nameof(m.LastName), "Last name is required.");
                break;
            case CustomerOptions.Business:
                if (Blank(m.BusinessName)) ModelState.AddModelError(nameof(m.BusinessName), "Business name is required.");
                break;
            case CustomerOptions.Group:
                if (Blank(m.GroupName)) ModelState.AddModelError(nameof(m.GroupName), "Group name is required.");
                if (m.NoOfMale + m.NoOfFemale != m.NoOfMembers)
                    ModelState.AddModelError(nameof(m.NoOfMembers), "Male and female counts must add up to the number of members.");
                break;
        }

        void InList(string? value, string[] allowed, string field, string label)
        {
            if (!Blank(value) && !allowed.Contains(value!.Trim()))
                ModelState.AddModelError(field, $"Please select a valid {label}.");
        }
        InList(m.Prefix, CustomerOptions.Prefixes, nameof(m.Prefix), "prefix");
        InList(m.Sex, CustomerOptions.Sexes, nameof(m.Sex), "sex");
        InList(m.MaritalStatus, CustomerOptions.MaritalStatuses, nameof(m.MaritalStatus), "marital status");
        InList(m.ResidenceStatus, CustomerOptions.ResidenceStatuses, nameof(m.ResidenceStatus), "residence status");

        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        if (m.DateOfBirth > today)
            ModelState.AddModelError(nameof(m.DateOfBirth), "Date of birth cannot be in the future.");
        if (m.BusinessRegDate > today)
            ModelState.AddModelError(nameof(m.BusinessRegDate), "Registration date cannot be in the future.");

        if (m.BranchId != null &&
            !await _db.Branches.AnyAsync(b => b.Id == m.BranchId && b.OrganizationId == orgId && !b.IsDeleted && b.IsActive))
            ModelState.AddModelError(nameof(m.BranchId), "Please select a valid branch.");

        await ValidateLocationAsync(m, orgId);
    }

    /// <summary>Each chosen location must exist in this organisation and sit under the chosen parent.</summary>
    private async Task ValidateLocationAsync(ProfileFormModel m, Guid orgId)
    {
        if (m.RegionId != null &&
            !await _db.Regions.AnyAsync(x => x.Id == m.RegionId && x.OrganizationId == orgId && !x.IsDeleted))
            ModelState.AddModelError(nameof(m.RegionId), "Please select a valid region.");

        if (m.DistrictId != null &&
            !await _db.Districts.AnyAsync(x => x.Id == m.DistrictId && x.OrganizationId == orgId && !x.IsDeleted
                                               && x.RegionId == m.RegionId))
            ModelState.AddModelError(nameof(m.DistrictId), "Please select a district that belongs to the chosen region.");

        if (m.AreaId != null &&
            !await _db.Areas.AnyAsync(x => x.Id == m.AreaId && x.OrganizationId == orgId && !x.IsDeleted
                                           && x.DistrictId == m.DistrictId))
            ModelState.AddModelError(nameof(m.AreaId), "Please select an area that belongs to the chosen district.");

        if (m.SectorId != null &&
            !await _db.Sectors.AnyAsync(x => x.Id == m.SectorId && x.OrganizationId == orgId && !x.IsDeleted))
            ModelState.AddModelError(nameof(m.SectorId), "Please select a valid sector.");

        if (m.SubSectorId != null &&
            !await _db.SubSectors.AnyAsync(x => x.Id == m.SubSectorId && x.OrganizationId == orgId && !x.IsDeleted
                                               && x.SectorId == m.SectorId))
            ModelState.AddModelError(nameof(m.SubSectorId), "Please select a sub-sector that belongs to the chosen sector.");
    }

    /// <summary>True when another non-deleted customer already holds this national ID.</summary>
    protected async Task<bool> NationalIdTakenAsync(Guid orgId, string? nationalId, Guid excludeCustomerId)
    {
        if (string.IsNullOrWhiteSpace(nationalId)) return false;
        var id = nationalId.Trim();
        return await _db.Customers.AnyAsync(c =>
            c.OrganizationId == orgId && !c.IsDeleted && c.Id != excludeCustomerId && c.NationalId == id);
    }

    // ── Customer numbers ──────────────────────────────────────────────────────

    /// <summary>
    /// Next CUST-{year}-{seq:D6}. Based on the highest number ever issued this year (deleted rows
    /// included), so numbers are never reused. A unique index backs this up; see <see cref="AddCustomerAsync"/>.
    /// </summary>
    private async Task<string> NextCustomerNoAsync(Guid orgId)
    {
        var prefix = $"CUST-{DateTime.UtcNow.Year}-";
        var last = await _db.Customers
            .Where(c => c.OrganizationId == orgId && c.CustomerNo.StartsWith(prefix))
            .OrderByDescending(c => c.CustomerNo)
            .Select(c => c.CustomerNo)
            .FirstOrDefaultAsync();

        var seq = last != null && int.TryParse(last.AsSpan(prefix.Length), out var n) ? n : 0;
        return $"{prefix}{seq + 1:D6}";
    }

    /// <summary>
    /// Creates and saves a customer with a fresh number. If two users collide on the same number the
    /// unique index rejects the second save and we simply take the next one.
    /// </summary>
    protected async Task<Customer> AddCustomerAsync(Guid orgId, CustomerProfile profile, Guid createdBy)
    {
        for (var attempt = 1; ; attempt++)
        {
            var customer = new Customer(orgId, profile.BranchId, await NextCustomerNoAsync(orgId),
                profile.CustomerType, profile.FirstName, profile.LastName, createdBy);
            customer.Update(profile);
            _db.Customers.Add(customer);
            try
            {
                await _db.SaveChangesAsync();
                return customer;
            }
            catch (DbUpdateException ex) when (attempt < 5 && ex.InnerException is SqlException { Number: 2601 or 2627 })
            {
                _db.Entry(customer).State = EntityState.Detached;
            }
        }
    }

    // ── Detail view data ──────────────────────────────────────────────────────

    protected static void FillDetail(ProfileDetailViewModel d, PersonProfileBase e)
    {
        d.DisplayName = e.DisplayName;
        d.CustomerType = e.CustomerType;
        d.BranchName = e.Branch?.Name ?? "";
        d.CreatedAt = e.CreatedAt;
        d.Prefix = e.Prefix; d.FirstName = e.FirstName; d.MiddleName = e.MiddleName; d.LastName = e.LastName;
        d.DateOfBirth = e.DateOfBirth; d.Sex = e.Sex; d.MaritalStatus = e.MaritalStatus; d.SpouseName = e.SpouseName;
        d.Dependants = e.Dependants; d.Nationality = e.Nationality; d.ResidenceStatus = e.ResidenceStatus;
        d.Language = e.Language; d.Occupation = e.Occupation; d.EmployerName = e.EmployerName;
        d.NationalId = e.NationalId; d.PassportNo = e.PassportNo;
        d.BusinessName = e.BusinessName; d.BusinessRegDate = e.BusinessRegDate; d.BusinessRegNo = e.BusinessRegNo;
        d.BusinessTelephone = e.BusinessTelephone; d.NatureOfBusiness = e.NatureOfBusiness; d.DirectorName = e.DirectorName;
        d.GroupName = e.GroupName; d.NoOfMembers = e.NoOfMembers; d.NoOfMale = e.NoOfMale; d.NoOfFemale = e.NoOfFemale;
        d.Telephone = e.Telephone; d.MobileNo = e.MobileNo; d.Email = e.Email;
        d.Address1 = e.Address1; d.Address2 = e.Address2; d.Address3 = e.Address3;
        d.RegionName = e.Region?.Name; d.DistrictName = e.District?.Name; d.AreaName = e.Area?.Name;
        d.SectorName = e.Sector?.Name; d.SubSectorName = e.SubSector?.Name;
        d.TinNo = e.TinNo; d.RelationshipOfficer = e.RelationshipOfficer;
    }

    protected async Task<string?> UserNameAsync(Guid? userId)
    {
        if (userId == null) return null;
        var u = await _db.AppUsers.Where(x => x.Id == userId).Select(x => new { x.FirstName, x.LastName }).FirstOrDefaultAsync();
        return u == null ? null : $"{u.FirstName} {u.LastName}".Trim();
    }

    protected static List<SelectListItem> StatusItems(IEnumerable<string> statuses)
        => statuses.Select(s => new SelectListItem(char.ToUpper(s[0]) + s[1..].ToLower(), s)).ToList();
}
