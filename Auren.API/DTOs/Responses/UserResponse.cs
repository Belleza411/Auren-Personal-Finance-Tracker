namespace Auren.API.DTOs.Responses
{
    public sealed record UserResponse(
         Guid UserId,
         string Email,
         string FirstName,
         string LastName,
         string? ProfilePictureUrl,
         bool IsGoogleUser,
         DateTime CreatedAt,
         DateTime? LastLoginAt,
    );
}
