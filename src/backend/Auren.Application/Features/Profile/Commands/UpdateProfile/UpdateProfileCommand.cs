using Auren.Application.Features.Auth.DTOs;

namespace Auren.Application.Features.Profile.Commands.UpdateProfile
{
    public record UpdateProfileCommand(Guid UserId, UserDto Dto);
}
