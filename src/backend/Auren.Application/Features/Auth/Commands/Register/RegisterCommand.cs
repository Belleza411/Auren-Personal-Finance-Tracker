using Auren.Application.Features.Auth.DTOs;
using System;
using System.Collections.Generic;
using System.Text;

namespace Auren.Application.Features.Auth.Commands.Register
{
    public record RegisterCommand(RegisterRequest Request);
}
