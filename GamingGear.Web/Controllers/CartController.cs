using CartItem = GamingGear.Web.Models.CartItem;
using GamingGear.Application.Interfaces;
using GamingGear.Domain.Entities;
using GamingGear.Infrastructure.Data;
using GamingGear.Web.Helpers;
using GamingGear.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace GamingGear.Web.Controllers;

[Authorize]
public class CartController : Controller
{
    private readonly IRepository<Product> _productRepo;
    private readonly IRepository<Order> _orderRepo;
    private readonly IRepository<OrderDetail> _orderDetailRepo;
    private readonly IConfiguration _configuration;
    private readonly IEmailService _emailService;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ApplicationDbContext _context;

    private const string CartSessionKey = "CartSession";
    private const string BuyNowSessionKey = "BuyNowSession";
    private const string CouponCodeSessionKey = "CouponCodeSession";
    private const string DiscountAmountSessionKey = "DiscountSession";

    public CartController(
        IRepository<Product> productRepo,
        IRepository<Order> orderRepo,
        IRepository<OrderDetail> orderDetailRepo,
        IConfiguration configuration,
        IEmailService emailService,
        UserManager<ApplicationUser> userManager,
        ApplicationDbContext context)
    {
        _productRepo = productRepo;
        _orderRepo = orderRepo;
        _orderDetailRepo = orderDetailRepo;
        _configuration = configuration;
        _emailService = emailService;
        _userManager = userManager;
        _context = context;
    }

    private IActionResult? CheckAdminRestriction()
    {
        if (User.Identity != null && User.Identity.IsAuthenticated && User.IsInRole("Admin"))
        {
            TempData["ErrorMessage"] = "Tài khoản Quản trị viên không được phép thực hiện chức năng mua hàng!";
            return RedirectToAction("Index", "Product");
        }
        return null;
    }

    public IActionResult Index()
    {
        var adminCheck = CheckAdminRestriction();
        if (adminCheck != null) return adminCheck;

        var cart = HttpContext.Session.GetObjectFromJson<List<CartItem>>(CartSessionKey) ?? new List<CartItem>();
        ViewBag.TotalAmount = cart.Sum(item => item.TotalPrice);
        return View(cart);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult ApplyCoupon(string couponCode)
    {
        var adminCheck = CheckAdminRestriction();
        if (adminCheck != null) return adminCheck;

        if (string.IsNullOrWhiteSpace(couponCode))
        {
            TempData["ErrorMessage"] = "Vui lòng nhập mã giảm giá!";
            return Redirect(Request.Headers["Referer"].ToString() ?? Url.Action("Index"));
        }

        // ⚡ Lấy danh sách sản phẩm linh hoạt từ cả Giỏ hàng hoặc Mua Ngay
        var cart = HttpContext.Session.GetObjectFromJson<List<CartItem>>(CartSessionKey) ?? new List<CartItem>();
        if (!cart.Any())
        {
            cart = HttpContext.Session.GetObjectFromJson<List<CartItem>>(BuyNowSessionKey) ?? new List<CartItem>();
        }

        decimal subtotal = cart.Sum(x => x.TotalPrice);

        string code = couponCode.Trim().ToUpper();

        var promotion = _context.Promotions.FirstOrDefault(p => p.Code == code && p.IsActive);

        if (promotion == null)
        {
            TempData["ErrorMessage"] = "Mã giảm giá không tồn tại hoặc đã bị khóa!";
            return Redirect(Request.Headers["Referer"].ToString() ?? Url.Action("Index"));
        }

        if (promotion.ExpiryDate.HasValue && promotion.ExpiryDate.Value < DateTime.Now)
        {
            TempData["ErrorMessage"] = $"Mã giảm giá '{code}' đã hết hạn sử dụng (Hạn: {promotion.ExpiryDate.Value:dd/MM/yyyy HH:mm})!";
            return Redirect(Request.Headers["Referer"].ToString() ?? Url.Action("Index"));
        }

        if (subtotal < promotion.MinOrderAmount)
        {
            TempData["ErrorMessage"] = $"Đơn hàng phải đạt tối thiểu {promotion.MinOrderAmount:N0} đ mới được áp dụng mã này!";
            return Redirect(Request.Headers["Referer"].ToString() ?? Url.Action("Index"));
        }

        decimal discountValue = promotion.DiscountAmount;

        HttpContext.Session.SetString(CouponCodeSessionKey, code);
        HttpContext.Session.SetObjectAsJson(DiscountAmountSessionKey, discountValue);
        TempData["SuccessMessage"] = $"Áp dụng mã '{code}' thành công (Giảm {discountValue:N0} đ)!";

        return Redirect(Request.Headers["Referer"].ToString() ?? Url.Action("Index"));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult RemoveCoupon()
    {
        var adminCheck = CheckAdminRestriction();
        if (adminCheck != null) return adminCheck;

        HttpContext.Session.Remove(CouponCodeSessionKey);
        HttpContext.Session.Remove(DiscountAmountSessionKey);

        TempData["SuccessMessage"] = "Đã hủy mã giảm giá. Đơn hàng đã được tính toán lại về giá gốc!";

        return Redirect(Request.Headers["Referer"].ToString() ?? Url.Action("Index"));
    }

    [HttpGet]
    public IActionResult AddToCart() => RedirectToAction("Index", "Product");

    [HttpPost]
    public async Task<IActionResult> AddToCart(int productId, int quantity = 1)
    {
        var adminCheck = CheckAdminRestriction();
        if (adminCheck != null) return adminCheck;

        var product = await _productRepo.GetByIdAsync(productId);
        if (product == null) return NotFound();

        if (product.StockQuantity <= 0)
        {
            TempData["ErrorMessage"] = "Sản phẩm này hiện đã hết hàng!";
            return RedirectToAction("Index", "Product");
        }

        var cart = HttpContext.Session.GetObjectFromJson<List<CartItem>>(CartSessionKey) ?? new List<CartItem>();
        var cartItem = cart.FirstOrDefault(p => p.ProductId == productId);

        if (cartItem == null)
        {
            cart.Add(new CartItem
            {
                ProductId = product.Id,
                ProductName = product.Name,
                Price = product.Price,
                Quantity = quantity,
                ImageUrl = product.ImageUrl
            });
        }
        else
        {
            cartItem.Quantity += quantity;
        }

        HttpContext.Session.SetObjectAsJson(CartSessionKey, cart);
        return RedirectToAction("Index");
    }

    [HttpGet]
    public IActionResult BuyNow() => RedirectToAction("Index", "Product");

    [HttpPost]
    public async Task<IActionResult> BuyNow(int productId, int quantity = 1)
    {
        var adminCheck = CheckAdminRestriction();
        if (adminCheck != null) return adminCheck;

        var product = await _productRepo.GetByIdAsync(productId);
        if (product == null) return NotFound();

        if (product.StockQuantity <= 0)
        {
            TempData["ErrorMessage"] = "Sản phẩm này hiện đã hết hàng!";
            return RedirectToAction("Index", "Product");
        }

        var buyNowItem = new List<CartItem>
        {
            new CartItem
            {
                ProductId = product.Id,
                ProductName = product.Name,
                Price = product.Price,
                Quantity = quantity,
                ImageUrl = product.ImageUrl
            }
        };

        HttpContext.Session.SetObjectAsJson(BuyNowSessionKey, buyNowItem);
        return RedirectToAction("Checkout", new { isBuyNow = true });
    }

    [HttpPost]
    public async Task<IActionResult> UpdateQuantity(int productId, int quantity)
    {
        var adminCheck = CheckAdminRestriction();
        if (adminCheck != null) return adminCheck;

        var cart = HttpContext.Session.GetObjectFromJson<List<CartItem>>(CartSessionKey) ?? new List<CartItem>();
        var cartItem = cart.FirstOrDefault(p => p.ProductId == productId);

        if (cartItem != null)
        {
            var product = await _productRepo.GetByIdAsync(productId);
            int maxStock = product != null ? product.StockQuantity : int.MaxValue;

            if (quantity > 0)
            {
                if (quantity <= maxStock)
                {
                    cartItem.Quantity = quantity;
                }
                else
                {
                    TempData["ErrorMessage"] = $"Sản phẩm '{cartItem.ProductName}' chỉ còn lại {maxStock} sản phẩm!";
                }
            }
            else
            {
                cart.Remove(cartItem);
            }

            HttpContext.Session.SetObjectAsJson(CartSessionKey, cart);
        }

        return RedirectToAction("Index");
    }

    public IActionResult Remove(int id)
    {
        var adminCheck = CheckAdminRestriction();
        if (adminCheck != null) return adminCheck;

        var cart = HttpContext.Session.GetObjectFromJson<List<CartItem>>(CartSessionKey) ?? new List<CartItem>();
        var cartItem = cart.FirstOrDefault(p => p.ProductId == id);
        if (cartItem != null)
        {
            cart.Remove(cartItem);
            HttpContext.Session.SetObjectAsJson(CartSessionKey, cart);
        }
        return RedirectToAction("Index");
    }

    public IActionResult Checkout(bool isBuyNow = false)
    {
        var adminCheck = CheckAdminRestriction();
        if (adminCheck != null) return adminCheck;

        List<CartItem> itemsToPay = isBuyNow
            ? HttpContext.Session.GetObjectFromJson<List<CartItem>>(BuyNowSessionKey) ?? new List<CartItem>()
            : HttpContext.Session.GetObjectFromJson<List<CartItem>>(CartSessionKey) ?? new List<CartItem>();

        if (!itemsToPay.Any()) itemsToPay = HttpContext.Session.GetObjectFromJson<List<CartItem>>(CartSessionKey) ?? new List<CartItem>();
        if (!itemsToPay.Any()) itemsToPay = HttpContext.Session.GetObjectFromJson<List<CartItem>>(BuyNowSessionKey) ?? new List<CartItem>();

        if (!itemsToPay.Any()) return RedirectToAction("Index");

        ViewBag.IsBuyNow = isBuyNow;
        return View(new CheckoutViewModel { CartItems = itemsToPay });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Checkout(CheckoutViewModel model, string paymentMethod = "COD", bool isBuyNow = false)
    {
        var adminCheck = CheckAdminRestriction();
        if (adminCheck != null) return adminCheck;

        string sessionKey = isBuyNow ? BuyNowSessionKey : CartSessionKey;
        var cart = HttpContext.Session.GetObjectFromJson<List<CartItem>>(sessionKey) ?? new List<CartItem>();

        if (!cart.Any()) cart = HttpContext.Session.GetObjectFromJson<List<CartItem>>(CartSessionKey) ?? new List<CartItem>();
        if (!cart.Any()) cart = HttpContext.Session.GetObjectFromJson<List<CartItem>>(BuyNowSessionKey) ?? new List<CartItem>();

        if (!cart.Any())
        {
            ModelState.AddModelError(string.Empty, "Giỏ hàng của bạn đang trống hoặc phiên làm việc đã hết hạn.");
            ViewBag.IsBuyNow = isBuyNow;
            model.CartItems = new List<CartItem>();
            return View(model);
        }

        if (!ModelState.IsValid)
        {
            ViewBag.IsBuyNow = isBuyNow;
            model.CartItems = cart;
            return View(model);
        }

        var subtotal = cart.Sum(x => x.TotalPrice);

        var couponCode = HttpContext.Session.GetString(CouponCodeSessionKey);
        var discountAmount = HttpContext.Session.GetObjectFromJson<decimal>(DiscountAmountSessionKey);
        var shippingFee = subtotal >= 1000000 ? 0 : 49000;

        var finalTotal = (subtotal - discountAmount) + shippingFee;
        if (finalTotal < 0) finalTotal = 0;

        var user = await _userManager.GetUserAsync(User);

        var order = new Order
        {
            UserId = user?.Id ?? string.Empty,
            OrderDate = DateTime.Now,
            FullName = model.FullName,
            PhoneNumber = model.PhoneNumber,
            ShippingAddress = model.Address,
            TotalAmount = finalTotal,
            Status = paymentMethod == "VNPAY" ? "Pending_Payment" : "Pending",

            CouponCode = couponCode,
            DiscountAmount = discountAmount,
            ShippingFee = shippingFee,
            PaymentMethod = paymentMethod
        };

        _context.Orders.Add(order);
        await _context.SaveChangesAsync();

        foreach (var item in cart)
        {
            _context.OrderDetails.Add(new OrderDetail
            {
                OrderId = order.Id,
                ProductId = item.ProductId,
                Quantity = item.Quantity,
                UnitPrice = item.Price
            });

            var product = await _productRepo.GetByIdAsync(item.ProductId);
            if (product != null)
            {
                product.StockQuantity -= item.Quantity;
                await _productRepo.UpdateAsync(product);
            }
        }

        await _context.SaveChangesAsync();

        HttpContext.Session.Remove(sessionKey);
        HttpContext.Session.Remove(CartSessionKey);
        HttpContext.Session.Remove(BuyNowSessionKey);
        HttpContext.Session.Remove(CouponCodeSessionKey);
        HttpContext.Session.Remove(DiscountAmountSessionKey);

        if (paymentMethod == "COD")
        {
            await SendOrderConfirmationEmail(user, model.FullName, order, "COD", subtotal, shippingFee, discountAmount);
            return RedirectToAction("OrdersSuccess", new { id = order.Id });
        }

        return RedirectToAction("QrPayment", new { orderId = order.Id });
    }

    public async Task<IActionResult> QrPayment(int orderId)
    {
        var adminCheck = CheckAdminRestriction();
        if (adminCheck != null) return adminCheck;

        var order = await _orderRepo.GetByIdAsync(orderId);
        if (order == null) return NotFound();

        ViewBag.BankCode = _configuration["BankInfo:BankCode"] ?? "TCB";
        ViewBag.AccountNumber = _configuration["BankInfo:AccountNumber"] ?? "1488111111";
        ViewBag.AccountName = _configuration["BankInfo:AccountName"] ?? "TRINH QUANG THOANG";

        return View(order);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SwitchToCod(int orderId)
    {
        var adminCheck = CheckAdminRestriction();
        if (adminCheck != null) return adminCheck;

        var order = await _orderRepo.GetByIdAsync(orderId);
        if (order == null) return NotFound();

        order.Status = "Pending";
        order.PaymentMethod = "COD";
        await _orderRepo.UpdateAsync(order);

        var user = await _userManager.GetUserAsync(User);

        decimal subtotal = order.TotalAmount >= 1000000 ? order.TotalAmount : order.TotalAmount - 49000;
        decimal shippingFee = order.TotalAmount >= 1000000 ? 0 : 49000;

        await SendOrderConfirmationEmail(user, order.FullName, order, "COD (Thanh toán khi nhận hàng)", subtotal, shippingFee, 0);

        TempData["SuccessMessage"] = "Đã chuyển phương thức thanh toán sang COD và gửi email xác nhận thành công!";
        return RedirectToAction("OrdersSuccess", new { id = order.Id });
    }

    public IActionResult OrdersSuccess(int id)
    {
        var adminCheck = CheckAdminRestriction();
        if (adminCheck != null) return adminCheck;

        ViewBag.OrderId = id;
        return View();
    }

    private async Task SendOrderConfirmationEmail(ApplicationUser? user, string fallbackName, Order order, string paymentMethodName, decimal subtotal, decimal shippingFee, decimal discountAmount)
    {
        try
        {
            string emailRecipient = user?.Email ?? string.Empty;
            if (!string.IsNullOrEmpty(emailRecipient))
            {
                string subject = $"⚡ [Ambition Karma Shop] Đặt hàng thành công! (Mã đơn: #{order.Id})";
                string htmlMessage = $@"
                    <div style='font-family: Arial, sans-serif; padding: 25px; background-color: #f8fafc; border-radius: 12px; border: 1px solid #e2e8f0;'>
                        <div style='text-align: center; margin-bottom: 20px;'>
                            <h2 style='color: #2563eb; margin: 0;'>Ambition Karma Shop ⚡</h2>
                            <p style='color: #64748b; font-size: 14px;'>Xác nhận đơn hàng thiết bị Gaming</p>
                        </div>
                        
                        <p>Xin chào <b>{user?.FullName ?? fallbackName}</b>,</p>
                        <p>Cảm ơn bạn đã tin tưởng và mua sắm tại <b>Ambition Karma Shop</b>! Đơn hàng của bạn đã được ghi nhận thành công.</p>
                        
                        <div style='background: #ffffff; padding: 20px; border-radius: 8px; border: 1px solid #e2e8f0; margin: 20px 0;'>
                            <p style='margin: 0 0 8px 0; color: #475569;'>📦 <b>Mã đơn hàng:</b> #{order.Id}</p>
                            <p style='margin: 0 0 8px 0; color: #475569;'>📅 <b>Thời gian đặt:</b> {order.OrderDate:dd/MM/yyyy HH:mm}</p>
                            <p style='margin: 0 0 8px 0; color: #475569;'>💳 <b>Phương thức thanh toán:</b> {paymentMethodName}</p>
                            <p style='margin: 0 0 8px 0; color: #475569;'>💵 <b>Tạm tính tiền hàng:</b> {subtotal:N0} VNĐ</p>
                            {(discountAmount > 0 ? $"<p style='margin: 0 0 8px 0; color: #16a34a;'>🏷️ <b>Giảm giá:</b> -{discountAmount:N0} VNĐ</p>" : "")}
                            <p style='margin: 0 0 8px 0; color: #475569;'>🚚 <b>Phí vận chuyển:</b> {(shippingFee == 0 ? "Miễn phí" : $"{shippingFee:N0} VNĐ")}</p>
                            <p style='margin: 0 0 8px 0; color: #475569;'>💰 <b>Tổng tiền thanh toán:</b> <span style='color: #dc2626; font-weight: bold;'>{order.TotalAmount:N0} VNĐ</span></p>
                            <p style='margin: 0; color: #475569;'>📍 <b>Địa chỉ nhận hàng:</b> {order.ShippingAddress}</p>
                        </div>
                        
                        <p style='font-size: 14px; color: #64748b;'>Chúng tôi sẽ bàn giao đơn vị vận chuyển sớm nhất.</p>
                        <hr style='border: none; border-top: 1px solid #e2e8f0; margin: 20px 0;' />
                        <p style='font-size: 12px; color: #94a3b8; text-align: center;'>Email tự động từ hệ thống Ambition Karma Shop.</p>
                    </div>";

                await _emailService.SendEmailAsync(emailRecipient, subject, htmlMessage);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Lỗi gửi mail đơn hàng: {ex.Message}");
        }
    }
}