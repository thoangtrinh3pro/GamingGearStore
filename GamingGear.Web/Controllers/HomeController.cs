using System.Diagnostics;
using GamingGear.Application.Interfaces;
using GamingGear.Domain.Entities;
using GamingGear.Web.Models;
using Microsoft.AspNetCore.Mvc;

namespace GamingGear.Web.Controllers;

public class HomeController : Controller
{
    private readonly IRepository<Product> _productRepo;
    private readonly IRepository<Category> _categoryRepo;
    private readonly ILogger<HomeController> _logger;

    public HomeController(
        IRepository<Product> productRepo,
        IRepository<Category> categoryRepo,
        ILogger<HomeController> logger)
    {
        _productRepo = productRepo;
        _categoryRepo = categoryRepo;
        _logger = logger;
    }

    // 1. TRANG CHỦ (Hiển thị TOÀN BỘ sản phẩm trong DB)
    public async Task<IActionResult> Index()
    {
        var products = await _productRepo.GetAllAsync();
        var categories = await _categoryRepo.GetAllAsync();

        ViewBag.Categories = categories;

        // Trả về toàn bộ danh sách sản phẩm (không dùng Take để tránh bị gò bó)
        return View(products.ToList());
    }

    // 2. TÌM KIẾM SẢN PHẨM
    public async Task<IActionResult> Search(string query)
    {
        var products = await _productRepo.GetAllAsync();

        if (!string.IsNullOrWhiteSpace(query))
        {
            query = query.ToLower().Trim();
            products = products.Where(p =>
                p.Name.ToLower().Contains(query) ||
                (p.Description != null && p.Description.ToLower().Contains(query))
            ).ToList();
        }

        ViewBag.SearchQuery = query;
        return View("SearchResults", products);
    }

    // 3. TRANG GIỚI THIỆU THƯƠNG HIỆU
    public IActionResult About()
    {
        ViewData["Title"] = "Giới thiệu thương hiệu";
        return View();
    }

    // 4. TRANG LIÊN HỆ
    public IActionResult Contact()
    {
        ViewData["Title"] = "Liên hệ";
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Contact(string fullName, string email, string message)
    {
        TempData["SuccessMessage"] = "Cảm ơn bạn đã gửi liên hệ! Ambition Karma Shop sẽ phản hồi trong vòng 24h.";
        return RedirectToAction(nameof(Contact));
    }

    // 5. CHÍNH SÁCH BẢO MẬT
    public IActionResult Privacy()
    {
        return View();
    }

    // 6. XỬ LÝ LỖI HỆ THỐNG
    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}