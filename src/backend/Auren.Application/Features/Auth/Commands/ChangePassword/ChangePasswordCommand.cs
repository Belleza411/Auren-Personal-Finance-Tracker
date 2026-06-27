using Auren.Application.Features.Auth.DTOs;

namespace Auren.Application.Features.Auth.Commands.ChangePassword
{
    public record ChangePasswordCommand(
        Guid UserId,
        ChangePasswordDto Dto
        );
}
