using GamingGear.Application.Interfaces;
using GamingGear.Domain.Entities;
using GamingGear.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GamingGear.Web.Controllers;

[Authorize(Roles = "Admin")]
public class AdminOrderController : Controller
{
    private readonly IRepository<Order> _orderRepo;
    private readonly IRepository<OrderDetail> _orderDetailRepo;
    private readonly IRepository<Product> _productRepo;
    private readonly ApplicationDbContext _context;

    public AdminOrderController(
        IRepository<Order> orderRepo,
        IRepository<OrderDetail> orderDetailRepo,
        IRepository<Product> productRepo,
        ApplicationDbContext context)
    {
        _orderRepo = orderRepo;
        _orderDetailRepo = orderDetailRepo;
        _productRepo = productRepo;
        _context = context;
    }

    // GET: /AdminOrder
    public async Task<IActionResult> Index()
    {
        var orders = await _orderRepo.GetAllAsync();
        var sortedOrders = orders.OrderByDescending(o => o.OrderDate).ToList();
        return View(sortedOrders);
    }

    // GET: /AdminOrder/Details/5
    public async Task<IActionResult> Details(int id)
    {
        var order = await _context.Orders
            .Include(o => o.OrderDetails)
                .ThenInclude(od => od.Product)
            .FirstOrDefaultAsync(o => o.Id == id);

        if (order == null) return NotFound();

        ViewBag.OrderDetails = order.OrderDetails.ToList();
        ViewBag.Products = order.OrderDetails
            .Where(od => od.Product != null)
            .ToDictionary(od => od.ProductId, od => od.Product!);

        return View(order);
    }

    // ⚡ Action in hóa đơn cho Admin
    // GET: /AdminOrder/PrintInvoice/5
    public async Task<IActionResult> PrintInvoice(int id)
    {
        var order = await _context.Orders
            .Include(o => o.OrderDetails)
                .ThenInclude(od => od.Product)
            .FirstOrDefaultAsync(o => o.Id == id);

        if (order == null) return NotFound();

        return View(order);
    }

    // POST: /AdminOrder/UpdateStatus
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateStatus(int id, string status)
    {
        var order = await _orderRepo.GetByIdAsync(id);
        if (order != null)
        {
            string oldStatus = order.Status;

            // Nếu đơn hàng chuyển sang trạng thái "Cancelled" (Đã hủy) từ trạng thái khác -> Hoàn trả tồn kho
            if (status == "Cancelled" && oldStatus != "Cancelled")
            {
                var orderDetails = await _context.OrderDetails
                    .Where(d => d.OrderId == id)
                    .ToListAsync();

                foreach (var detail in orderDetails)
                {
                    var product = await _productRepo.GetByIdAsync(detail.ProductId);
                    if (product != null)
                    {
                        product.StockQuantity += detail.Quantity;
                        await _productRepo.UpdateAsync(product);
                    }
                }
            }

            order.Status = status;
            await _orderRepo.UpdateAsync(order);
            TempData["SuccessMessage"] = $"Đã cập nhật trạng thái đơn hàng #{id} thành công!";
        }
        return RedirectToAction("Details", new { id = id });
    }
}