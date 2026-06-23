using Microsoft.AspNetCore.Http;

namespace Auren.Application.Features.Profile.DTOs
{
	public sealed record ProfileImageUploadRequest(IFormFile File, string? Name, string? Description);
}
