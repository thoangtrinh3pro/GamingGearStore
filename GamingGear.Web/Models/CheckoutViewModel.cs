using System.ComponentModel.DataAnnotations;

namespace GamingGear.Web.Models;

public class CheckoutViewModel
{
    [Required(ErrorMessage = "Vui lòng nhập họ và tên")]
    public string FullName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Vui lòng nhập số điện thoại")]
    public string PhoneNumber { get; set; } = string.Empty;

    [Required(ErrorMessage = "Vui lòng nhập địa chỉ giao hàng")]
    public string Address { get; set; } = string.Empty;

    public List<CartItem> CartItems { get; set; } = new();
}