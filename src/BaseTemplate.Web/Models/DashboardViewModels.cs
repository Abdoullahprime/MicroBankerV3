// src/BaseTemplate.Web/Models/DashboardViewModels.cs
namespace BaseTemplate.Web.Models;

public class DashboardViewModel
{
    public int ActiveUsers { get; set; }
    public int TotalUsers  { get; set; }
    public int Roles       { get; set; }
    public int Branches    { get; set; }
    public int Departments { get; set; }
    public int Units       { get; set; }
    public int Positions   { get; set; }
    public List<RecentLoginRow> RecentLogins { get; set; } = new();
}

public class RecentLoginRow
{
    public string   FullName    { get; set; } = "";
    public string   Username    { get; set; } = "";
    public DateTime LastLoginAt { get; set; }
}
