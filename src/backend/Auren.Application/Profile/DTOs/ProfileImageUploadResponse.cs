namespace Auren.Application.Profile.DTOs
{
	public sealed record ProfileImageUploadResponse(
		string Name,
		string? Description,
		string Extension,
		long SizeInBytes,
		string Path
	);
}
