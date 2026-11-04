using System.ComponentModel.DataAnnotations;

namespace GamingGear.Web.Models;

public class VerifyOtpViewModel
{
    [Required(ErrorMessage = "Vui lòng nhập email")]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Vui lòng nhập mã OTP 6 chữ số")]
    [StringLength(6, MinimumLength = 6, ErrorMessage = "Mã OTP phải đúng 6 chữ số")]
    public string OtpCode { get; set; } = string.Empty;
}