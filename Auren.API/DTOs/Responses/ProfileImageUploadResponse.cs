namespace Auren.API.DTOs.Responses
{
	public sealed record ProfileImageUploadResponse(
		string Name,
		string? Description,
		string Extension,
		long SizeInBytes,
		string Path
	);
}
