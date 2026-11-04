using GamingGear.Domain.Entities;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace GamingGear.Infrastructure.Data;

public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<Category> Categories { get; set; }
    public DbSet<Product> Products { get; set; }
    public DbSet<Order> Orders { get; set; }
    public DbSet<OrderDetail> OrderDetails { get; set; }
    public DbSet<Feedback> Feedbacks { get; set; }
    public DbSet<SupportMessage> SupportMessages { get; set; } // 👈 Lưu tin nhắn hỗ trợ
    public DbSet<Promotion> Promotions { get; set; } // 👈 Thêm DbSet quản lý mã giảm giá
    public DbSet<Review> Reviews => Set<Review>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        // Cấu hình relationship hoặc constraint
        builder.Entity<Product>()
            .Property(p => p.Price)
            .HasColumnType("decimal(18,2)");

        builder.Entity<Order>()
            .Property(o => o.TotalAmount)
            .HasColumnType("decimal(18,2)");

        builder.Entity<OrderDetail>()
            .Property(od => od.UnitPrice)
            .HasColumnType("decimal(18,2)");

        // ⚡ Cấu hình kiểu dữ liệu decimal cho mã giảm giá
        builder.Entity<Promotion>()
            .Property(pr => pr.DiscountAmount)
            .HasColumnType("decimal(18,2)");
    }
}