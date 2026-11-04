using GamingGear.Application.Interfaces;
using GamingGear.Domain.Entities;
using GamingGear.Web.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace GamingGear.Web.Controllers;

public class AccountController : Controller
{
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;
    private readonly IEmailService _emailService;

    public AccountController(
        SignInManager<ApplicationUser> signInManager,
        UserManager<ApplicationUser> userManager,
        RoleManager<IdentityRole> roleManager,
        IEmailService emailService)
    {
        _signInManager = signInManager;
        _userManager = userManager;
        _roleManager = roleManager;
        _emailService = emailService;
    }

    // GET: /Account/Login
    [HttpGet]
    public IActionResult Login(string? returnUrl = null)
    {
        ViewData["ReturnUrl"] = returnUrl;
        return View();
    }

    // POST: /Account/Login
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginViewModel model, string? returnUrl = null)
    {
        ViewData["ReturnUrl"] = returnUrl;

        if (!ModelState.IsValid)
        {
            return View(model);
        }

        // Đăng nhập bằng SignInManager của Identity
        var result = await _signInManager.PasswordSignInAsync(
            model.Email,
            model.Password,
            model.RememberMe,
            lockoutOnFailure: false);

        if (result.Succeeded)
        {
            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }
            return RedirectToAction("Index", "Home");
        }

        ModelState.AddModelError(string.Empty, "Tài khoản hoặc mật khẩu không chính xác!");
        return View(model);
    }

    // GET: /Account/Register
    [HttpGet]
    public IActionResult Register()
    {
        return View();
    }

    // POST: /Account/Register (Gửi mã OTP xác thực qua Email)
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Register(RegisterViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        // Kiểm tra xem Email đã tồn tại trong hệ thống chưa
        var existingUser = await _userManager.FindByEmailAsync(model.Email);
        if (existingUser != null)
        {
            ModelState.AddModelError("Email", "Email này đã được sử dụng bởi tài khoản khác!");
            return View(model);
        }

        // Tạo mã OTP ngẫu nhiên gồm 6 chữ số
        var random = new Random();
        string otpCode = random.Next(100000, 999999).ToString();

        // Lưu tạm thông tin đăng ký và mã OTP vào Session (Hiệu lực trong 5 phút)
        HttpContext.Session.SetString("Register_FullName", model.FullName ?? "");
        HttpContext.Session.SetString("Register_Email", model.Email);
        HttpContext.Session.SetString("Register_Password", model.Password);
        HttpContext.Session.SetString("Register_OTP", otpCode);
        HttpContext.Session.SetString("Register_OTP_ExpireTime", DateTime.Now.AddMinutes(5).ToString());

        // Tiến hành gửi Email chứa mã OTP
        try
        {
            string subject = "🔐 Mã OTP Xác Thực Đăng Ký Tài Khoản - Ambition Karma Shop";
            string htmlMessage = $@"
                <div style='font-family: Arial, sans-serif; padding: 25px; background-color: #f8fafc; border-radius: 12px; border: 1px solid #e2e8f0;'>
                    <div style='text-align: center; margin-bottom: 20px;'>
                        <h2 style='color: #2563eb; margin: 0;'>Ambition Karma Shop ⚡</h2>
                        <p style='color: #64748b; font-size: 14px;'>Xác thực đăng ký tài khoản mới</p>
                    </div>
                    
                    <p>Xin chào <b>{model.FullName}</b>,</p>
                    <p>Cảm ơn bạn đã đăng ký tài khoản tại Ambition Karma Shop. Đây là mã OTP xác thực của bạn:</p>
                    
                    <div style='text-align: center; margin: 30px 0;'>
                        <span style='background: #2563eb; color: #ffffff; font-size: 32px; font-weight: bold; letter-spacing: 8px; padding: 12px 30px; border-radius: 10px; display: inline-block;'>{otpCode}</span>
                    </div>
                    
                    <p style='color: #ef4444; font-size: 13px; text-align: center;'>⚠️ Mã OTP này có hiệu lực trong vòng <b>5 phút</b>. Vui lòng không chia sẻ mã này cho bất kỳ ai.</p>
                    
                    <hr style='border: none; border-top: 1px solid #e2e8f0; margin: 20px 0;' />
                    <p style='font-size: 12px; color: #94a3b8; text-align: center;'>Email tự động từ hệ thống Ambition Karma Shop.</p>
                </div>";

            await _emailService.SendEmailAsync(model.Email, subject, htmlMessage);
        }
        catch (Exception)
        {
            ModelState.AddModelError(string.Empty, "Lỗi không thể gửi email OTP. Vui lòng thử lại sau!");
            return View(model);
        }

        // Chuyển hướng sang trang nhập mã OTP
        return RedirectToAction(nameof(VerifyOtp), new { email = model.Email });
    }

    // GET: /Account/VerifyOtp
    [HttpGet]
    public IActionResult VerifyOtp(string email)
    {
        var model = new VerifyOtpViewModel { Email = email ?? string.Empty };
        return View(model);
    }

    // POST: /Account/VerifyOtp (Xác thực mã OTP và chính thức tạo tài khoản)
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> VerifyOtp(VerifyOtpViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        string savedOtp = HttpContext.Session.GetString("Register_OTP") ?? "";
        string savedEmail = HttpContext.Session.GetString("Register_Email") ?? "";
        string expireTimeString = HttpContext.Session.GetString("Register_OTP_ExpireTime") ?? "";

        // Kiểm tra thời gian hết hạn của OTP
        if (DateTime.TryParse(expireTimeString, out DateTime expireTime) && DateTime.Now > expireTime)
        {
            ModelState.AddModelError("OtpCode", "Mã OTP đã hết hạn! Vui lòng thực hiện đăng ký lại.");
            return View(model);
        }

        // Kiểm tra tính chính xác của mã OTP và Email
        if (model.OtpCode != savedOtp || model.Email != savedEmail)
        {
            ModelState.AddModelError("OtpCode", "Mã OTP không chính xác. Vui lòng kiểm tra lại!");
            return View(model);
        }

        // Lấy thông tin đã lưu tạm trong Session để tạo User thật
        string fullName = HttpContext.Session.GetString("Register_FullName") ?? "";
        string password = HttpContext.Session.GetString("Register_Password") ?? "";

        var user = new ApplicationUser
        {
            UserName = savedEmail,
            Email = savedEmail,
            FullName = fullName,
            EmailConfirmed = true,
            CreatedAt = DateTime.UtcNow
        };

        var result = await _userManager.CreateAsync(user, password);
        if (result.Succeeded)
        {
            // Kiểm tra nếu chưa có role "User" thì tạo mới
            if (!await _roleManager.RoleExistsAsync("User"))
            {
                await _roleManager.CreateAsync(new IdentityRole("User"));
            }

            // Gán quyền "User" cho tài khoản
            await _userManager.AddToRoleAsync(user, "User");

            // Tự động đăng nhập luôn sau khi xác thực thành công
            await _signInManager.SignInAsync(user, isPersistent: false);

            // Dọn dẹp Session sau khi hoàn tất
            HttpContext.Session.Remove("Register_OTP");
            HttpContext.Session.Remove("Register_Email");
            HttpContext.Session.Remove("Register_Password");
            HttpContext.Session.Remove("Register_FullName");
            HttpContext.Session.Remove("Register_OTP_ExpireTime");

            return RedirectToAction("Index", "Home");
        }

        foreach (var error in result.Errors)
        {
            ModelState.AddModelError(string.Empty, error.Description);
        }

        return View(model);
    }

    // POST: /Account/Logout
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        await _signInManager.SignOutAsync();
        return RedirectToAction("Index", "Home");
    }

    // GET: /Account/AccessDenied
    public IActionResult AccessDenied()
    {
        return View();
    }
}