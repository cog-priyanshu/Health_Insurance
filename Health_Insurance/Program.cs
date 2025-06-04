// Program.cs
using Health_Insurance.Data;
using Health_Insurance.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.Cookies;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

// Configure the database context to use SQL Server
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Register your custom UserService for dependency injection
builder.Services.AddScoped<IUserService, UserService>();

// Register other services (ensure these are already present)
builder.Services.AddScoped<IEnrollmentService, EnrollmentService>();
builder.Services.AddScoped<IPremiumCalculatorService, PremiumCalculatorService>();
builder.Services.AddScoped<IClaimService, ClaimService>();


// --- Configure Authentication Services ---
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Account/Login"; // Specifies the path to the login page
        options.LogoutPath = "/Account/Logout"; // Specifies the path to the logout action
        options.AccessDeniedPath = "/Account/AccessDenied"; // Specifies the path for access denied
        options.ExpireTimeSpan = TimeSpan.FromMinutes(30); // Cookie expiration time
        options.SlidingExpiration = true; // Renew cookie if half of expiration time has passed
    });
// --- End Configure Authentication Services ---


var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

// --- Add Authentication and Authorization Middleware ---
// These must be placed between UseRouting() and MapControllerRoute()
app.UseAuthentication(); // Must be before UseAuthorization
app.UseAuthorization();
// --- End Authentication and Authorization Middleware ---

app.MapControllerRoute(
    name: "default",
    // Change the default controller and action to point to your Login page
    pattern: "{controller=Account}/{action=Login}/{id?}"); // Changed default route

app.Run();


