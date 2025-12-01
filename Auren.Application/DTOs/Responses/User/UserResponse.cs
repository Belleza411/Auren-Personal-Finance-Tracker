namespace Auren.Application.DTOs.Responses.User
{
    public sealed record UserResponse(
         Guid UserId,
         string Email,
         string FirstName,
         string LastName,
         string? ProfilePictureUrl,
         DateTime CreatedAt,
         DateTime? LastLoginAt
    );
}
