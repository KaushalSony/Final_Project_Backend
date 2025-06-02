namespace Final_Project_WebAPI.Services
{
    public interface IEmailService
    {
        Task SendEmailAsync(string to, string subject, string body);
    }
}