using Auren.Application.Features.Auth.DTOs;
using Auren.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

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
