namespace GamingGear.Application.Interfaces; // Sửa namespace cho khớp project

public interface IEmailService
{
    Task SendEmailAsync(string toEmail, string subject, string htmlMessage);
}