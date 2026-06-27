namespace Auren.Application.Features.Auth.DTOs
{
    public record AuthResponse(
        UserResponse User,
        string Message
    );
}
