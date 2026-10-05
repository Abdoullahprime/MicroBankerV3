// src/BaseTemplate.Web/Controllers/AuthController.cs
using BaseTemplate.Domain.Entities.Auth;
using BaseTemplate.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace BaseTemplate.Web.Controllers;

public class AuthController : Controller
{
    private readonly AppDbContext _db;

    public AuthController(AppDbContext db)
    {
        _db = db;
    }

    // ── GET /Auth/Login ────────────────────────────────────────────
    [HttpGet]
    public IActionResult Login(string? returnUrl = null)
    {
        if (User.Identity?.IsAuthenticated == true)
            return RedirectToAction("Index", "Home");

        ViewData["ReturnUrl"] = returnUrl;
        return View();
    }

    // ── POST /Auth/Login ───────────────────────────────────────────
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(string username, string password, string? returnUrl = null)
    {
        ViewData["ReturnUrl"] = returnUrl;

        if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
        {
            ModelState.AddModelError("", "Username and password are required.");
            return View();
        }

        var user = await _db.AppUsers
            .Where(u => u.Username == username.Trim().ToLower() && !u.IsDeleted && u.IsActive)
            .FirstOrDefaultAsync();

        if (user == null || !BCrypt.Net.BCrypt.Verify(password, user.PasswordHash))
        {
            ModelState.AddModelError("", "Invalid username or password.");
            return View();
        }

        // Look up the user's primary role
        var userRole = await _db.UserRoles
            .Include(ur => ur.Role)
            .Where(ur => ur.UserId == user.Id && !ur.IsDeleted)
            .FirstOrDefaultAsync();

        var roleName = userRole?.Role?.Name ?? "User";
        var roleCode = userRole?.Role?.Code ?? "USER";
        var roleId   = userRole?.RoleId.ToString() ?? "";

        // Build cookie claims
        var claims = new List<Claim>
        {
            new Claim("userId",      user.Id.ToString()),
            new Claim("companyId",   user.OrganizationId.ToString()),
            new Claim("fullName",    user.FullName),
            new Claim("initials",    user.Initials),
            new Claim("role",        roleName),
            new Claim("roleCode",    roleCode),
            new Claim("roleId",      roleId),
            new Claim("mustChangePassword", user.MustChangePassword ? "1" : "0"),
            new Claim(ClaimTypes.Name,  user.Username),
            new Claim(ClaimTypes.Email, user.Email),
            new Claim(ClaimTypes.Role,  roleCode),
        };

        var identity  = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        var principal = new ClaimsPrincipal(identity);

        await HttpContext.SignInAsync(
            CookieAuthenticationDefaults.AuthenticationScheme,
            principal,
            new AuthenticationProperties
            {
                IsPersistent = false,
                ExpiresUtc   = DateTimeOffset.UtcNow.AddHours(8)
            });

        // Record last login (fire-and-forget save)
        user.RecordLogin();
        await _db.SaveChangesAsync();

        // First login → force password change before anything else
        if (user.MustChangePassword)
            return RedirectToAction(nameof(ChangePassword));

        if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
            return Redirect(returnUrl);

        return RedirectToAction("Index", "Home");
    }

    // ── GET /Auth/ChangePassword ───────────────────────────────────
    [HttpGet, Authorize]
    public IActionResult ChangePassword() => View();

    // ── POST /Auth/ChangePassword ──────────────────────────────────
    [HttpPost, Authorize, ValidateAntiForgeryToken]
    public async Task<IActionResult> ChangePassword(string currentPassword, string newPassword, string confirmPassword)
    {
        if (newPassword != confirmPassword)
        {
            ModelState.AddModelError("", "New password and confirm password do not match.");
            return View();
        }

        if (newPassword.Length < 8)
        {
            ModelState.AddModelError("", "Password must be at least 8 characters.");
            return View();
        }

        var userIdStr = User.FindFirstValue("userId");
        if (!Guid.TryParse(userIdStr, out var userId))
            return RedirectToAction(nameof(Login));

        var user = await _db.AppUsers.FindAsync(userId);
        if (user == null || !BCrypt.Net.BCrypt.Verify(currentPassword, user.PasswordHash))
        {
            ModelState.AddModelError("", "Current password is incorrect.");
            return View();
        }

        user.SetPassword(BCrypt.Net.BCrypt.HashPassword(newPassword));
        await _db.SaveChangesAsync();

        // Claims were issued while MustChangePassword was set — sign in again with fresh ones.
        if (User.FindFirstValue("mustChangePassword") == "1")
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            TempData["Success"] = "Password changed. Please sign in with your new password.";
            return RedirectToAction(nameof(Login));
        }

        TempData["Success"] = "Password changed successfully.";
        return RedirectToAction("Index", "Home");
    }

    // ── GET /Auth/Unauthorised ─────────────────────────────────────
    [HttpGet]
    public IActionResult Unauthorised() => View();

    // ── GET /Auth/Logout ───────────────────────────────────────────
    [HttpGet]
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return RedirectToAction(nameof(Login));
    }
}
