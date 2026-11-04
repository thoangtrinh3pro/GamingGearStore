namespace GamingGear.Domain.Entities;

public class Promotion
{
    public int Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public decimal DiscountAmount { get; set; }        // Số tiền giảm
    public decimal MinOrderAmount { get; set; } = 0;   // 👈 Điều kiện: Giá trị đơn hàng tối thiểu
    public DateTime? ExpiryDate { get; set; }          // 👈 Thời hạn sử dụng (có thể để trống nếu không hết hạn)
    public bool IsActive { get; set; } = true;         // Trạng thái kích hoạt (Bật/Tắt)
}