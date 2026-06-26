using Auren.Application.Common.Interfaces;
using Auren.Application.Common.Result;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace Auren.Application.Features.Auth.Commands.DeleteAccount
{
    public class DeleteAccountHandler(
        IIdentityService identity,
        ITokenService tokenService,
        IAppDbContext db)
    {
        public async Task<Result> Handle(
            DeleteAccountCommand cmd,
            CancellationToken ct)
        {
            var user = await identity.FindByIdAsync(cmd.UserId);

            if (user == null)
                return Result.Failure(Error.NotFound("User not found."));

            var passwordValid = await identity.CheckPasswordAsync(user!, cmd.Password);
            if (!passwordValid.Succeeded)
                return Result.Failure(Error.InvalidInput("Invalid password."));

            await tokenService.RevokeAllUserRefreshTokens(cmd.UserId, ct);

            var transactions = await db.Transactions
                .Where(t => t.UserId == cmd.UserId)
                .ToListAsync(ct);

            var categories = await db.Categories
                .Where(c => c.UserId == cmd.UserId)
                .ToListAsync(ct);

            db.Transactions.RemoveRange(transactions);
            db.Categories.RemoveRange(categories);
            await db.SaveChangesAsync(ct);

            var result = await identity.DeleteAsync(user!);
            if (!result.Succeeded)
            {
                var errors = result.Errors.Select(e => e.Description).ToArray();
                return Result.Failure(Error.DeleteFailed(errors));
            }

            return Result.Success();
        }
    }
}
