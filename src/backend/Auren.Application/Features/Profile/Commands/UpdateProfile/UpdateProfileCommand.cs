using Auren.Application.Features.Auth.DTOs;
using System;
using System.Collections.Generic;
using System.Text;

namespace Auren.Application.Features.Profile.Commands.UpdateProfile
{
    public record UpdateProfileCommand(Guid UserId, UserDto Dto);
}
