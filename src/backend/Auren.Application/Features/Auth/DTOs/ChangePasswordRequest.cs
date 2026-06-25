using System;
using System.Collections.Generic;
using System.Text;

namespace Auren.Application.Features.Auth.DTOs
{
    public sealed record ChangePasswordRequest(string CurrentPassword, string NewPassword, string ConfirmPassword);
}
