using GamingGear.Domain.Entities;
using GamingGear.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GamingGear.Web.Controllers;

[Authorize(Roles = "Admin")]
public class ProductManagementController : Controller
{
    private readonly IRepository<Product> _productRepository;
    private readonly IRepository<Category> _categoryRepository;

    public ProductManagementController(
        IRepository<Product> productRepository,
        IRepository<Category> categoryRepository)
    {
        _productRepository = productRepository;
        _categoryRepository = categoryRepository;
    }

    // ==========================================
    // 1. XEM DANH SÁCH SẢN PHẨM (INDEX)
    // ==========================================
    public async Task<IActionResult> Index()
    {
        var products = await _productRepository.GetAllAsync();
        return View(products);
    }

    // ==========================================
    // 2. THÊM MỚI SẢN PHẨM (CREATE)
    // ==========================================

    [HttpGet]
    public async Task<IActionResult> Create()
    {
        var categories = await _categoryRepository.GetAllAsync();
        ViewBag.Categories = categories;
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Product product)
    {
        if (ModelState.IsValid)
        {
            await _productRepository.AddAsync(product);
            return RedirectToAction(nameof(Index));
        }

        var categories = await _categoryRepository.GetAllAsync();
        ViewBag.Categories = categories;
        return View(product);
    }

    // ==========================================
    // 3. CHỈNH SỬA SẢN PHẨM (EDIT)
    // ==========================================

    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        var product = await _productRepository.GetByIdAsync(id);
        if (product == null)
        {
            return NotFound();
        }

        var categories = await _categoryRepository.GetAllAsync();
        ViewBag.Categories = categories;

        return View(product);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, Product product)
    {
        if (id != product.Id)
        {
            return BadRequest();
        }

        if (ModelState.IsValid)
        {
            try
            {
                await _productRepository.UpdateAsync(product);
                return RedirectToAction(nameof(Index));
            }
            catch (Exception)
            {
                ModelState.AddModelError("", "Không thể cập nhật sản phẩm. Vui lòng thử lại.");
            }
        }

        var categories = await _categoryRepository.GetAllAsync();
        ViewBag.Categories = categories;

        return View(product);
    }

    // ==========================================
    // 4. XÓA SẢN PHẨM (DELETE)
    // ==========================================

    // GET: /ProductManagement/Delete/5 (Hiển thị trang xác nhận trước khi xóa)
    [HttpGet]
    public async Task<IActionResult> Delete(int id)
    {
        var product = await _productRepository.GetByIdAsync(id);
        if (product == null)
        {
            return NotFound();
        }

        return View(product);
    }

    // POST: /ProductManagement/Delete/5 (Thực thi xóa khỏi Database)
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var product = await _productRepository.GetByIdAsync(id);
        if (product != null)
        {
            await _productRepository.DeleteAsync(id);
        }

        return RedirectToAction(nameof(Index));
    }
}