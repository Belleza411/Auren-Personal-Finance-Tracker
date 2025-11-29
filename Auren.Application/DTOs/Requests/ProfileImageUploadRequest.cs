using Microsoft.AspNetCore.Http;

namespace Auren.Application.DTOs.Requests
{
	public sealed record ProfileImageUploadRequest(IFormFile File, string? Name, string? Description);
}
