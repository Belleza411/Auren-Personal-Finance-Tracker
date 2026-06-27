using Auren.Application.Features.Auth.DTOs;
using Auren.Domain.Entities;

namespace Auren.Application.Extensions
{
    public static class UserMappingExtensions
    {
        public static UserResponse ToUserResponse(this ApplicationUser user) =>
            new(
                user.Email!,
                user.FirstName,
                user.LastName,
                user.ProfilePictureUrl,
                user.CreatedAt,
                user.LastLoginAt
            );
    }
}
