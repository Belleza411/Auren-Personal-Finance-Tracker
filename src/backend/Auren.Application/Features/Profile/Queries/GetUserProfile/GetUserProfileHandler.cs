using Auren.Application.Common.Interfaces;
using Auren.Application.Common.Result;
using Auren.Application.Extensions;
using Auren.Application.Features.Auth.DTOs;
using Auren.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace Auren.Application.Features.Profile.Queries.GetUserProfile
{
    public class GetUserProfileHandler(IAuthDbContext db)
    {
        public async Task<Result<UserResponse>> Handle(
            GetUserProfileQuery cmd,
            CancellationToken ct)
        {
            var user = await db.Users
                .AsNoTracking()
                .FirstOrDefaultAsync(u =>
                    u.UserId == cmd.UserId,
                    ct);

            return user != null
                ? Result.Success(user.ToUserResponse())
                : Result.Failure<UserResponse>(Error.NotFound("User not found."));
        }
    }
}
