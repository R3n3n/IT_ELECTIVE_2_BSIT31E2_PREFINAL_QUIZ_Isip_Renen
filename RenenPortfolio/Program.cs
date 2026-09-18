using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RenenPortfolio.Security;
using RenenPortfolio.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews(options =>
{
    // Every POST is checked for a valid antiforgery token, not just the ones
    // that remember the attribute.
    options.Filters.Add(new AutoValidateAntiforgeryTokenAttribute());
});

// ---- The single hardcoded account -----------------------------------------
var loginOptions = builder.Configuration.GetSection("PortfolioLogin").Get<LoginOptions>()
    ?? throw new InvalidOperationException("The PortfolioLogin section is missing from appsettings.json.");

builder.Services.AddSingleton(loginOptions);
builder.Services.AddSingleton<IUserService, HardcodedUserService>();
builder.Services.AddSingleton<ILoginThrottle, LoginThrottle>();

// ---- Portfolio data --------------------------------------------------------
builder.Services.AddSingleton<IProjectService, ProjectService>();
builder.Services.AddSingleton<ICommentService, CommentService>();

// ---- Authentication --------------------------------------------------------
builder.Services
    .AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Account/Login";
        options.LogoutPath = "/Account/Logout";
        options.AccessDeniedPath = "/Account/Login";
        options.ExpireTimeSpan = TimeSpan.FromHours(8);
        options.SlidingExpiration = true;

        options.Cookie.Name = "__Host-portfolio.auth";
        options.Cookie.HttpOnly = true;                            
        options.Cookie.SameSite = SameSiteMode.Strict;              
        options.Cookie.SecurePolicy = CookieSecurePolicy.Always;   
        options.Cookie.IsEssential = true;
    });

builder.Services.AddAuthorization(options =>
{
    options.FallbackPolicy = new AuthorizationPolicyBuilder()
        .RequireAuthenticatedUser()
        .Build();
});

builder.Services.AddAntiforgery(options =>
{
    options.Cookie.Name = "__Host-portfolio.csrf";
    options.Cookie.HttpOnly = true;
    options.Cookie.SameSite = SameSiteMode.Strict;
    options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
});

builder.Services.AddHsts(options =>
{
    options.MaxAge = TimeSpan.FromDays(365);
    options.IncludeSubDomains = true;
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}
else
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseStatusCodePagesWithReExecute("/Home/Error", "?statusCode={0}");

// ---- Security headers ------------------------------------------------------
app.Use(async (context, next) =>
{
    var headers = context.Response.Headers;
    headers["X-Content-Type-Options"] = "nosniff";
    headers["X-Frame-Options"] = "DENY";
    headers["Referrer-Policy"] = "strict-origin-when-cross-origin";
    headers["Permissions-Policy"] = "camera=(), microphone=(), geolocation=(), payment=()";
    headers["Content-Security-Policy"] =
        "default-src 'self'; " +
        "img-src 'self' data:; " +
        "style-src 'self' https://fonts.googleapis.com; " +
        "font-src 'self' https://fonts.gstatic.com; " +
        "script-src 'self'; " +
        "form-action 'self'; " +
        "frame-ancestors 'none'; " +
        "base-uri 'self'";
    await next();
});

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Projects}/{action=Index}/{id?}");

app.Run();