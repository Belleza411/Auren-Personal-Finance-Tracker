using System.Security.Claims;

namespace Auren.Application.Extensions
{
	public static class ClaimsPrincipalExtension
	{
        public static Guid? GetCurrentUserId(this ClaimsPrincipal user)
        {
            var userIdClaim = user.FindFirst("UserId")?.Value;

            if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
            {
                return null;
            }

            return userId;
        }
    }
}
