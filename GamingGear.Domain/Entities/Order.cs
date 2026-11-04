namespace GamingGear.Domain.Entities;

public class Order
{
    public int Id { get; set; }
    public DateTime OrderDate { get; set; } = DateTime.Now;

    public string UserId { get; set; } = string.Empty;
    public ApplicationUser? User { get; set; }

    // Thông tin người nhận hàng
    public string FullName { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public string ShippingAddress { get; set; } = string.Empty;

    public decimal TotalAmount { get; set; }
    public string Status
    {
        get; set; } = "Pending"; // Pending, Shipping, Completed, Cancelled

    // Thông tin khuyến mãi & phí ship
    public string? CouponCode { get; set; }
    public decimal DiscountAmount { get; set; }
    public decimal ShippingFee { get; set; }

    // ⚡ BỔ SUNG TRƯỜNG NÀY ĐỂ LƯU PHƯƠNG THỨC THANH TOÁN (COD hoặc VNPAY/QR)
    public string PaymentMethod { get; set; } = "COD";

    // Quan hệ 1-N với Chi tiết đơn hàng
    public ICollection<OrderDetail> OrderDetails { get; set; } = new List<OrderDetail>();
    public string? CancelReason { get; set; }
}