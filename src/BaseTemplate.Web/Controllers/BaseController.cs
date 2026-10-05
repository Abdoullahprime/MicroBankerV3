// src/BaseTemplate.Web/Controllers/BaseController.cs
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Security.Claims;

namespace BaseTemplate.Web.Controllers;

[Authorize]
public abstract class BaseController : Controller
{
    protected Guid GetCompanyId()
    {
        var val = User.FindFirstValue("companyId");
        if (string.IsNullOrEmpty(val) || !Guid.TryParse(val, out var id))
            throw new UnauthorizedAccessException("Organization ID claim not found.");
        return id;
    }

    protected Guid GetUserId()
    {
        var val = User.FindFirstValue("userId");
        if (string.IsNullOrEmpty(val) || !Guid.TryParse(val, out var id))
            throw new UnauthorizedAccessException("User ID claim not found.");
        return id;
    }

    protected Guid GetRoleId()
    {
        var val = User.FindFirstValue("roleId");
        if (string.IsNullOrEmpty(val) || !Guid.TryParse(val, out var id))
            return Guid.Empty;
        return id;
    }

    protected string GetFullName()  => User.FindFirstValue("fullName")  ?? "Unknown";
    protected string GetUserRole()  => User.FindFirstValue("role")      ?? "";
    protected string GetRoleCode()  => User.FindFirstValue("roleCode")  ?? "";

    // Must be public to match base class signature
    public override void OnActionExecuting(ActionExecutingContext context)
    {
        base.OnActionExecuting(context);

        // Users flagged MustChangePassword (e.g. the seeded admin) may not use the app until they change it.
        if (User.FindFirstValue("mustChangePassword") == "1")
        {
            context.Result = RedirectToAction("ChangePassword", "Auth");
            return;
        }
        ViewData["UserId"]      = User.FindFirstValue("userId");
        ViewData["CompanyId"]   = User.FindFirstValue("companyId");
        ViewData["FullName"]    = User.FindFirstValue("fullName");
        ViewData["UserName"]    = User.FindFirstValue("fullName");
        ViewData["UserInitials"]= User.FindFirstValue("initials");
        ViewData["UserRole"]    = User.FindFirstValue("role");
        ViewData["RoleCode"]    = User.FindFirstValue("roleCode");
        ViewData["Initials"]    = User.FindFirstValue("initials");
        ViewData["Role"]        = User.FindFirstValue("role");
    }
}
