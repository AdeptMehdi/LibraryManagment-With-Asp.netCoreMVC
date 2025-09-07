using LibraryManagement;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using LibraryManagement.Models;
using LibraryManagement.Data;
using LibraryManagement.Seed;
var builder = WebApplication.CreateBuilder(args);

// DbContext و Identity
builder.Services.AddDbContext<LibraryContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddDefaultIdentity<ApplicationUser>(options =>
    {
        options.SignIn.RequireConfirmedAccount = false; // اگر خواستی تایید ایمیل فعال باشه
    })
    .AddRoles<IdentityRole>() // برای Role Management
    .AddEntityFrameworkStores<LibraryContext>()
    .AddDefaultUI(); // فعال کردن UI آماده

builder.Services.AddControllersWithViews();

var app = builder.Build();
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    await SeedData.CreateRolesAndAdmin(services);

}


if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication(); // Identity
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");
app.MapRazorPages(); // مهم برای Identity UI

app.Run();