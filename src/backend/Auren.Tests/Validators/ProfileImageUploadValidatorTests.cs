using Auren.Application.Features.Profile.DTOs;
using Auren.Application.Features.Profile.Validators;
using FluentValidation.TestHelper;
using Microsoft.AspNetCore.Http;
using Moq;
using System.Net.Mime;

namespace Auren.Tests.Validators
{
    public class ProfileImageUploadValidatorTests
    {
        private readonly ProfileImageUploadValidator _validator = new();
        private static IFormFile CreateMockFile(
            string fileName = "photo.png",
            long sizeInBytes = 1024,
            string contentType = "image/jpeg")
        {
            var fileMock = new Mock<IFormFile>();

            fileMock.Setup(f => f.FileName).Returns(fileName);
            fileMock.Setup(f => f.Length).Returns(sizeInBytes);
            fileMock.Setup(f => f.ContentType).Returns(contentType);

            return fileMock.Object;
        }

        private static ProfileImageUploadRequest CreateRequest(
            IFormFile? file = null,
            string name = "Profile Photo",
            string? description = null) =>
                new(
                    File: file!,
                    Name: name,
                    Description: description
                );


        [Fact]
        public void File_WhenNull_ShouldFail()
        {
            var request = CreateRequest(file: null);
            var result = _validator.TestValidate(request);

            result.ShouldHaveValidationErrorFor(x => x.File)
                .WithErrorMessage("File is required. ");
        }

        [Theory]
        [InlineData("photo.gif")]
        [InlineData("photo.bmp")]
        [InlineData("photo.webp")]
        [InlineData("photo.pdf")]
        [InlineData("photo.txt")]
        [InlineData("photo.exe")] 
        public void File_WithInvalidExtension_ShouldHaveError(string fileName)
        {
            var request = CreateRequest(file: CreateMockFile(fileName));
            var result = _validator.TestValidate(request);
            result.ShouldHaveValidationErrorFor(x => x.File)
                .WithErrorMessage("Unsupported file extension.");
        }

        [Theory]
        [InlineData("photo.jpg")]
        [InlineData("photo.jpeg")]
        [InlineData("photo.png")]
        public void File_WithValidExtension_ShouldNotHaveError(string fileName)
        {
            var request = CreateRequest(file: CreateMockFile(fileName));
            var result = _validator.TestValidate(request);

            result.ShouldNotHaveValidationErrorFor(x => x.File);
        }

        [Fact]
        public void File_ExactlyAtMaxSize_ShouldNotHaveError()
        {
            var maxSize = 2 * 1024 * 1024; // exactly 2MB
            var request = CreateRequest(file: CreateMockFile(sizeInBytes: maxSize));
            var result = _validator.TestValidate(request);
            result.ShouldNotHaveValidationErrorFor(x => x.File);
        }

        [Fact]
        public void File_ExceededMaxSize_ShouldHaveError()
        {
            var maxSize = (2 * 1024 * 1024) + 1; // 2.1MB
            var request = CreateRequest(file: CreateMockFile(sizeInBytes: maxSize));
            var result = _validator.TestValidate(request);
            result.ShouldHaveValidationErrorFor(x => x.File)
                .WithErrorMessage("File size must be less than 2 mb. ");
        }

        [Fact]
        public void File_BelowMaxSize_ShouldNotHaveError()
        {
            var underMaxSize = 1 * 1024 * 1024; // 1MB
            var request = CreateRequest(file: CreateMockFile(sizeInBytes: underMaxSize));
            var result = _validator.TestValidate(request);
            result.ShouldNotHaveValidationErrorFor(x => x.File);
        }

        [Fact]
        public void Name_WhenEmpty_ShouldHaveError()
        {
            var request = CreateRequest(name: string.Empty);
            var result = _validator.TestValidate(request);
            result.ShouldHaveValidationErrorFor(x => x.Name)
                .WithErrorMessage("Name is required. ");
        }

        [Fact]
        public void Name_WhenNull_ShouldHaveError()
        {
            var request = CreateRequest(name: null!);
            var result = _validator.TestValidate(request);
            result.ShouldHaveValidationErrorFor(x => x.Name)
                .WithErrorMessage("Name is required. ");
        }

        [Fact]
        public void Name_WhenValid_ShouldNotHaveError()
        {
            var request = CreateRequest(name: "Profile Photo");
            var result = _validator.TestValidate(request);
            result.ShouldNotHaveValidationErrorFor(x => x.Name);
        }

        [Fact]
        public void Name_ExactlyAtMaxLength_ShouldNotHaveError()
        {
            var request = CreateRequest(name: new string('a', 100));
            var result = _validator.TestValidate(request);
            result.ShouldNotHaveValidationErrorFor(x => x.Name);
        }

        [Fact]
        public void ValidRequest_ShouldNotHaveAnyError()
        {
            var request = CreateRequest(
                file: CreateMockFile("photo.jpg", 1024 * 1024),
                name: "My Profile Picture");

            var result = _validator.TestValidate(request);
            result.ShouldNotHaveAnyValidationErrors();
        }
}
}
