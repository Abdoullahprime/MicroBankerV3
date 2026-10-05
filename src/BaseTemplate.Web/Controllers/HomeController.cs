// src/BaseTemplate.Web/Controllers/HomeController.cs
using System.Diagnostics;
using BaseTemplate.Infrastructure.Persistence;
using BaseTemplate.Web.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BaseTemplate.Web.Controllers;

public class HomeController : BaseController
{
    private readonly AppDbContext _db;

    public HomeController(AppDbContext db) { _db = db; }

    public async Task<IActionResult> Index()
    {
        ViewData["ActivePage"] = "Dashboard";

        var vm = new DashboardViewModel
        {
            ActiveUsers  = await _db.AppUsers.CountAsync(u => !u.IsDeleted && u.IsActive),
            TotalUsers   = await _db.AppUsers.CountAsync(u => !u.IsDeleted),
            Roles        = await _db.Roles.CountAsync(r => !r.IsDeleted && r.IsActive),
            Branches     = await _db.Branches.CountAsync(b => !b.IsDeleted),
            Departments  = await _db.Departments.CountAsync(d => !d.IsDeleted),
            Units        = await _db.Units.CountAsync(u => !u.IsDeleted),
            Positions    = await _db.Positions.CountAsync(p => !p.IsDeleted),
            RecentLogins = await _db.AppUsers
                .Where(u => !u.IsDeleted && u.LastLoginAt != null)
                .OrderByDescending(u => u.LastLoginAt)
                .Take(8)
                .Select(u => new RecentLoginRow
                {
                    FullName    = u.FirstName + " " + u.LastName,
                    Username    = u.Username,
                    LastLoginAt = u.LastLoginAt!.Value,
                })
                .ToListAsync(),
        };

        return View(vm);
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
