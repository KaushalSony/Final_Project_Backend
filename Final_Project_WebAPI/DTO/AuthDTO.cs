namespace Final_Project_WebAPI.DTO
{
    public class LoginDTO
    {
        public string Email { get; set; } = null!;

        public string Password { get; set; } = null!;
    }
    public class RegisterDTO
    {
        public string Name { get; set; } = null!;

        public string Email { get; set; } = null!;

        public string Role { get; set; } = null!;

        public string Password { get; set; } = null!;
    }
    public class ResetPasswordDTO
    {
        public string Email { get; set; } = null!;
        public string NewPassword { get; set; } = null!;
    }
    public class ForgotPasswordDTO
    {
        public string Email { get; set; } = null!;
    }
    public class ResetPasswordWithTokenDTO
    {
        public string Token { get; set; } = null!;
        public string NewPassword { get; set; } = null!;
    }
    public class AuthResponseDTO
    {
        public string Token { get; set; } = null!; // Added null-forgiving operator to suppress CS8618

        public string Name { get; set; } = null!;

        public Guid UserId { get; set; }

        public string Role { get; set; } = null!;
    }
}
