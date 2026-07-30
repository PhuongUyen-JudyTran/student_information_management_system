using Microsoft.AspNetCore.Authentication.Cookies;
using SIMS.Application;
using SIMS.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();

// Add Infrastructure & Application Services via DI extension methods
builder.Services.AddInfrastructureServices();
builder.Services.AddApplicationServices();

// Configure Cookie Authentication
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Account/Login";
        options.LogoutPath = "/Account/Logout";
        options.AccessDeniedPath = "/Account/AccessDenied";
        options.ExpireTimeSpan = TimeSpan.FromMinutes(30);
        options.SlidingExpiration = true;
    });

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapRazorPages();

// Seed default administrator if admin username is not found
using (var scope = app.Services.CreateScope())
{
    var authService = scope.ServiceProvider.GetRequiredService<AuthService>();
    var userRepo = scope.ServiceProvider.GetRequiredService<SIMS.Domain.Interfaces.IUserRepository>();
    var adminUser = await userRepo.GetByUsernameAsync("admin");
    if (adminUser == null)
    {
        await authService.RegisterUserAsync(
            "admin", 
            "admin123", 
            "admin@university.edu", 
            SIMS.Domain.Entities.UserRole.Administrator, 
            DateTime.Today, 
            null, 
            null, 
            "SuperAdmin"
        );
    }
}

app.Run();
