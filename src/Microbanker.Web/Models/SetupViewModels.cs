// src/Microbanker.Web/Models/SetupViewModels.cs
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Microbanker.Web.Models;

// ─── REGION ──────────────────────────────────────────────────────────────────

public class RegionViewModel
{
    public string EncryptedId { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Code { get; set; }
    public bool IsActive { get; set; } = true;
}

public class RegionFormModel
{
    public string? EncryptedId { get; set; }

    [Required(ErrorMessage = "Region name is required"), StringLength(200)]
    [Display(Name = "Region Name")]
    public string Name { get; set; } = string.Empty;

    [StringLength(20)]
    public string? Code { get; set; }

    [Display(Name = "Active")]
    public bool IsActive { get; set; } = true;
}

// ─── DISTRICT ────────────────────────────────────────────────────────────────

public class DistrictViewModel
{
    public string EncryptedId { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Code { get; set; }
    public string RegionName { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
}

public class DistrictFormModel
{
    public string? EncryptedId { get; set; }

    [Required(ErrorMessage = "Please select a region")]
    [Display(Name = "Region")]
    public Guid? RegionId { get; set; }

    [Required(ErrorMessage = "District name is required"), StringLength(200)]
    [Display(Name = "District Name")]
    public string Name { get; set; } = string.Empty;

    [StringLength(20)]
    public string? Code { get; set; }

    [Display(Name = "Active")]
    public bool IsActive { get; set; } = true;

    public List<SelectListItem> Regions { get; set; } = new();
}

// ─── AREA ────────────────────────────────────────────────────────────────────

public class AreaViewModel
{
    public string EncryptedId { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Code { get; set; }
    public string DistrictName { get; set; } = string.Empty;
    public string RegionName { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
}

public class DistrictOption
{
    public Guid Id { get; set; }
    public Guid RegionId { get; set; }
    public string Name { get; set; } = string.Empty;
}

public class AreaFormModel
{
    public string? EncryptedId { get; set; }

    /// <summary>Only used to filter the district list in the browser; not stored.</summary>
    [Display(Name = "Region")]
    public Guid? RegionId { get; set; }

    [Required(ErrorMessage = "Please select a district")]
    [Display(Name = "District")]
    public Guid? DistrictId { get; set; }

    [Required(ErrorMessage = "Area name is required"), StringLength(200)]
    [Display(Name = "Area Name")]
    public string Name { get; set; } = string.Empty;

    [StringLength(20)]
    public string? Code { get; set; }

    [Display(Name = "Active")]
    public bool IsActive { get; set; } = true;

    public List<SelectListItem> Regions { get; set; } = new();
    public List<DistrictOption> Districts { get; set; } = new();
}

// ─── SECTOR ──────────────────────────────────────────────────────────────────

public class SectorViewModel
{
    public string EncryptedId { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Code { get; set; }
    public bool IsActive { get; set; } = true;
}

public class SectorFormModel
{
    public string? EncryptedId { get; set; }

    [Required(ErrorMessage = "Sector name is required"), StringLength(200)]
    [Display(Name = "Sector Name")]
    public string Name { get; set; } = string.Empty;

    [StringLength(20)]
    public string? Code { get; set; }

    [Display(Name = "Active")]
    public bool IsActive { get; set; } = true;
}

// ─── SUB-SECTOR ──────────────────────────────────────────────────────────────

public class SubSectorViewModel
{
    public string EncryptedId { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Code { get; set; }
    public string SectorName { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
}

public class SubSectorFormModel
{
    public string? EncryptedId { get; set; }

    [Required(ErrorMessage = "Please select a sector")]
    [Display(Name = "Sector")]
    public Guid? SectorId { get; set; }

    [Required(ErrorMessage = "Sub-sector name is required"), StringLength(200)]
    [Display(Name = "Sub-Sector Name")]
    public string Name { get; set; } = string.Empty;

    [StringLength(20)]
    public string? Code { get; set; }

    [Display(Name = "Active")]
    public bool IsActive { get; set; } = true;

    public List<SelectListItem> Sectors { get; set; } = new();
}

// ─── PRODUCT TYPE ────────────────────────────────────────────────────────────

public class ProductTypeViewModel
{
    public string EncryptedId { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsActive { get; set; } = true;
}

public class ProductTypeFormModel
{
    public string? EncryptedId { get; set; }

    [Required(ErrorMessage = "Product type name is required"), StringLength(200)]
    [Display(Name = "Product Type Name")]
    public string Name { get; set; } = string.Empty;

    [Required(ErrorMessage = "Code is required"), StringLength(20)]
    public string Code { get; set; } = string.Empty;

    [StringLength(500)]
    public string? Description { get; set; }

    [Display(Name = "Active")]
    public bool IsActive { get; set; } = true;
}

// ─── PRODUCT DEFINITION ──────────────────────────────────────────────────────

public class ProductDefinitionViewModel
{
    public string EncryptedId { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public string ProductTypeName { get; set; } = string.Empty;
    public decimal MinBalance { get; set; }
    public decimal InterestRate { get; set; }
    public string? InterestCalcMethod { get; set; }
    public bool IsActive { get; set; } = true;
}

public class ProductDefinitionFormModel
{
    public string? EncryptedId { get; set; }

    [Required(ErrorMessage = "Please select a product type")]
    [Display(Name = "Product Type")]
    public Guid? ProductTypeId { get; set; }

    [Required(ErrorMessage = "Product name is required"), StringLength(200)]
    [Display(Name = "Product Name")]
    public string Name { get; set; } = string.Empty;

    [Required(ErrorMessage = "Code is required"), StringLength(20)]
    public string Code { get; set; } = string.Empty;

    [Range(typeof(decimal), "0", "999999999999999", ErrorMessage = "Minimum balance cannot be negative")]
    [Display(Name = "Minimum Balance")]
    public decimal MinBalance { get; set; }

    [Range(typeof(decimal), "0", "100", ErrorMessage = "Interest rate must be between 0 and 100")]
    [Display(Name = "Interest Rate (% p.a.)")]
    public decimal InterestRate { get; set; }

    [Display(Name = "Interest Calculation Method")]
    public string? InterestCalcMethod { get; set; }

    [Range(0, 12, ErrorMessage = "Frequency must be between 0 and 12 months")]
    [Display(Name = "Interest Frequency (months)")]
    public int InterestFrequency { get; set; }

    [Range(0, 3650, ErrorMessage = "Dormancy must be between 0 and 3650 days")]
    [Display(Name = "Dormancy (days)")]
    public int DormancyDays { get; set; }

    [Range(0, 1000, ErrorMessage = "Value must be between 0 and 1000")]
    [Display(Name = "Max Debits / Month")]
    public int MaxDebitPerMonth { get; set; }

    [StringLength(500)]
    public string? Description { get; set; }

    [Display(Name = "Active")]
    public bool IsActive { get; set; } = true;

    public List<SelectListItem> ProductTypes { get; set; } = new();
    public List<SelectListItem> CalcMethods { get; set; } = new();
}

// ─── TRANSACTION CODE ────────────────────────────────────────────────────────

public class TransactionCodeViewModel
{
    public string EncryptedId { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string DrCr { get; set; } = string.Empty;
    public bool RequiresApproval { get; set; }
    public bool IsActive { get; set; } = true;
}

public class TransactionCodeFormModel
{
    public string? EncryptedId { get; set; }

    [Required(ErrorMessage = "Code is required"), StringLength(20)]
    public string Code { get; set; } = string.Empty;

    [Required(ErrorMessage = "Name is required"), StringLength(200)]
    public string Name { get; set; } = string.Empty;

    [Required(ErrorMessage = "Please select Debit or Credit")]
    [Display(Name = "Debit / Credit")]
    public string DrCr { get; set; } = string.Empty;

    [StringLength(500)]
    public string? Description { get; set; }

    [Display(Name = "Requires Approval")]
    public bool RequiresApproval { get; set; }

    [Display(Name = "Active")]
    public bool IsActive { get; set; } = true;

    public List<SelectListItem> DrCrOptions { get; set; } = new();
}

// ─── CHART OF ACCOUNTS (GL) ──────────────────────────────────────────────────

public class GLAccountViewModel
{
    public string EncryptedId { get; set; } = string.Empty;
    public string AccountNumber { get; set; } = string.Empty;
    public string AccountName { get; set; } = string.Empty;
    public string AccountType { get; set; } = string.Empty;
    public string? ParentLabel { get; set; }
    public int Level { get; set; } = 1;
    public bool IsControlAccount { get; set; }
    public bool IsActive { get; set; } = true;
}

public class GLAccountFormModel
{
    public string? EncryptedId { get; set; }

    [Required(ErrorMessage = "Account number is required"), StringLength(30)]
    [Display(Name = "Account Number")]
    public string AccountNumber { get; set; } = string.Empty;

    [Required(ErrorMessage = "Account name is required"), StringLength(200)]
    [Display(Name = "Account Name")]
    public string AccountName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Please select an account type")]
    [Display(Name = "Account Type")]
    public string AccountType { get; set; } = string.Empty;

    [Display(Name = "Parent Account")]
    public Guid? ParentId { get; set; }

    [Display(Name = "Control Account")]
    public bool IsControlAccount { get; set; }

    [StringLength(500)]
    public string? Description { get; set; }

    [Display(Name = "Active")]
    public bool IsActive { get; set; } = true;

    public List<SelectListItem> AccountTypes { get; set; } = new();
    public List<SelectListItem> Parents { get; set; } = new();
}
