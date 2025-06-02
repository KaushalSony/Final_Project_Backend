using Final_Project_WebAPI.Services;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace Final_Project_WebAPI.Controllers
{
    [Route("api/test/email")]
    [ApiController]
    public class TestEmailController : ControllerBase
    {
        private readonly IEmailService _emailService;

        public TestEmailController(IEmailService emailService)
        {
            _emailService = emailService;
        }

        [HttpPost]
        public async Task<IActionResult> SendTestEmail([FromQuery] string to)
        {
            try
            {
                await _emailService.SendEmailAsync(
                    to,
                    "Test Email",
                    "This is a test email from the Final_Project_WebAPI."
                );
                return Ok("Test email sent successfully.");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Failed to send test email: {ex.Message}");
            }
        }
    }
}