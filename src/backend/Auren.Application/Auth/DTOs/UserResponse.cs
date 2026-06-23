namespace Auren.Application.Auth.DTOs
{
    public sealed record UserResponse(
         string Email,
         string FirstName,
         string LastName,
         string? ProfilePictureUrl,
         DateTime CreatedAt,
         DateTime? LastLoginAt
    );
}
