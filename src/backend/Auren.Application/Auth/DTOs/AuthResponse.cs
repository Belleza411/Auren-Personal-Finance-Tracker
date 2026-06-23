namespace Auren.Application.Auth.DTOs
{
	public class AuthResponse
	{
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public UserResponse? User { get; set; }
        public List<string> Errors { get; set; } = new();
    }
}
