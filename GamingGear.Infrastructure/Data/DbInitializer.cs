using GamingGear.Domain.Entities;
using Microsoft.AspNetCore.Identity;

namespace GamingGear.Infrastructure.Data;

public static class DbInitializer
{
    public static async Task SeedAsync(ApplicationDbContext context, UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager)
    {
        await context.Database.EnsureCreatedAsync();

        // 1. Tạo các Role cơ bản
        string[] roleNames = { "Admin", "User", "Customer" };
        foreach (var roleName in roleNames)
        {
            if (!await roleManager.RoleExistsAsync(roleName))
            {
                await roleManager.CreateAsync(new IdentityRole(roleName));
            }
        }

        // 2. Seed tài khoản Admin (thoangtrinh3@gmail.com)
        var adminEmail = "thoangtrinh3@gmail.com";
        var adminUser = await userManager.FindByEmailAsync(adminEmail);

        if (adminUser == null)
        {
            adminUser = new ApplicationUser
            {
                UserName = adminEmail,
                Email = adminEmail,
                FullName = "Trịnh Quang Thoang (Admin)",
                EmailConfirmed = true,
                CreatedAt = DateTime.UtcNow
            };

            var result = await userManager.CreateAsync(adminUser, "123456789");
            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(adminUser, "Admin");
            }
        }

        // 3. Seed tài khoản User (hn3508777@gmail.com)
        var userEmail = "hn3508777@gmail.com";
        var testUser = await userManager.FindByEmailAsync(userEmail);

        if (testUser == null)
        {
            testUser = new ApplicationUser
            {
                UserName = userEmail,
                Email = userEmail,
                FullName = "Rô Bin Sôn (Customer)",
                EmailConfirmed = true,
                CreatedAt = DateTime.UtcNow
            };

            var result = await userManager.CreateAsync(testUser, "123456789");
            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(testUser, "User");
            }
        }

        // 4. Seed Danh mục mẫu
        if (!context.Categories.Any())
        {
            var categories = new List<Category>
            {
                new Category { Name = "Bàn phím cơ", Description = "Bàn phím cơ Custom & Gaming cao cấp" },
                new Category { Name = "Chuột Gaming", Description = "Chuột không dây & có dây siêu nhẹ Esports" },
                new Category { Name = "Tai nghe Gaming", Description = "Tai nghe âm thanh vòm 7.1 chuẩn thi đấu" },
                new Category { Name = "Lót chuột & Phụ kiện", Description = "Pad chuột khổ lớn, kê tay, Cable custom" }
            };

            await context.Categories.AddRangeAsync(categories);
            await context.SaveChangesAsync();
        }

        var keyboardCat = context.Categories.FirstOrDefault(c => c.Name == "Bàn phím cơ") ?? context.Categories.First();
        var mouseCat = context.Categories.FirstOrDefault(c => c.Name == "Chuột Gaming") ?? context.Categories.First();
        var headsetCat = context.Categories.FirstOrDefault(c => c.Name == "Tai nghe Gaming") ?? context.Categories.First();
        var accessoryCat = context.Categories.FirstOrDefault(c => c.Name == "Lót chuột & Phụ kiện") ?? context.Categories.First();

        // 5. KHO SẢN PHẨM GẦN 30 MÓN CHUẨN XÁC (BẢO VỆ DỮ LIỆU ĐƠN HÀNG KHI RESTART)
        if (!context.Products.Any())
        {
            var masterProductList = new List<Product>
            {
                // === BÀN PHÍM CƠ ===
                new Product
                {
                    Name = "Bàn phím cơ Keychron Q1 Pro",
                    Description = "Layout 75% nhôm CNC nguyên khối. Kết nối 3 chế độ: Cáp Type-C rời mạ vàng, Bluetooth 5.1 và Wireless 2.4GHz. Mạch Hot-swap 5-pin, đệm Gasket Mount cho âm gõ đầm ấm.",
                    Price = 3490000m, StockQuantity = 15,
                    ImageUrl = "https://images.unsplash.com/photo-1618384887929-16ec33fab9ef?auto=format&fit=crop&q=80&w=600",
                    CategoryId = keyboardCat.Id
                },
                new Product
                {
                    Name = "Bàn phím cơ Logitech G Pro X TKL",
                    Description = "Thiết kế Tenkeyless Esports. Kết nối LIGHTSPEED không dây 1ms, Bluetooth và jack cáp sạc USB-C bọc dù. Switch GX Mechanical siêu nảy, LED RGB LIGHTSYNC rực rỡ.",
                    Price = 3990000m, StockQuantity = 12,
                    ImageUrl = "https://images.unsplash.com/photo-1587829741301-dc798b83add3?auto=format&fit=crop&q=80&w=600",
                    CategoryId = keyboardCat.Id
                },
                new Product
                {
                    Name = "Bàn phím cơ Razer BlackWidow V4 Pro",
                    Description = "Flagship trang bị con đĩa Command Dial và cổng USB Passthrough. Dùng cáp USB Type-C tháo rời, switch quang cơ Razer bền 100 triệu lần nhấn, kê tay da đệm từ tính.",
                    Price = 4990000m, StockQuantity = 8,
                    ImageUrl = "https://images.unsplash.com/photo-1595225476474-87563907a212?auto=format&fit=crop&q=80&w=600",
                    CategoryId = keyboardCat.Id
                },
                new Product
                {
                    Name = "Bàn phím cơ Asus ROG Strix Scope II",
                    Description = "Switch quang cơ ROG NX Storm lube sẵn. Kết nối dây cáp USB 2.0 Type-C chắc chắn, phím Space gia cố chống rung, đệm tiêu âm kép triệt tiêu tiếng vang vỏ hộp.",
                    Price = 3690000m, StockQuantity = 10,
                    ImageUrl = "https://images.unsplash.com/photo-1555680202-c86f0e12f086?auto=format&fit=crop&q=80&w=600",
                    CategoryId = keyboardCat.Id
                },
                new Product
                {
                    Name = "Bàn phím cơ Akko Mod007 v3 VIA",
                    Description = "Bàn phím Custom nhôm nguyên khối, mạch South-Facing Hot-swap tùy biến qua phần mềm VIA. Cáp rời Type-C chống nhiễu, đi kèm Keycap PBT Cherry Profile.",
                    Price = 2890000m, StockQuantity = 14,
                    ImageUrl = "https://images.unsplash.com/photo-1618384887929-16ec33fab9ef?auto=format&fit=crop&q=80&w=500",
                    CategoryId = keyboardCat.Id
                },
                new Product
                {
                    Name = "Bàn phím cơ Epomaker RT100 Retro",
                    Description = "Layout 97 phím kèm màn hình Smart Mini Display hiển thị GIF/pin. Núm xoay đa năng, kết nối 3 chế độ (Type-C, Bluetooth 5.0, 2.4GHz Wireless) mượt mà.",
                    Price = 2590000m, StockQuantity = 16,
                    ImageUrl = "https://images.unsplash.com/photo-1587829741301-dc798b83add3?auto=format&fit=crop&q=80&w=500",
                    CategoryId = keyboardCat.Id
                },

                // === CHUỘT GAMING ===
                new Product
                {
                    Name = "Chuột Logitech G Pro X Superlight 2",
                    Description = "Siêu nhẹ 60g. Đầu thu USB Wireless LIGHTSPEED không độ trễ, hỗ trợ sạc nhanh qua jack USB-C. Cảm biến HERO 2 (32,000 DPI), switch quang hybrid cực nhạy.",
                    Price = 2990000m, StockQuantity = 20,
                    ImageUrl = "https://images.unsplash.com/photo-1615663245857-ac93bb7c39e7?auto=format&fit=crop&q=80&w=600",
                    CategoryId = mouseCat.Id
                },
                new Product
                {
                    Name = "Chuột Razer DeathAdder V3 Pro",
                    Description = "Form công thái học FPS. Cảm biến Focus Pro 30K DPI, kết nối Razer HyperSpeed 2.4GHz USB Receiver hoặc dây cáp Speedflex USB Type-C siêu mềm mại.",
                    Price = 3290000m, StockQuantity = 18,
                    ImageUrl = "https://images.unsplash.com/photo-1527864550417-7fd91fc51a46?auto=format&fit=crop&q=80&w=600",
                    CategoryId = mouseCat.Id
                },
                new Product
                {
                    Name = "Chuột Pulsar X2V2 Wireless",
                    Description = "Chuột đối xứng siêu nhẹ. Mắt đọc PAW3395 (26,000 DPI), kết nối Wireless 2.4GHz và dây Type-C. Encoder Pulsar Blue Superglide chống bụi tuyệt đối.",
                    Price = 2490000m, StockQuantity = 15,
                    ImageUrl = "https://images.unsplash.com/photo-1593508512255-86ab42a8e620?auto=format&fit=crop&q=80&w=600",
                    CategoryId = mouseCat.Id
                },
                new Product
                {
                    Name = "Chuột Lamzu Atlantis OG V2 Pro",
                    Description = "Đáy khoét rỗng độc đáo 55g. Hỗ trợ polling rate 4K Hz, kết nối không dây 2.4G và cáp Type-C chống rối, feet chuột 100% PTFE lướt cực mượt.",
                    Price = 2690000m, StockQuantity = 13,
                    ImageUrl = "https://images.unsplash.com/photo-1615663245857-ac93bb7c39e7?auto=format&fit=crop&q=80&w=500",
                    CategoryId = mouseCat.Id
                },
                new Product
                {
                    Name = "Chuột Ninjutso Sora V2 Wireless",
                    Description = "Siêu nhẹ kỷ lục 39g dành cho game thủ Pro. Cảm biến PixArt 3395, kết nối Wireless không độ trễ qua USB Dongle 2.4GHz và cáp sạc USB Type-C.",
                    Price = 2790000m, StockQuantity = 9,
                    ImageUrl = "https://images.unsplash.com/photo-1527864550417-7fd91fc51a46?auto=format&fit=crop&q=80&w=500",
                    CategoryId = mouseCat.Id
                },
                new Product
                {
                    Name = "Chuột Glorious Model O 2 Wireless",
                    Description = "Thiết kế tổ ong nhẹ 68g thoáng khí. Cảm biến BAMF 2.0 26K DPI, kết nối kép 2.4GHz Wireless và cáp sạc Ascended USB-C siêu dẻo.",
                    Price = 2190000m, StockQuantity = 16,
                    ImageUrl = "https://images.unsplash.com/photo-1615663245857-ac93bb7c39e7?auto=format&fit=crop&q=80&w=400",
                    CategoryId = mouseCat.Id
                },

                // === TAI NGHE GAMING ===
                new Product
                {
                    Name = "Tai nghe Razer BlackShark V2 Pro",
                    Description = "Tai nghe Esports chuẩn thi đấu. Kết nối đa dạng: Wireless 2.4GHz USB Dongle, Bluetooth 5.2 và jack cắm 3.5mm tiêu chuẩn cho PC, PS5, Switch, điện thoại.",
                    Price = 4190000m, StockQuantity = 10,
                    ImageUrl = "https://images.unsplash.com/photo-1546435770-a3e426bf472b?auto=format&fit=crop&q=80&w=600",
                    CategoryId = headsetCat.Id
                },
                new Product
                {
                    Name = "Tai nghe HyperX Cloud III Wireless",
                    Description = "Thời lượng pin kỷ lục 120 giờ. Tương thích USB Wireless Receiver 2.4GHz kèm đầu chuyển USB-C sang USB-A. Driver 53mm tái tạo âm thanh vòm 3D.",
                    Price = 3590000m, StockQuantity = 14,
                    ImageUrl = "https://images.unsplash.com/photo-1505740420928-5e560c06d30e?auto=format&fit=crop&q=80&w=600",
                    CategoryId = headsetCat.Id
                },
                new Product
                {
                    Name = "Tai nghe SteelSeries Arctis Nova 7",
                    Description = "Hệ thống Nova Acoustic System với driver High Fidelity. Kết nối song song Wireless 2.4GHz và Bluetooth, có jack 3.5mm cho thiết bị di động.",
                    Price = 3890000m, StockQuantity = 12,
                    ImageUrl = "https://images.unsplash.com/photo-1484704849700-f032a568e944?auto=format&fit=crop&q=80&w=600",
                    CategoryId = headsetCat.Id
                },
                new Product
                {
                    Name = "Tai nghe Asus ROG Delta S Wireless",
                    Description = "Màng loa Essence 50mm độc quyền. Kết nối Dual-mode 2.4GHz & Bluetooth, hỗ trợ sạc nhanh qua cổng USB-C (15 phút sạc dùng 3 giờ), mút tai êm ái.",
                    Price = 4490000m, StockQuantity = 7,
                    ImageUrl = "https://images.unsplash.com/photo-1546435770-a3e426bf472b?auto=format&fit=crop&q=80&w=500",
                    CategoryId = headsetCat.Id
                },
                new Product
                {
                    Name = "Tai nghe Corsair Virtuoso RGB Wireless",
                    Description = "Âm thanh Hi-Fi 24bit/96kHz qua dây cáp USB Type-C, kết nối không dây Slipstream 2.4GHz siêu nhanh và jack 3.5mm. Khung nhôm cao cấp sang trọng.",
                    Price = 4290000m, StockQuantity = 8,
                    ImageUrl = "https://images.unsplash.com/photo-1505740420928-5e560c06d30e?auto=format&fit=crop&q=80&w=500",
                    CategoryId = headsetCat.Id
                },
                new Product
                {
                    Name = "Tai nghe Logitech G733 Lightspeed RGB",
                    Description = "Thiết kế trọng lượng nhẹ 278g với quai đeo vải co giãn cá tính. Công nghệ âm thanh vòm DTS Headphone:X 2.0, kết nối không dây LIGHTSPEED qua USB Receiver.",
                    Price = 3190000m, StockQuantity = 15,
                    ImageUrl = "https://images.unsplash.com/photo-1590658268037-6bf12165a8df?auto=format&fit=crop&q=80&w=600",
                    CategoryId = headsetCat.Id
                },
                new Product
                {
                    Name = "Tai nghe Razer Kraken V3 X",
                    Description = "Trọng lượng siêu nhẹ 285g chuyên chơi game giả lập 7.1. Màng loa TriForce 40mm, cổng kết nối USB Type-A mạ vàng truyền tín hiệu âm thanh cực nét.",
                    Price = 1490000m, StockQuantity = 22,
                    ImageUrl = "https://images.unsplash.com/photo-1535223289827-42f1e9919769?auto=format&fit=crop&q=80&w=600",
                    CategoryId = headsetCat.Id
                },

                // === LÓT CHUỘT & PHỤ KIỆN ===
                new Product
                {
                    Name = "Lót chuột SteelSeries QcK Heavy XXL",
                    Description = "Khổ lớn 900x400x6mm. Bề mặt vải vi dệt độc quyền tối ưu hóa mọi mắt đọc cảm biến quang học, đế cao su thiên nhiên dày 6mm êm ái chống trượt.",
                    Price = 890000m, StockQuantity = 30,
                    ImageUrl = "https://images.unsplash.com/photo-1615663245857-ac93bb7c39e7?auto=format&fit=crop&q=80&w=600",
                    CategoryId = accessoryCat.Id
                },
                new Product
                {
                    Name = "Lót chuột Artisan Zero FX Soft XL",
                    Description = "Pad chuột cao cấp số 1 Nhật Bản dành cho game thủ FPS. Bề mặt vải dệt công nghệ cao giúp dừng tâm chính xác, đế xốp Poron bám chặt mặt bàn.",
                    Price = 1450000m, StockQuantity = 15,
                    ImageUrl = "https://images.unsplash.com/photo-1527864550417-7fd91fc51a46?auto=format&fit=crop&q=80&w=500",
                    CategoryId = accessoryCat.Id
                },
                new Product
                {
                    Name = "Bộ Keycap PBT Dye-Sub Samurai",
                    Description = "Set 138 phím Profile Cherry chuẩn ANSI layout. Nhựa PBT siêu bền chống bám vân tay và bóng dầu, công nghệ in nhiệt Dye-Sublimation sắc nét.",
                    Price = 690000m, StockQuantity = 22,
                    ImageUrl = "https://images.unsplash.com/photo-1595225476474-87563907a212?auto=format&fit=crop&q=80&w=500",
                    CategoryId = accessoryCat.Id
                },
                new Product
                {
                    Name = "Kê tay bàn phím cơ gỗ Walnut",
                    Description = "Gỗ óc chó tự nhiên nguyên khối 100% gia công CNC tinh xảo. Góc nghiêng công thái học chuẩn giúp nâng đỡ cổ tay, chống mỏi khi gõ phím lâu.",
                    Price = 450000m, StockQuantity = 25,
                    ImageUrl = "https://images.unsplash.com/photo-1587829741301-dc798b83add3?auto=format&fit=crop&q=80&w=500",
                    CategoryId = accessoryCat.Id
                },
                new Product
                {
                    Name = "Cáp Custom Coiled Cable Aviator",
                    Description = "Cáp xoắn lò xo bọc dù tích hợp đầu nối kim loại mạ vàng GX16 Aviator. Cổng USB Type-C sang USB-A truyền tín hiệu siêu tốc cho bàn phím cơ.",
                    Price = 550000m, StockQuantity = 20,
                    ImageUrl = "https://images.unsplash.com/photo-1555680202-c86f0e12f086?auto=format&fit=crop&q=80&w=500",
                    CategoryId = accessoryCat.Id
                },
                new Product
                {
                    Name = "Chân đế kê tai nghe RGB Corsair ST100",
                    Description = "Giá treo tai nghe chất liệu nhôm nguyên khối bền bỉ. Tích hợp dải đèn LED RGB 9 vùng, tích hợp DAC âm thanh vòm 7.1 chuẩn jack 3.5mm và 2 cổng USB 3.1.",
                    Price = 1290000m, StockQuantity = 11,
                    ImageUrl = "https://images.unsplash.com/photo-1484704849700-f032a568e944?auto=format&fit=crop&q=80&w=500",
                    CategoryId = accessoryCat.Id
                },
                new Product
                {
                    Name = "Tay đỡ Micro HyperX QuadCast Boom Arm",
                    Description = "Cần gạt chịu lực chắc chắn gắn bàn làm việc, tích hợp rãnh quản lý cáp gọn gàng, khớp xoay linh hoạt 360 độ chuyên dụng cho streamer.",
                    Price = 1150000m, StockQuantity = 14,
                    ImageUrl = "https://images.unsplash.com/photo-1583394838336-acd977736f90?auto=format&fit=crop&q=80&w=600",
                    CategoryId = accessoryCat.Id
                },
                new Product
                {
                    Name = "Hub chuyển đổi USB-C Hub 7 trong 1 Anker",
                    Description = "Mở rộng cổng kết nối gồm cổng HDMI 4K, cổng sạc nhanh PD 100W, khe đọc thẻ nhớ SD/TF và 3 cổng USB 3.0 tốc độ cao cho laptop gaming.",
                    Price = 990000m, StockQuantity = 19,
                    ImageUrl = "https://cdn2.cellphones.com.vn/x/media/catalog/product/h/u/hub-chuyen-doi-anker-7-in-1-usb-c-a83d2_4_.png",
                    CategoryId = accessoryCat.Id
                }
            };

            await context.Products.AddRangeAsync(masterProductList);
            await context.SaveChangesAsync();

            // 6. Seed Feedback đánh giá cho sản phẩm (Chỉ chạy khi vừa khởi tạo kho sản phẩm)
            var allProducts = context.Products.ToList();
            var sampleFeedbacks = new List<Feedback>();

            foreach (var prod in allProducts)
            {
                sampleFeedbacks.Add(new Feedback
                {
                    ProductId = prod.Id,
                    UserId = testUser?.Id ?? "",
                    Rating = 5,
                    Comment = $"Sản phẩm {prod.Name} đúng như mô tả ảnh mẫu, chất lượng build cực kỳ cao cấp và jack cắm kết nối nhạy bén!",
                    AdminReply = "Ambition Karma Shop chân thành cảm ơn đánh giá của bạn! Chúc bạn có những phút giây trải nghiệm game tuyệt vời.",
                    CreatedAt = DateTime.Now.AddDays(-2)
                });

                sampleFeedbacks.Add(new Feedback
                {
                    ProductId = prod.Id,
                    UserId = testUser?.Id ?? "",
                    Rating = 5,
                    Comment = "Shop giao hàng siêu tốc, đóng gói chống sốc kỹ càng. Sẽ ủng hộ shop dài lâu.",
                    AdminReply = "Cảm ơn bạn đã tin tưởng ủng hộ shop nhé!",
                    CreatedAt = DateTime.Now.AddDays(-1)
                });
            }

            await context.Feedbacks.AddRangeAsync(sampleFeedbacks);
            await context.SaveChangesAsync();
        }

        // 7. Seed lịch sử câu hỏi Hỗ trợ 24/7 gắn với tài khoản testUser (hn3508777@gmail.com)
        if (!context.SupportMessages.Any())
        {
            var sampleSupportMessages = new List<SupportMessage>
            {
                new SupportMessage
                {
                    UserId = testUser?.Id ?? "",
                    CustomerName = "Rô Bin Sôn (Test)",
                    Question = "Shop ơi, em muốn tìm bàn phím custom gõ êm nhẹ để vừa học vừa chơi game đêm không ảnh hưởng phòng bên thì chọn loại nào?",
                    AdminReply = "Chào bạn! Với nhu cầu gõ êm không gây tiếng ồn đêm khuya, shop khuyên bạn nên chọn mẫu Keychron Q1 Pro hoặc Akko Mod007 v3 dùng mạch South-Facing kết hợp đệm tiêu âm Gasket Mount.",
                    CreatedAt = DateTime.Now.AddDays(-2),
                    IsResolved = true
                },
                new SupportMessage
                {
                    UserId = testUser?.Id ?? "",
                    CustomerName = "Rô Bin Sôn (Test)",
                    Question = "Chuột Superlight 2 kết nối Wireless 2.4GHz có bị nhiễu sóng khi dùng chung bàn với Router Wi-Fi không shop?",
                    AdminReply = "Chào bạn! Công nghệ LIGHTSPEED 2.4GHz của Logitech có tần số phản hồi 1ms siêu chống nhiễu. Bạn cứ yên tâm sử dụng nhé!",
                    CreatedAt = DateTime.Now.AddDays(-1),
                    IsResolved = true
                }
            };

            await context.SupportMessages.AddRangeAsync(sampleSupportMessages);
            await context.SaveChangesAsync();
        }
    }
}