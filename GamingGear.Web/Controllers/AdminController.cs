using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using GamingGear.Infrastructure.Data;
using GamingGear.Domain.Entities;

namespace GamingGear.Web.Controllers;

[Authorize(Roles = "Admin")]
public class AdminController : Controller
{
    private readonly ApplicationDbContext _context;

    public AdminController(ApplicationDbContext context)
    {
        _context = context;
    }

    public IActionResult Index()
    {
        return View();
    }

    // GET: /Admin/Promotions
    public IActionResult Promotions()
    {
        var promotions = _context.Promotions.ToList();
        return View(promotions);
    }

    // POST: /Admin/CreatePromotion
    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult CreatePromotion(string code, decimal discountAmount, decimal minOrderAmount, DateTime? expiryDate)
    {
        if (string.IsNullOrWhiteSpace(code))
        {
            TempData["ErrorMessage"] = "Mã khuyến mãi không được để trống!";
            return RedirectToAction("Promotions");
        }

        string upperCode = code.Trim().ToUpper();
        var existing = _context.Promotions.FirstOrDefault(p => p.Code == upperCode);
        if (existing != null)
        {
            TempData["ErrorMessage"] = "Mã khuyến mãi này đã tồn tại trong hệ thống!";
            return RedirectToAction("Promotions");
        }

        _context.Promotions.Add(new Promotion
        {
            Code = upperCode,
            DiscountAmount = discountAmount,
            MinOrderAmount = minOrderAmount,
            ExpiryDate = expiryDate,
            IsActive = true
        });
        _context.SaveChanges();

        TempData["SuccessMessage"] = $"Đã tạo mã giảm giá '{upperCode}' thành công!";
        return RedirectToAction("Promotions");
    }

    // POST: /Admin/DeletePromotion
    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult DeletePromotion(int id)
    {
        var promotion = _context.Promotions.Find(id);
        if (promotion != null)
        {
            _context.Promotions.Remove(promotion);
            _context.SaveChanges();
            TempData["SuccessMessage"] = "Đã xóa mã giảm giá thành công!";
        }
        return RedirectToAction("Promotions");
    }
}