using Auren.Application.Features.Profile.DTOs;

namespace Auren.Application.Common.Interfaces
{
    public interface ICloudinaryService
    {
        Task<ProfileImageUploadResponse> UploadProfileImageAsync(
            ProfileImageUploadRequest request,
            CancellationToken ct);
    }
}
