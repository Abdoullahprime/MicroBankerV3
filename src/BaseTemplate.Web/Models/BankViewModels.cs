// src/BaseTemplate.Web/Models/BankViewModels.cs
using System.ComponentModel.DataAnnotations;

namespace BaseTemplate.Web.Models;

// ── BANK ──────────────────────────────────────────────────────────────────────

public class BankViewModel
{
    public string EncryptedId { get; set; } = "";
    public string Name { get; set; } = "";
    public string? Code { get; set; }
    public string? BranchCode { get; set; }
    public string? SwiftCode { get; set; }
    public bool IsActive { get; set; }
}

public class BankFormModel
{
    public string? EncryptedId { get; set; }

    [Required(ErrorMessage = "Bank name is required."), MaxLength(200)]
    [Display(Name = "Bank Name")]
    public string Name { get; set; } = "";

    [MaxLength(20)]
    [Display(Name = "Bank Code")]
    public string? Code { get; set; }

    [MaxLength(20)]
    [Display(Name = "Branch Code")]
    public string? BranchCode { get; set; }

    [MaxLength(20)]
    [Display(Name = "SWIFT Code")]
    public string? SwiftCode { get; set; }

    [Display(Name = "Active")]
    public bool IsActive { get; set; } = true;
}
