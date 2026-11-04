# 🛒 GamingGear Store - E-Commerce Web Application

<p align="center">
  <b>Đồ án Môn học / Khóa luận tốt nghiệp</b><br>
  Hệ thống Thương mại Điện tử chuyên kinh doanh thiết bị và phụ kiện chơi game.
</p>

---

## 👨‍💻 Thông tin sinh viên thực hiện
* **Họ và tên:** Trịnh Quang Thoang
* **Mã số sinh viên (MSSV):** 230226
* **Trường:** Đại học Thái Bình Dương

---

## 🛠️ Công nghệ & Kiến trúc Hệ thống
Dự án được xây dựng theo mô hình **Clean Architecture** nhằm đảm bảo tính tách biệt, dễ bảo trì và mở rộng:
* **Backend:** ASP.NET Core MVC (.NET), Entity Framework Core.
* **Authentication & Authorization:** ASP.NET Core Identity (Quản lý phân quyền người dùng và Admin).
* **Database:** Microsoft SQL Server.
* **Architecture Pattern:** Clean Architecture (chia thành 4 tầng riêng biệt: `Domain`, `Application`, `Infrastructure`, `Web`).

---

## 🌟 Các tính năng nổi bật
1. **Phân hệ Khách hàng:**
   * Duyệt sản phẩm Gaming Gear theo danh mục, tìm kiếm và lọc sản phẩm mượt mà.
   * Giỏ hàng quản lý qua Session, áp dụng mã giảm giá / Voucher linh hoạt.
   * Quy trình đặt hàng trực tuyến, trừ tồn kho tự động.
   * **Tra cứu và quản lý lịch sử đơn hàng:** Khách hàng dễ dàng theo dõi trạng thái đơn.
   * **Hủy đơn hàng thông minh:** Cho phép hủy khi đơn ở trạng thái chờ (`Pending`) và tự động hoàn trả số lượng tồn kho.
   * **Hệ thống đánh giá sản phẩm thực tế (Smart Review):** Đảm bảo khách hàng phải mua và hoàn tất đơn hàng mới có thể viết đánh giá tính từ thời điểm đặt đơn.
   * **In hóa đơn chuẩn A4:** Tích hợp tính năng xuất và in hóa đơn trực tiếp từ trình duyệt thông qua CSS `@media print`.

2. **Phân hệ Quản trị (Admin Dashboard):**
   * Quản lý danh mục sản phẩm, cập nhật thông tin hàng hóa.
   * Quản lý đơn hàng, duyệt trạng thái và theo dõi doanh thu.

---

## ⚙️ Hướng dẫn Cài đặt & Chạy Dự án

### Yêu cầu hệ thống
* Cài đặt [.NET SDK](https://dotnet.microsoft.com/) (phiên bản tương thích với dự án).
* Microsoft SQL Server / LocalDB.
* Visual Studio 2022 hoặc Visual Studio Code.

### Các bước thực hiện
1. **Clone hoặc tải mã nguồn về máy:**
   ```bash
   git clone <repository-url>
   cd GamingGearStore