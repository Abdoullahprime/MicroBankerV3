// src/Microbanker.Web/Models/CustomerViewModels.cs
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Microbanker.Web.Models;

/// <summary>An option in a dropdown that is filtered by another dropdown (district by region, etc.).</summary>
public class LocationOption
{
    public Guid Id { get; set; }
    public Guid ParentId { get; set; }
    public string Name { get; set; } = string.Empty;
}

// ─── SHARED FORM (Customer + Self-registration) ──────────────────────────────

public abstract class ProfileFormModel
{
    public string? EncryptedId { get; set; }

    [Required(ErrorMessage = "Please select a branch")]
    [Display(Name = "Branch")]
    public Guid? BranchId { get; set; }

    [Required(ErrorMessage = "Please select a customer type")]
    [Display(Name = "Customer Type")]
    public string CustomerType { get; set; } = "INDIVIDUAL";

    // ── Individual ──
    [StringLength(10)] public string? Prefix { get; set; }
    [StringLength(100)][Display(Name = "First Name")] public string? FirstName { get; set; }
    [StringLength(100)][Display(Name = "Middle Name")] public string? MiddleName { get; set; }
    [StringLength(100)][Display(Name = "Last Name")] public string? LastName { get; set; }
    [DataType(DataType.Date)][Display(Name = "Date of Birth")] public DateOnly? DateOfBirth { get; set; }
    public string? Sex { get; set; }
    [Display(Name = "Marital Status")] public string? MaritalStatus { get; set; }
    [StringLength(200)][Display(Name = "Spouse Name")] public string? SpouseName { get; set; }
    [Range(0, 50, ErrorMessage = "Dependants must be between 0 and 50")] public int Dependants { get; set; }
    [StringLength(100)] public string? Nationality { get; set; }
    [Display(Name = "Residence Status")] public string? ResidenceStatus { get; set; }
    [StringLength(50)] public string? Language { get; set; }
    [StringLength(100)] public string? Occupation { get; set; }
    [StringLength(200)][Display(Name = "Employer")] public string? EmployerName { get; set; }
    [StringLength(50)][Display(Name = "National ID")] public string? NationalId { get; set; }
    [StringLength(50)][Display(Name = "Passport No.")] public string? PassportNo { get; set; }

    // ── Business ──
    [StringLength(200)][Display(Name = "Business Name")] public string? BusinessName { get; set; }
    [DataType(DataType.Date)][Display(Name = "Registration Date")] public DateOnly? BusinessRegDate { get; set; }
    [StringLength(50)][Display(Name = "Registration No.")] public string? BusinessRegNo { get; set; }
    [StringLength(30)][Display(Name = "Business Telephone")] public string? BusinessTelephone { get; set; }
    [StringLength(200)][Display(Name = "Nature of Business")] public string? NatureOfBusiness { get; set; }
    [StringLength(200)][Display(Name = "Director Name")] public string? DirectorName { get; set; }

    // ── Group ──
    [StringLength(200)][Display(Name = "Group Name")] public string? GroupName { get; set; }
    [Range(0, 100000)][Display(Name = "No. of Members")] public int NoOfMembers { get; set; }
    [Range(0, 100000)][Display(Name = "No. of Male")] public int NoOfMale { get; set; }
    [Range(0, 100000)][Display(Name = "No. of Female")] public int NoOfFemale { get; set; }

    // ── Contact ──
    [StringLength(30)] public string? Telephone { get; set; }
    [StringLength(30)][Display(Name = "Mobile No.")] public string? MobileNo { get; set; }
    [StringLength(200), EmailAddress] public string? Email { get; set; }
    [StringLength(200)][Display(Name = "Address Line 1")] public string? Address1 { get; set; }
    [StringLength(200)][Display(Name = "Address Line 2")] public string? Address2 { get; set; }
    [StringLength(200)][Display(Name = "Address Line 3")] public string? Address3 { get; set; }
    [Display(Name = "Region")] public Guid? RegionId { get; set; }
    [Display(Name = "District")] public Guid? DistrictId { get; set; }
    [Display(Name = "Area")] public Guid? AreaId { get; set; }
    [Display(Name = "Sector")] public Guid? SectorId { get; set; }
    [Display(Name = "Sub-Sector")] public Guid? SubSectorId { get; set; }
    [StringLength(50)][Display(Name = "TIN")] public string? TinNo { get; set; }
    [StringLength(100)][Display(Name = "Relationship Officer")] public string? RelationshipOfficer { get; set; }

    // ── Dropdown data (filled by the controller, never posted) ──
    public List<SelectListItem> Branches { get; set; } = new();
    public List<SelectListItem> CustomerTypes { get; set; } = new();
    public List<SelectListItem> Prefixes { get; set; } = new();
    public List<SelectListItem> Sexes { get; set; } = new();
    public List<SelectListItem> MaritalStatuses { get; set; } = new();
    public List<SelectListItem> ResidenceStatuses { get; set; } = new();
    public List<SelectListItem> Regions { get; set; } = new();
    public List<LocationOption> Districts { get; set; } = new();
    public List<LocationOption> Areas { get; set; } = new();
    public List<SelectListItem> Sectors { get; set; } = new();
    public List<LocationOption> SubSectors { get; set; } = new();
}

public class CustomerFormModel : ProfileFormModel
{
    public string? CustomerNo { get; set; }
}

public class SelfRegFormModel : ProfileFormModel { }

// ─── SHARED DETAIL ───────────────────────────────────────────────────────────

public class ProfileDetailViewModel
{
    public string EncryptedId { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public string CustomerType { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string BranchName { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }

    public string? Prefix { get; set; }
    public string? FirstName { get; set; }
    public string? MiddleName { get; set; }
    public string? LastName { get; set; }
    public DateOnly? DateOfBirth { get; set; }
    public string? Sex { get; set; }
    public string? MaritalStatus { get; set; }
    public string? SpouseName { get; set; }
    public int Dependants { get; set; }
    public string? Nationality { get; set; }
    public string? ResidenceStatus { get; set; }
    public string? Language { get; set; }
    public string? Occupation { get; set; }
    public string? EmployerName { get; set; }
    public string? NationalId { get; set; }
    public string? PassportNo { get; set; }
    public string? BusinessName { get; set; }
    public DateOnly? BusinessRegDate { get; set; }
    public string? BusinessRegNo { get; set; }
    public string? BusinessTelephone { get; set; }
    public string? NatureOfBusiness { get; set; }
    public string? DirectorName { get; set; }
    public string? GroupName { get; set; }
    public int NoOfMembers { get; set; }
    public int NoOfMale { get; set; }
    public int NoOfFemale { get; set; }
    public string? Telephone { get; set; }
    public string? MobileNo { get; set; }
    public string? Email { get; set; }
    public string? Address1 { get; set; }
    public string? Address2 { get; set; }
    public string? Address3 { get; set; }
    public string? RegionName { get; set; }
    public string? DistrictName { get; set; }
    public string? AreaName { get; set; }
    public string? SectorName { get; set; }
    public string? SubSectorName { get; set; }
    public string? TinNo { get; set; }
    public string? RelationshipOfficer { get; set; }
}

public class CustomerDetailViewModel : ProfileDetailViewModel
{
    public string CustomerNo { get; set; } = string.Empty;
    public string? CreatedByName { get; set; }
    public bool IsApproved { get; set; }
    public string? ApprovedByName { get; set; }
    public DateTime? ApprovedAt { get; set; }
    public string? ApprovalRemarks { get; set; }
}

public class SelfRegDetailViewModel : ProfileDetailViewModel
{
    public string? RejectionReason { get; set; }
    public string? ReviewedByName { get; set; }
    public DateTime? ReviewedAt { get; set; }
    /// <summary>Encrypted id of the customer created on approval.</summary>
    public string? CustomerEncryptedId { get; set; }
    public string? CustomerNo { get; set; }
}

// ─── LISTS ───────────────────────────────────────────────────────────────────

public class CustomerRowViewModel
{
    public string EncryptedId { get; set; } = string.Empty;
    public string CustomerNo { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public string CustomerType { get; set; } = string.Empty;
    public string BranchName { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}

public class CustomerListViewModel
{
    public List<CustomerRowViewModel> Rows { get; set; } = new();
    public string? Status { get; set; }
    public string? Q { get; set; }
    public int Page { get; set; } = 1;
    public int TotalPages { get; set; } = 1;
    public int TotalCount { get; set; }
    public List<SelectListItem> Statuses { get; set; } = new();
}

public class SelfRegRowViewModel
{
    public string EncryptedId { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public string CustomerType { get; set; } = string.Empty;
    public string BranchName { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}

public class SelfRegListViewModel
{
    public List<SelfRegRowViewModel> Rows { get; set; } = new();
    public string? Status { get; set; }
    public int Page { get; set; } = 1;
    public int TotalPages { get; set; } = 1;
    public int TotalCount { get; set; }
    public List<SelectListItem> Statuses { get; set; } = new();
}
