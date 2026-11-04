using GamingGear.Domain.Entities;
using GamingGear.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GamingGear.Web.Controllers;

public class FeedbackController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;

    public FeedbackController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    // POST: /Feedback/Create
    [HttpPost]
    [Authorize]
    public async Task<IActionResult> Create(int productId, int rating, string comment)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null)
        {
            return RedirectToAction("Login", "Account");
        }

        // 🌟 KIỂM TRA ĐIỀU KIỆN: Khách phải có đơn hàng ĐÃ HOÀN THÀNH chứa sản phẩm này
        // (Lưu ý: Nếu bảng Order của ông dùng CustomerId thì sửa o.UserId thành o.CustomerId)
        bool hasPurchasedAndCompleted = await _context.Orders
            .Include(o => o.OrderDetails)
            .AnyAsync(o => o.UserId == user.Id &&
                           (o.Status == "Completed" || o.Status == "Đã hoàn thành" || o.Status == "Hoàn thành") &&
                           o.OrderDetails.Any(od => od.ProductId == productId));

        if (!hasPurchasedAndCompleted)
        {
            TempData["ErrorFeedback"] = "🚫 Bạn cần phải mua sản phẩm này và đơn hàng phải ở trạng thái 'Đã hoàn thành' thì mới được quyền viết đánh giá!";
            return RedirectToAction("Details", "Product", new { id = productId });
        }

        // Tạo feedback mới (Đã bỏ CustomerName vì thuộc tính này là read-only)
        var feedback = new Feedback
        {
            ProductId = productId,
            UserId = user.Id,
            Rating = rating,
            Comment = comment,
            CreatedAt = DateTime.Now
        };

        _context.Feedbacks.Add(feedback);
        await _context.SaveChangesAsync();

        TempData["SuccessFeedback"] = "✅ Gửi đánh giá thành công! Cảm ơn phản hồi của bạn.";
        return RedirectToAction("Details", "Product", new { id = productId });
    }

    // POST: /Feedback/AdminReply
    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> AdminReply(int feedbackId, string adminReply, int productId)
    {
        var feedback = await _context.Feedbacks.FindAsync(feedbackId);
        if (feedback != null)
        {
            feedback.AdminReply = adminReply;
            _context.Update(feedback);
            await _context.SaveChangesAsync();
            TempData["SuccessFeedback"] = "⚡ Đã gửi phản hồi đánh giá thành công!";
        }
        else
        {
            TempData["ErrorFeedback"] = "Không tìm thấy đánh giá cần phản hồi!";
        }

        return RedirectToAction("Details", "Product", new { id = productId });
    }
}