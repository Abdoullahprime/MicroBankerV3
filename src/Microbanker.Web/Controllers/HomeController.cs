// src/Microbanker.Web/Controllers/HomeController.cs
using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microbanker.Infrastructure.Persistence;
using Microbanker.Web.Models;

namespace Microbanker.Web.Controllers;

public class HomeController : BaseController
{
    private readonly MicrobankerDbContext _db;

    public HomeController(MicrobankerDbContext db) { _db = db; }

    public async Task<IActionResult> Index()
    {
        ViewData["ActivePage"] = "Dashboard";

        var org = await _db.Organizations
            .Where(o => !o.IsDeleted)
            .FirstOrDefaultAsync();

        var vm = new DashboardViewModel
        {
            HasOrganization  = org != null,
            OrganizationName = org?.Name
        };

        if (org != null)
        {
            var branches = await _db.Branches
                .Where(b => b.OrganizationId == org.Id && !b.IsDeleted)
                .ToListAsync();

            var branchIds = branches.Select(b => b.Id).ToList();

            var departments = await _db.Departments
                .Where(d => branchIds.Contains(d.BranchId) && !d.IsDeleted)
                .ToListAsync();

            var deptIds = departments.Select(d => d.Id).ToList();

            var units = await _db.Units
                .Where(u => deptIds.Contains(u.DepartmentId) && !u.IsDeleted)
                .ToListAsync();

            vm.BranchCount     = branches.Count;
            vm.DepartmentCount = departments.Count;
            vm.UnitCount       = units.Count;

            vm.BankCount = await _db.Banks
                .CountAsync(b => b.OrganizationId == org.Id && !b.IsDeleted);

            vm.UserCount = await _db.AppUsers
                .CountAsync(u => u.OrganizationId == org.Id && !u.IsDeleted);

            vm.ActiveUserCount = await _db.AppUsers
                .CountAsync(u => u.OrganizationId == org.Id && !u.IsDeleted && u.IsActive);

            vm.RoleCount = await _db.Roles
                .CountAsync(r => r.OrganizationId == org.Id && !r.IsDeleted);

            vm.BranchBreakdown = branches
                .OrderByDescending(b => b.IsHeadOffice)
                .ThenBy(b => b.Name)
                .Select(b =>
                {
                    var deptsInBranch   = departments.Where(d => d.BranchId == b.Id).ToList();
                    var deptIdsInBranch = deptsInBranch.Select(d => d.Id).ToList();
                    return new BranchSummaryRow
                    {
                        BranchName      = b.Name,
                        IsHeadOffice    = b.IsHeadOffice,
                        DepartmentCount = deptsInBranch.Count,
                        UnitCount       = units.Count(u => deptIdsInBranch.Contains(u.DepartmentId))
                    };
                })
                .ToList();
        }

        return View(vm);
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
