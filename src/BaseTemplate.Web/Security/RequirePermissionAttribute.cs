// src/BaseTemplate.Web/Security/RequirePermissionAttribute.cs
using BaseTemplate.Web.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace BaseTemplate.Web.Security;

/// <summary>
/// Secures a controller (or a single action) by permission-matrix module.
///
///   [RequirePermission(Modules.Users)]                           — class level
///   [RequirePermission(Modules.Users, PermissionActions.Export)] — action-level override
///
/// With no explicit action code the action is derived from the action name:
/// Create*/Add* → CREATE, Edit*/Update*/Save*/SendTest* → EDIT, Delete*/Remove* → DELETE,
/// Approve* → APPROVE, Export* → EXPORT, everything else → VIEW.
/// When both class and action carry the attribute, the action-level one wins.
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false)]
public sealed class RequirePermissionAttribute : Attribute, IAsyncAuthorizationFilter
{
    public string Module { get; }
    public string? Action { get; }

    public RequirePermissionAttribute(string module, string? action = null)
    {
        Module = module;
        Action = action;
    }

    public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
    {
        // Class and method both register this filter; only the most specific one decides.
        var mostSpecific = context.ActionDescriptor.EndpointMetadata
            .OfType<RequirePermissionAttribute>().LastOrDefault();
        if (!ReferenceEquals(mostSpecific, this)) return;

        if (context.HttpContext.User.Identity?.IsAuthenticated != true) return; // [Authorize] handles it

        context.ActionDescriptor.RouteValues.TryGetValue("action", out var actionName);
        var action = Action ?? Derive(actionName);
        var perms  = await context.HttpContext.RequestServices
            .GetRequiredService<IPermissionService>().GetCurrentAsync();

        if (!perms.Can(Module, action))
            context.Result = new RedirectToActionResult("Unauthorised", "Auth", null);
    }

    private static string Derive(string? name)
    {
        name ??= "";
        if (name.StartsWith("Create") || name.StartsWith("Add"))
            return PermissionActions.Create;
        if (name.StartsWith("Edit") || name.StartsWith("Update") || name.StartsWith("Save") || name.StartsWith("SendTest"))
            return PermissionActions.Edit;
        if (name.StartsWith("Delete") || name.StartsWith("Remove"))
            return PermissionActions.Delete;
        if (name.StartsWith("Approve"))
            return PermissionActions.Approve;
        if (name.StartsWith("Export"))
            return PermissionActions.Export;
        return PermissionActions.View;
    }
}
