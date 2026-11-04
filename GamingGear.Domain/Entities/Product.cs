using System.ComponentModel.DataAnnotations.Schema;

namespace GamingGear.Domain.Entities;

public class Product
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public decimal Price { get; set; }
    public int StockQuantity { get; set; }
    public string? ImageUrl { get; set; }

    // Khóa ngoại liên kết Danh mục
    public int CategoryId { get; set; }
    public Category? Category { get; set; }

    // Quan hệ 1-N với Feedback
    public virtual ICollection<Feedback> Feedbacks { get; set; } = new List<Feedback>();

    // 🌟 THUỘC TÍNH TÍNH TOÁN ĐIỂM SỐ SAO TRUNG BÌNH (Không tạo cột trong DB)
    [NotMapped]
    public double AverageRating
    {
        get
        {
            if (Feedbacks == null || !Feedbacks.Any())
                return 0.0;
            return Math.Round(Feedbacks.Average(f => f.Rating), 1);
        }
    }

    // 🌟 TỔNG SỐ LƯỢNG ĐÁNH GIÁ
    [NotMapped]
    public int TotalReviews => Feedbacks?.Count ?? 0;
}