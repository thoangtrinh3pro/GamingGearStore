using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GamingGear.Domain.Entities
{
    public class Feedback
    {
        public int Id { get; set; }

        [Required]
        public int ProductId { get; set; }
        [ForeignKey("ProductId")]
        public virtual Product? Product { get; set; }

        // Người gửi feedback (Khách hàng)
        public string? UserId { get; set; }
        [ForeignKey("UserId")]
        public virtual ApplicationUser? User { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập nội dung đánh giá!")]
        [StringLength(1000)]
        public string Comment { get; set; } = string.Empty;

        [Range(1, 5, ErrorMessage = "Đánh giá sao từ 1 đến 5!")]
        public int Rating { get; set; } = 5;

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        // 💬 Phản hồi từ phía Admin
        public string? AdminReply { get; set; }
        public DateTime? RepliedAt { get; set; }

        // ✨ Thuộc tính phụ trợ hiển thị tên khách hàng (Bỏ qua ánh xạ CSDL)
        [NotMapped]
        public string CustomerName => User?.FullName ?? User?.UserName ?? "Khách vãng lai";
    }
}