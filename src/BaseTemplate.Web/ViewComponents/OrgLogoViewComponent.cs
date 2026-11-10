// src/BaseTemplate.Web/ViewComponents/OrgLogoViewComponent.cs
using BaseTemplate.Infrastructure.Persistence;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace BaseTemplate.Web.ViewComponents;

public class OrgLogoViewComponent : ViewComponent
{
    public const string CacheKey = "OrgLogo";

    private readonly AppDbContext _db;
    private readonly IMemoryCache  _cache;

    public OrgLogoViewComponent(AppDbContext db, IMemoryCache cache)
    {
        _db    = db;
        _cache = cache;
    }

    public async Task<IViewComponentResult> InvokeAsync()
    {
        var org = await _cache.GetOrCreateAsync(CacheKey, async entry =>
        {
            entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5);
            return await _db.Organizations
                .Where(o => !o.IsDeleted)
                .Select(o => new OrgLogoModel(o.Name, o.Logo))
                .FirstOrDefaultAsync();
        });

        return View(org);
    }
}

public record OrgLogoModel(string Name, string? Logo);
