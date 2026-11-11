// src/Microbanker.Web/Program.cs
using Microbanker.Infrastructure.Persistence;
using Microbanker.Web.Services;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// ── MVC ──────────────────────────────────────────────────────────────────────
builder.Services.AddControllersWithViews();

// ── Database ─────────────────────────────────────────────────────────────────
builder.Services.AddDbContext<MicrobankerDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("MicrobankerConnection")));

// ── Authentication ────────────────────────────────────────────────────────────
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath       = "/Auth/Login";
        options.AccessDeniedPath = "/Auth/Login";
        options.ExpireTimeSpan  = TimeSpan.FromHours(8);
        options.SlidingExpiration = true;
    });

// ── Services ─────────────────────────────────────────────────────────────────
builder.Services.AddSingleton<IEncryptionService, AesEncryptionService>();
builder.Services.AddScoped<IPostingService, PostingService>();
builder.Services.AddMemoryCache();
builder.Services.AddHttpContextAccessor();

var app = builder.Build();

// ── Seed database ─────────────────────────────────────────────────────────────
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<MicrobankerDbContext>();
    await DbSeeder.SeedAsync(db);
}

// ── Pipeline ──────────────────────────────────────────────────────────────────
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
