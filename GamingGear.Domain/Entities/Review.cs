namespace GamingGear.Domain.Entities;

public class Review
{
    public int Id { get; set; }
    public int ProductId { get; set; }
    public string UserId { get; set; } = string.Empty;
    public string UserName { get; set; } = string.Empty;
    public int Rating { get; set; } = 5; // Số sao (1 đến 5)
    public string Comment { get; set; } = string.Empty; // Nội dung đánh giá
    public DateTime CreatedAt { get; set; } = DateTime.Now;
}