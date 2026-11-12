// src/Microbanker.Web/Security/RequirePermissionAttribute.cs
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microbanker.Domain.Security;
using Microbanker.Web.Services;

namespace Microbanker.Web.Security;

/// <summary>
/// Secures a controller (or a single action) by permission-matrix module.
///
///   [RequirePermission(Modules.Users)]                            — class level
///   [RequirePermission(Modules.Users, PermissionActions.Delete)]  — action-level override
///
/// With no explicit action the permission is worked out from the request, and it fails closed:
///  • an action named Create*/Add* needs CREATE, Edit*/Update*/Save* EDIT, Delete*/Remove* DELETE,
///    Approve* APPROVE, Export* EXPORT;
///  • any other read (GET/HEAD) needs VIEW;
///  • any other write (POST, PUT, PATCH, DELETE) needs EDIT — never merely VIEW — so a new state-changing action
///    that nobody remembered to classify is still guarded by a write permission.
/// When both the class and the action carry the attribute, the action-level one wins; when the same action name
/// has a GET and a POST method, each method may carry its own.
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
        var action = Action ?? Derive(actionName, context.HttpContext.Request.Method);
        var perms  = await context.HttpContext.RequestServices
            .GetRequiredService<IPermissionService>().GetCurrentAsync();

        if (!perms.Can(Module, action))
            context.Result = new RedirectToActionResult("Unauthorised", "Auth", null);
    }

    /// <summary>The permission action an unclassified action needs. See the class summary.</summary>
    public static string Derive(string? actionName, string httpMethod)
    {
        var name = actionName ?? "";
        if (name.StartsWith("Create", StringComparison.OrdinalIgnoreCase) || name.StartsWith("Add", StringComparison.OrdinalIgnoreCase))
            return PermissionActions.Create;
        if (name.StartsWith("Edit", StringComparison.OrdinalIgnoreCase) || name.StartsWith("Update", StringComparison.OrdinalIgnoreCase)
            || name.StartsWith("Save", StringComparison.OrdinalIgnoreCase))
            return PermissionActions.Edit;
        if (name.StartsWith("Delete", StringComparison.OrdinalIgnoreCase) || name.StartsWith("Remove", StringComparison.OrdinalIgnoreCase))
            return PermissionActions.Delete;
        if (name.StartsWith("Approve", StringComparison.OrdinalIgnoreCase))
            return PermissionActions.Approve;
        if (name.StartsWith("Export", StringComparison.OrdinalIgnoreCase))
            return PermissionActions.Export;

        return HttpMethods.IsGet(httpMethod) || HttpMethods.IsHead(httpMethod) || HttpMethods.IsOptions(httpMethod)
            ? PermissionActions.View
            : PermissionActions.Edit;
    }
}
