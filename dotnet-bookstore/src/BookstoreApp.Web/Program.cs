using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Identity.Web;
using Microsoft.Identity.Web.UI;
using BookstoreApp.Data;
using BookstoreApp.Web.Services.Interfaces;
using BookstoreApp.Web.Services.Implementations;

var builder = WebApplication.CreateBuilder(args);

// Add Application Insights (only if connection string is configured)
var appInsightsConnStr = builder.Configuration.GetValue<string>("ApplicationInsights:ConnectionString");
if (!string.IsNullOrEmpty(appInsightsConnStr) && !appInsightsConnStr.Contains("<YOUR"))
{
    builder.Services.AddApplicationInsightsTelemetry();
}

// Check if we're using local auth (for development without Entra ID)
var useLocalAuth = builder.Configuration.GetValue<bool>("UseLocalAuth");

if (useLocalAuth)
{
    // Simple cookie authentication for local development
    builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
        .AddCookie(options =>
        {
            options.LoginPath = "/LocalAuth/Login";
            options.LogoutPath = "/LocalAuth/Logout";
            options.Cookie.Name = "BookstoreAuth";
        });
    
    builder.Services.AddControllersWithViews();
}
else
{
    // Production: Entra ID authentication
    builder.Services.AddAuthentication(OpenIdConnectDefaults.AuthenticationScheme)
        .AddMicrosoftIdentityWebApp(builder.Configuration.GetSection("AzureAd"));

    builder.Services.AddControllersWithViews()
        .AddMicrosoftIdentityUI();
}

// Add Razor Pages (for Identity UI)
builder.Services.AddRazorPages();

// Add DbContext
builder.Services.AddDbContext<BookstoreDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("BookstoreDb"),
        sqlOptions => sqlOptions.EnableRetryOnFailure(3)
    ));

// Add session support (for shopping cart)
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
    options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
});

// Register services
builder.Services.AddScoped<IBookService, BookService>();
builder.Services.AddScoped<ICartService, CartService>();
builder.Services.AddScoped<IOrderService, OrderService>();
builder.Services.AddScoped<ICustomerService, CustomerService>();

// Add health checks
builder.Services.AddHealthChecks()
    .AddSqlServer(builder.Configuration.GetConnectionString("BookstoreDb") ?? "");

var app = builder.Build();

// Configure the HTTP request pipeline
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

app.UseSession();

// Health check endpoint
app.MapHealthChecks("/health");

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.MapRazorPages();

// Apply migrations on startup (development only)
if (app.Environment.IsDevelopment())
{
    using var scope = app.Services.CreateScope();
    var dbContext = scope.ServiceProvider.GetRequiredService<BookstoreDbContext>();
    
    // Check if database exists and has pending migrations
    try
    {
        var pendingMigrations = dbContext.Database.GetPendingMigrations();
        if (pendingMigrations.Any())
        {
            dbContext.Database.Migrate();
        }
    }
    catch (Exception ex)
    {
        // If migrations fail (e.g., database exists but no migration history),
        // try EnsureCreated as fallback
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
        logger.LogWarning(ex, "Migration failed, attempting EnsureCreated...");
        dbContext.Database.EnsureCreated();
    }
}

app.Run();
