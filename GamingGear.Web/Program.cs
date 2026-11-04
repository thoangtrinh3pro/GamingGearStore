using GamingGear.Application.Interfaces;
using GamingGear.Application.Models;
using GamingGear.Domain.Entities;
using GamingGear.Infrastructure.Data;
using GamingGear.Infrastructure.Repositories;
using GamingGear.Infrastructure.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// 1. Lấy Chuỗi kết nối SQL
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

// 2. Đăng ký DbContext
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));

// 3. Đăng ký Repository cho Dependency Injection
builder.Services.AddScoped(typeof(IRepository<>), typeof(Repository<>));

// --- 4. CẤU HÌNH EMAIL SERVICE (SMTP) ---
builder.Services.Configure<EmailSettings>(builder.Configuration.GetSection("EmailSettings"));
builder.Services.AddTransient<IEmailService, EmailService>();
// ---------------------------------------

// 5. Đăng ký Identity (Phân quyền)
builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
{
    options.Password.RequireDigit = false;
    options.Password.RequireLowercase = false;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireUppercase = false;
    options.Password.RequiredLength = 6;
})
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddDefaultTokenProviders();

// --- 6. CẤU HÌNH SESSION CHO GIỎ HÀNG ---
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});
// ---------------------------------------

// 7. Add MVC
builder.Services.AddControllersWithViews();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

// --- BẬT SESSION MIDDLEWARE ---
app.UseSession();
// -----------------------------

// Bật tính năng xác thực và phân quyền
app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

// 8. TỰ ĐỘNG SEED DATA & ĐỒNG BỘ TÀI KHOẢN CHÍNH CHỦ KHI KHỞI ĐỘNG
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<ApplicationDbContext>();
        var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
        var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();

        // Gọi Seeder gốc của dự án
        await DbInitializer.SeedAsync(context, userManager, roleManager);

        // Đảm bảo Role cơ bản tồn tại
        if (!await roleManager.RoleExistsAsync("Admin"))
            await roleManager.CreateAsync(new IdentityRole("Admin"));
        if (!await roleManager.RoleExistsAsync("User"))
            await roleManager.CreateAsync(new IdentityRole("User"));

        // 1. Đồng bộ / Khởi tạo tài khoản ADMIN (thoangtrinh3@gmail.com)
        var adminUser = await userManager.FindByEmailAsync("thoangtrinh3@gmail.com");
        if (adminUser != null)
        {
            var token = await userManager.GeneratePasswordResetTokenAsync(adminUser);
            await userManager.ResetPasswordAsync(adminUser, token, "123456789");
            if (!await userManager.IsInRoleAsync(adminUser, "Admin"))
                await userManager.AddToRoleAsync(adminUser, "Admin");
        }
        else
        {
            var newAdmin = new ApplicationUser
            {
                UserName = "thoangtrinh3@gmail.com",
                Email = "thoangtrinh3@gmail.com",
                EmailConfirmed = true,
                FullName = "Trịnh Quang Thoang (Admin)"
            };
            var result = await userManager.CreateAsync(newAdmin, "123456789");
            if (result.Succeeded) await userManager.AddToRoleAsync(newAdmin, "Admin");
        }

        // 2. Đồng bộ / Khởi tạo tài khoản USER test (hn3508777@gmail.com)
        var testUser = await userManager.FindByEmailAsync("hn3508777@gmail.com");
        if (testUser != null)
        {
            var token = await userManager.GeneratePasswordResetTokenAsync(testUser);
            await userManager.ResetPasswordAsync(testUser, token, "123456789");

            if (!await userManager.IsInRoleAsync(testUser, "User"))
            {
                await userManager.AddToRoleAsync(testUser, "User");
            }
        }
        else
        {
            var newUser = new ApplicationUser
            {
                UserName = "hn3508777@gmail.com",
                Email = "hn3508777@gmail.com",
                EmailConfirmed = true,
                FullName = "Rô Bin Sôn (Customer)"
            };
            var result = await userManager.CreateAsync(newUser, "123456789");
            if (result.Succeeded) await userManager.AddToRoleAsync(newUser, "User");
        }
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "Lỗi xảy ra khi Seed Data hoặc đồng bộ tài khoản!");
    }
}

app.Run();