using Auren.Application.Features.Profile.DTOs;
using System;
using System.Collections.Generic;
using System.Text;

namespace Auren.Infrastructure.Common.Interfaces
{
    public interface ICloudinaryService
    {
        Task<ProfileImageUploadResponse> UploadProfileImageAsync(
            ProfileImageUploadRequest request,
            CancellationToken ct);
    }
}
