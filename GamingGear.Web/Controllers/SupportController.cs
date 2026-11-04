using GamingGear.Application.Interfaces;
using GamingGear.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace GamingGear.Web.Controllers;

public class SupportController : Controller
{
    private readonly IRepository<SupportMessage> _supportRepo;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IEmailService _emailService;

    public SupportController(
        IRepository<SupportMessage> supportRepo,
        UserManager<ApplicationUser> userManager,
        IEmailService emailService)
    {
        _supportRepo = supportRepo;
        _userManager = userManager;
        _emailService = emailService;
    }

    // GET: /Support/Index (Giao diện dành cho Khách hàng)
    public async Task<IActionResult> Index()
    {
        var messages = await _supportRepo.GetAllAsync();
        return View(messages.OrderByDescending(m => m.CreatedAt).ToList());
    }

    // 🌟 1. CỔNG QUẢN TRỊ ADMIN TẬP TRUNG: /Support/AdminIndex
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> AdminIndex(string status = "all")
    {
        var messages = await _supportRepo.GetAllAsync();
        var query = messages.AsQueryable();

        if (status == "pending")
        {
            query = query.Where(m => !m.IsResolved);
        }
        else if (status == "resolved")
        {
            query = query.Where(m => m.IsResolved);
        }

        ViewBag.CurrentStatus = status;
        ViewBag.PendingCount = messages.Count(m => !m.IsResolved);
        ViewBag.TotalCount = messages.Count();

        return View(query.OrderByDescending(m => m.CreatedAt).ToList());
    }

    // 🌟 2. API LẤY SỐ CÂU HỎI CHƯA TRẢ LỜI (DÙNG CHO THÔNG BÁO BADGE/LỜI NHẮC REALTIME)
    [HttpGet]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> GetPendingCount()
    {
        var messages = await _supportRepo.GetAllAsync();
        var pendingCount = messages.Count(m => !m.IsResolved);
        return Json(new { count = pendingCount });
    }

    // POST: /Support/SendQuestion (Khách gửi câu hỏi)
    [HttpPost]
    [Authorize]
    public async Task<IActionResult> SendQuestion(string question)
    {
        if (string.IsNullOrWhiteSpace(question))
        {
            TempData["ErrorSupport"] = "Nội dung câu hỏi không được để trống!";
            return RedirectToAction(nameof(Index));
        }

        var user = await _userManager.GetUserAsync(User);
        var supportMsg = new SupportMessage
        {
            UserId = user?.Id ?? "",
            CustomerName = user?.FullName ?? user?.UserName ?? "Khách hàng",
            Question = question,
            CreatedAt = DateTime.Now,
            IsResolved = false
        };

        await _supportRepo.AddAsync(supportMsg);
        TempData["SuccessSupport"] = "Đã gửi câu hỏi thành công! Quản trị viên sẽ phản hồi sớm nhất.";
        return RedirectToAction(nameof(Index));
    }

    // POST: /Support/AdminReply (Admin trả lời câu hỏi & Tự động bắn Email thông báo)
    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> AdminReply(int id, string adminReply, string redirectSource = "AdminIndex")
    {
        var msg = await _supportRepo.GetByIdAsync(id);
        if (msg != null)
        {
            msg.AdminReply = adminReply;
            msg.IsResolved = true;
            await _supportRepo.UpdateAsync(msg);

            // 🚀 TIẾN HÀNH BẮN EMAIL TỰ ĐỘNG CHO KHÁCH HÀNG
            try
            {
                var customer = await _userManager.FindByIdAsync(msg.UserId);
                if (customer != null && !string.IsNullOrEmpty(customer.Email))
                {
                    string subject = "⚡ Phản hồi mới từ Trung tâm Hỗ trợ Ambition Karma Shop";
                    string htmlMessage = $@"
                        <div style='font-family: Arial, sans-serif; padding: 25px; background-color: #f8fafc; border-radius: 12px; border: 1px solid #e2e8f0;'>
                            <div style='text-align: center; margin-bottom: 20px;'>
                                <h2 style='color: #2563eb; margin: 0;'>Ambition Karma Shop ⚡</h2>
                                <p style='color: #64748b; font-size: 14px;'>Hệ thống Quản lý Thiết bị Gaming & Setup</p>
                            </div>
                            
                            <p>Xin chào <b>{customer.FullName ?? "Quý khách hàng"}</b>,</p>
                            <p>Yêu cầu hỗ trợ của bạn đã được Quản trị viên tiếp nhận và phản hồi:</p>
                            
                            <div style='background: #ffffff; padding: 20px; border-radius: 8px; border-left: 4px solid #2563eb; margin: 20px 0; box-shadow: 0 2px 4px rgba(0,0,0,0.02);'>
                                <p style='margin: 0 0 10px 0; color: #475569;'><b>💬 Câu hỏi của bạn:</b> {msg.Question}</p>
                                <p style='margin: 0; color: #1e293b;'><b>✨ Phản hồi từ Admin:</b> {adminReply}</p>
                            </div>
                            
                            <p style='font-size: 14px; color: #64748b;'>Bạn có thể truy cập lại website để xem chi tiết hoặc đặt thêm câu hỏi mới bất cứ lúc nào.</p>
                            
                            <hr style='border: none; border-top: 1px solid #e2e8f0; margin: 20px 0;' />
                            <p style='font-size: 12px; color: #94a3b8; text-align: center;'>Email tự động từ hệ thống Ambition Karma Shop. Vui lòng không trả lời trực tiếp email này.</p>
                        </div>";

                    await _emailService.SendEmailAsync(customer.Email, subject, htmlMessage);
                }
            }
            catch (Exception ex)
            {
                // Ghi log lỗi gửi mail nhưng không làm gián đoạn luồng xử lý của Admin
                Console.WriteLine($"Lỗi gửi email: {ex.Message}");
            }

            TempData["SuccessSupport"] = "Đã phản hồi và gửi Email thông báo thành công cho khách hàng!";
        }

        if (redirectSource == "CustomerIndex")
            return RedirectToAction(nameof(Index));

        return RedirectToAction(nameof(AdminIndex));
    }
}