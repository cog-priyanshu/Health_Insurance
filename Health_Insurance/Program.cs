using Health_Insurance.Data;
using Health_Insurance.Models; // Added if needed for any Program.cs logic, though less common
using Health_Insurance.Services; // Ensure this using statement is present
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

// Configure the database context to use SQL Server
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Register the Enrollment Service for dependency injection (if not already there)
builder.Services.AddScoped<IEnrollmentService, EnrollmentService>();

// Register the Premium Calculator Service for dependency injection (if not already there)
builder.Services.AddScoped<IPremiumCalculatorService, PremiumCalculatorService>();

// Register the Claim Service for dependency injection
builder.Services.AddScoped<IClaimService, ClaimService>(); // Add this line

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

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();


