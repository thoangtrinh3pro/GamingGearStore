using GamingGear.Application.Interfaces;
using GamingGear.Domain.Entities;
using GamingGear.Infrastructure.Data; // Thêm namespace context nếu cần
using Microsoft.AspNetCore.Mvc;

namespace GamingGear.Web.Controllers;

public class ProductController : Controller
{
    private readonly IRepository<Product> _productRepo;
    private readonly IRepository<Category> _categoryRepo;
    private readonly IRepository<Feedback> _feedbackRepo;

    public ProductController(
        IRepository<Product> productRepo,
        IRepository<Category> categoryRepo,
        IRepository<Feedback> feedbackRepo)
    {
        _productRepo = productRepo;
        _categoryRepo = categoryRepo;
        _feedbackRepo = feedbackRepo;
    }

    // GET: /Product/Index
    public async Task<IActionResult> Index(int? categoryId, string searchString, string sortOrder)
    {
        var products = await _productRepo.GetAllAsync();
        var categories = await _categoryRepo.GetAllAsync();
        var allFeedbacks = await _feedbackRepo.GetAllAsync();

        foreach (var product in products)
        {
            product.Feedbacks = allFeedbacks.Where(f => f.ProductId == product.Id).ToList();
        }

        if (categoryId.HasValue)
        {
            products = products.Where(p => p.CategoryId == categoryId.Value).ToList();
        }

        if (!string.IsNullOrWhiteSpace(searchString))
        {
            products = products.Where(p => p.Name.Contains(searchString, StringComparison.OrdinalIgnoreCase) ||
                                           (p.Description != null && p.Description.Contains(searchString, StringComparison.OrdinalIgnoreCase))).ToList();
        }

        ViewBag.CurrentSort = sortOrder;
        ViewBag.CurrentSearch = searchString;
        ViewBag.SelectedCategoryId = categoryId;

        products = sortOrder switch
        {
            "price_asc" => products.OrderBy(p => p.Price).ToList(),
            "price_desc" => products.OrderByDescending(p => p.Price).ToList(),
            "name_az" => products.OrderBy(p => p.Name).ToList(),
            "name_za" => products.OrderByDescending(p => p.Name).ToList(),
            _ => products.OrderByDescending(p => p.Id).ToList(),
        };

        ViewBag.Categories = categories;
        return View(products);
    }

    // GET: /Product/Details/5
    public async Task<IActionResult> Details(int id)
    {
        var product = await _productRepo.GetByIdAsync(id);
        if (product == null)
        {
            return NotFound();
        }

        // 💬 Lấy chính xác danh sách Feedback thuộc về sản phẩm này
        var allFeedbacks = await _feedbackRepo.GetAllAsync();
        product.Feedbacks = allFeedbacks.Where(f => f.ProductId == id).OrderByDescending(f => f.CreatedAt).ToList();

        return View(product);
    }
}