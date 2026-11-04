using GamingGear.Application.Interfaces;
using GamingGear.Domain.Entities;
using GamingGear.Infrastructure.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace GamingGear.Web.Controllers;

public class OrderController : Controller
{
    private readonly IRepository<Order> _orderRepo;
    private readonly IRepository<OrderDetail> _orderDetailRepo;
    private readonly IRepository<Product> _productRepo;
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;

    public OrderController(
        IRepository<Order> orderRepo,
        IRepository<OrderDetail> orderDetailRepo,
        IRepository<Product> productRepo,
        ApplicationDbContext context,
        UserManager<ApplicationUser> userManager)
    {
        _orderRepo = orderRepo;
        _orderDetailRepo = orderDetailRepo;
        _productRepo = productRepo;
        _context = context;
        _userManager = userManager;
    }

    // GET: /Order (Trang Lịch sử / Tra cứu đơn hàng của khách)
    public async Task<IActionResult> Index(string searchTerm)
    {
        var allOrders = await _orderRepo.GetAllAsync();
        List<Order> orders = allOrders.OrderByDescending(o => o.OrderDate).ToList();

        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            searchTerm = searchTerm.Trim();
            orders = orders.Where(o =>
                o.Id.ToString() == searchTerm ||
                (!string.IsNullOrEmpty(o.PhoneNumber) && o.PhoneNumber.Contains(searchTerm)) ||
                (!string.IsNullOrEmpty(o.FullName) && o.FullName.Contains(searchTerm, StringComparison.OrdinalIgnoreCase))
            ).ToList();
            ViewBag.SearchTerm = searchTerm;
        }

        return View(orders);
    }

    // GET: /Order/Lookup (Giữ lại route cũ để tương thích)
    public IActionResult Lookup()
    {
        return RedirectToAction(nameof(Index));
    }

    // POST: /Order/Search 
    [HttpPost]
    public IActionResult Search(string searchTerm)
    {
        return RedirectToAction(nameof(Index), new { searchTerm });
    }

    // GET: /Order/Details/5 (Xem chi tiết đơn hàng)
    public async Task<IActionResult> Details(int id)
    {
        var order = await _orderRepo.GetByIdAsync(id);
        if (order == null) return NotFound();

        var allDetails = await _orderDetailRepo.GetAllAsync();
        var orderDetails = allDetails.Where(d => d.OrderId == id).ToList();

        var products = await _productRepo.GetAllAsync();
        ViewBag.Products = products.ToDictionary(p => p.Id, p => p);
        ViewBag.OrderDetails = orderDetails;

        // ⚡ Lấy đánh giá THỰC TẾ của user phát sinh SAU KHI ĐẶT ĐƠN HÀNG NÀY (Bỏ qua đống Seed Data cũ)
        var user = await _userManager.GetUserAsync(User);
        if (user != null)
        {
            var productIds = orderDetails.Select(d => d.ProductId).ToList();

            // Chỉ lấy các feedback của user cho sản phẩm trong đơn và được tạo SAU HOẶC BẰNG ngày đặt đơn
            var actualReviews = _context.Feedbacks
                .Where(f => f.UserId == user.Id
                         && productIds.Contains(f.ProductId)
                         && f.CreatedAt >= order.OrderDate)
                .ToList();

            var reviewedProductIds = actualReviews.Select(f => f.ProductId).Distinct().ToList();

            var reviewDetails = actualReviews
                .GroupBy(f => f.ProductId)
                .ToDictionary(g => g.Key, g => g.First());

            ViewBag.ReviewedProductIds = reviewedProductIds;
            ViewBag.UserReviews = reviewDetails;
        }
        else
        {
            ViewBag.ReviewedProductIds = new List<int>();
            ViewBag.UserReviews = new Dictionary<int, Feedback>();
        }

        return View(order);
    }

    // POST: /Order/CancelOrder (Khách hàng hủy đơn hàng có điều kiện)
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CancelOrder(int id, string cancelReason)
    {
        var order = await _orderRepo.GetByIdAsync(id);
        if (order == null)
        {
            return NotFound();
        }

        // Chỉ cho phép hủy khi đơn hàng ở trạng thái "Pending"
        if (order.Status != "Pending")
        {
            TempData["ErrorMessage"] = "Đơn hàng đã được xử lý hoặc đang vận chuyển, không thể tự hủy. Nếu cần hỗ trợ gấp, vui lòng liên hệ qua Zalo hoặc SĐT: 0373900143.";
            return RedirectToAction("Details", new { id = order.Id });
        }

        if (string.IsNullOrWhiteSpace(cancelReason))
        {
            TempData["ErrorMessage"] = "Vui lòng nhập lý do hủy đơn hàng!";
            return RedirectToAction("Details", new { id = order.Id });
        }

        order.Status = "Cancelled";
        order.CancelReason = cancelReason;
        await _orderRepo.UpdateAsync(order);

        // Tự động hoàn lại số lượng tồn kho
        var allDetails = await _orderDetailRepo.GetAllAsync();
        var orderDetails = allDetails.Where(d => d.OrderId == order.Id).ToList();

        foreach (var detail in orderDetails)
        {
            var product = await _productRepo.GetByIdAsync(detail.ProductId);
            if (product != null)
            {
                product.StockQuantity += detail.Quantity;
                await _productRepo.UpdateAsync(product);
            }
        }

        TempData["SuccessMessage"] = $"Đã hủy đơn hàng #{order.Id} thành công và hoàn trả kho!";
        return RedirectToAction("Details", new { id = order.Id });
    }

    // ⚡ POST: /Order/SubmitReview (Gửi đánh giá sản phẩm khi đơn hoàn tất)
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SubmitReview(int orderId, int productId, int rating, string comment)
    {
        var order = await _orderRepo.GetByIdAsync(orderId);
        if (order == null || (order.Status != "Completed" && order.Status != "Hoàn tất"))
        {
            TempData["ErrorMessage"] = "Đơn hàng chưa hoàn tất hoặc không hợp lệ để đánh giá!";
            return RedirectToAction("Details", new { id = orderId });
        }

        if (string.IsNullOrWhiteSpace(comment))
        {
            TempData["ErrorMessage"] = "Vui lòng nhập nội dung đánh giá!";
            return RedirectToAction("Details", new { id = orderId });
        }

        var user = await _userManager.GetUserAsync(User);
        if (user == null)
        {
            TempData["ErrorMessage"] = "Vui lòng đăng nhập lại để thực hiện đánh giá!";
            return RedirectToAction("Details", new { id = orderId });
        }

        // ⚡ Kiểm tra xem user đã đánh giá sản phẩm NÀY cho ĐƠN HÀNG NÀY chưa (tính từ sau OrderDate)
        bool alreadyReviewed = _context.Feedbacks.Any(f => f.UserId == user.Id
                                                        && f.ProductId == productId
                                                        && f.CreatedAt >= order.OrderDate);
        if (alreadyReviewed)
        {
            TempData["ErrorMessage"] = "Bạn đã đánh giá sản phẩm này cho đơn hàng này rồi!";
            return RedirectToAction("Details", new { id = orderId });
        }

        var feedback = new Feedback
        {
            ProductId = productId,
            UserId = user.Id,
            Rating = rating,
            Comment = comment.Trim(),
            CreatedAt = DateTime.Now
        };

        _context.Feedbacks.Add(feedback);
        await _context.SaveChangesAsync();

        TempData["SuccessMessage"] = "Cảm ơn bạn! Đánh giá sản phẩm đã được ghi nhận và hiển thị công khai.";
        return RedirectToAction("Details", new { id = orderId });
    }
}