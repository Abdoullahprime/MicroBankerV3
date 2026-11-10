// src/Microbanker.Web/Models/OrganizationViewModels.cs
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Microbanker.Web.Models;

// ─── ORGANIZATION ────────────────────────────────────────────────────────────

public class OrganizationViewModel
{
    public bool HasOrganization { get; set; }
    public string? EncryptedId { get; set; }
    public string? Name { get; set; }
    public string? ShortName { get; set; }
    public string? Address { get; set; }
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public string? Website { get; set; }
    public string? RegistrationNumber { get; set; }
    public string? TaxNumber { get; set; }
    public string? Industry { get; set; }
    public DateTime? EstablishedDate { get; set; }
}

public class OrganizationFormModel
{
    [Required, MaxLength(200)]
    [Display(Name = "Organization Name")]
    public string Name { get; set; } = "";

    [MaxLength(50)]
    [Display(Name = "Short Name / Code")]
    public string? ShortName { get; set; }

    [MaxLength(500)]
    public string? Address { get; set; }

    [MaxLength(50)]
    public string? Phone { get; set; }

    [MaxLength(200), EmailAddress]
    public string? Email { get; set; }

    [MaxLength(200)]
    public string? Website { get; set; }

    [MaxLength(100)]
    [Display(Name = "Registration Number")]
    public string? RegistrationNumber { get; set; }

    [MaxLength(100)]
    [Display(Name = "Tax Number")]
    public string? TaxNumber { get; set; }

    [MaxLength(100)]
    public string? Industry { get; set; }

    [Display(Name = "Established Date")]
    public DateTime? EstablishedDate { get; set; }

    public string? EncryptedId { get; set; }
}

// ─── BRANCH ──────────────────────────────────────────────────────────────────

public class BranchListViewModel
{
    public List<BranchRowModel> Branches { get; set; } = new();
}

public class BranchRowModel
{
    public string Name { get; set; } = "";
    public string? Code { get; set; }
    public string? Location { get; set; }
    public bool IsHeadOffice { get; set; }
    public bool IsActive { get; set; }
    public string EncryptedId { get; set; } = "";
}

public class BranchFormModel
{
    [Required, MaxLength(200)]
    [Display(Name = "Branch Name")]
    public string Name { get; set; } = "";

    [MaxLength(20)]
    public string? Code { get; set; }

    [MaxLength(200)]
    public string? Location { get; set; }

    [MaxLength(500)]
    public string? Address { get; set; }

    [MaxLength(50)]
    public string? Phone { get; set; }

    [MaxLength(200), EmailAddress]
    public string? Email { get; set; }

    [Display(Name = "Head Office")]
    public bool IsHeadOffice { get; set; }

    [Display(Name = "Active")]
    public bool IsActive { get; set; } = true;

    public string? EncryptedId { get; set; }
}

// ─── DEPARTMENT ───────────────────────────────────────────────────────────────

public class DepartmentListViewModel
{
    public List<DepartmentRowModel> Departments { get; set; } = new();
}

public class DepartmentRowModel
{
    public string Name { get; set; } = "";
    public string? Code { get; set; }
    public string BranchName { get; set; } = "";
    public bool IsActive { get; set; }
    public string EncryptedId { get; set; } = "";
}

public class DepartmentFormModel
{
    [Required, MaxLength(200)]
    [Display(Name = "Department Name")]
    public string Name { get; set; } = "";

    [MaxLength(20)]
    public string? Code { get; set; }

    [MaxLength(500)]
    public string? Description { get; set; }

    [Required(ErrorMessage = "Please select a branch.")]
    [Display(Name = "Branch")]
    public string? EncryptedBranchId { get; set; }

    [Display(Name = "Active")]
    public bool IsActive { get; set; } = true;

    public string? EncryptedId { get; set; }
    public List<SelectListItem> BranchOptions { get; set; } = new();
}

// ─── UNIT ─────────────────────────────────────────────────────────────────────

public class UnitListViewModel
{
    public List<UnitRowModel> Units { get; set; } = new();
}

public class UnitRowModel
{
    public string Name { get; set; } = "";
    public string? Code { get; set; }
    public string DepartmentName { get; set; } = "";
    public string BranchName { get; set; } = "";
    public bool IsActive { get; set; }
    public string EncryptedId { get; set; } = "";
}

public class UnitFormModel
{
    [Required, MaxLength(200)]
    [Display(Name = "Unit Name")]
    public string Name { get; set; } = "";

    [MaxLength(20)]
    public string? Code { get; set; }

    [MaxLength(500)]
    public string? Description { get; set; }

    [Required(ErrorMessage = "Please select a department.")]
    [Display(Name = "Department")]
    public string? EncryptedDepartmentId { get; set; }

    [Display(Name = "Active")]
    public bool IsActive { get; set; } = true;

    public string? EncryptedId { get; set; }
    public List<SelectListItem> DepartmentOptions { get; set; } = new();
}

// ─── BANK ─────────────────────────────────────────────────────────────────────

public class BankViewModel
{
    public string? EncryptedId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Code { get; set; }
    public string? BranchCode { get; set; }
    public string? SwiftCode { get; set; }
    public bool IsActive { get; set; } = true;
}

public class BankFormModel
{
    [Required(ErrorMessage = "Bank name is required")]
    [Display(Name = "Bank Name")]
    [StringLength(200)]
    public string Name { get; set; } = string.Empty;

    [Display(Name = "Bank Code")]
    [StringLength(20)]
    public string? Code { get; set; }

    [Display(Name = "Branch Code")]
    [StringLength(20)]
    public string? BranchCode { get; set; }

    [Display(Name = "SWIFT Code")]
    [StringLength(20)]
    public string? SwiftCode { get; set; }

    [Display(Name = "Active")]
    public bool IsActive { get; set; } = true;

    public string? EncryptedId { get; set; }
}
