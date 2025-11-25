using Auren.API.DTOs.Requests;
using FluentValidation;

namespace Auren.API.Validators
{
	public class ProfileImageUploadValidator : AbstractValidator<ProfileImageUploadRequest>
	{
		private const long MaxFileSizeInBytes = 2 * 1024 * 1024;

		public ProfileImageUploadValidator()
		{
			RuleFor(f => f.File)
				.NotEmpty().WithMessage("File is required. ")
				.Must(BeAValidImageExtension).WithMessage("Unsupported file extension.")
				.Must(f => f.Length <= MaxFileSizeInBytes)
					.WithMessage($"File size must be less than {MaxFileSizeInBytes / (1024 * 1024)} mb. ");

			RuleFor(f => f.Name)
				.NotEmpty().WithMessage("Name is required. ")
				.MaximumLength(100).WithMessage("Name must be at most 100 characters.");
		}

		private bool BeAValidImageExtension(IFormFile file)
		{
			if (file == null) return false;

			var allowedExtensions = new[] { ".jpg", ".jpeg", ".png" };

			var extension = Path.GetExtension(file.FileName).ToLowerInvariant();

			return allowedExtensions.Contains(extension);
		}
	}
}
