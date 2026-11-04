namespace GamingGear.Domain.Entities;

public class SupportMessage
{
    public int Id { get; set; }
    public string UserId { get; set; } = string.Empty;
    public string CustomerName { get; set; } = string.Empty;
    public string Question { get; set; } = string.Empty; // Khách hỏi
    public string? AdminReply { get; set; }               // Admin trả lời
    public DateTime CreatedAt { get; set; } = DateTime.Now;
    public bool IsResolved { get; set; } = false;         // Trạng thái đã trả lời hay chưa
}