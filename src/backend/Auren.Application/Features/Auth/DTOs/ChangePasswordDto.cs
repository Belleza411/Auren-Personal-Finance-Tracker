using System;
using System.Collections.Generic;
using System.Text;

namespace Auren.Application.Features.Auth.DTOs
{
    public sealed record ChangePasswordDto(
        string CurrentPassword,
        string NewPassword,
        string ConfirmPassword);
}
