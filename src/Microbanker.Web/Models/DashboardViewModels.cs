// src/Microbanker.Web/Models/DashboardViewModels.cs
namespace Microbanker.Web.Models;

/// <summary>
/// Landing dashboard. Currently reports on the organisation structure and
/// security set-up only — banking metrics get added as those modules land.
/// </summary>
public class DashboardViewModel
{
    public bool HasOrganization { get; set; }
    public string? OrganizationName { get; set; }

    // ── Organisation structure ─────────────────────────────────────
    public int BranchCount { get; set; }
    public int DepartmentCount { get; set; }
    public int UnitCount { get; set; }
    public int BankCount { get; set; }

    // ── Security ───────────────────────────────────────────────────
    public int UserCount { get; set; }
    public int ActiveUserCount { get; set; }
    public int RoleCount { get; set; }

    // ── Branch breakdown (departments per branch) ──────────────────
    public List<BranchSummaryRow> BranchBreakdown { get; set; } = new();
}

public class BranchSummaryRow
{
    public string BranchName { get; set; } = string.Empty;
    public bool IsHeadOffice { get; set; }
    public int DepartmentCount { get; set; }
    public int UnitCount { get; set; }
}
