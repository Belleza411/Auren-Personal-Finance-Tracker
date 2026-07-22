using Auren.Application.Common.Interfaces;
using Auren.Application.Common.Result;
using Auren.Application.Features.Auth.DTOs;
using Auren.Application.Features.Profile.Commands.UpdateProfile;
using Auren.Application.Features.Profile.DTOs;
using Auren.Infrastructure.Persistence;
using Auren.Tests.Common.Helpers;
using FluentAssertions;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.AspNetCore.Http;
using Moq;

namespace Auren.Tests.Features.Profile
{
    public class UpdateProfileHandlerTests
    {
        private readonly AurenAuthDbContext _db = TestDbContextFactory.CreateAuthDb();
        private readonly Mock<ICloudinaryService> _cloudinary = new();
        private readonly Mock<IValidator<UserDto>> _validator = new();
        private readonly UpdateProfileHandler _handler;

        public UpdateProfileHandlerTests()
        {
            _handler = new UpdateProfileHandler(_db, _cloudinary.Object, _validator.Object);
        }

        private static UserDto CreateDto(
            string? email = null,
            string? firstName = null,
            string? lastName = null,
            ProfileImageUploadRequest? profilePicture = null,
            string? currency = null) =>
                new(email, firstName, lastName, profilePicture, currency);

        private void SetupValidValidator()
        {
            _validator
                .Setup(v => v.ValidateAsync(It.IsAny<UserDto>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ValidationResult());
        }

        private void SetupInvalidValidator(params string[] errors)
        {
            var failures = errors.Select(e => new ValidationFailure("field", e)).ToList();
            _validator
                .Setup(v => v.ValidateAsync(It.IsAny<UserDto>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ValidationResult(failures));
        }


        [Fact]
        public async Task Handle_InvalidDto_ReturnsValidationFailed()
        {
            var ct = TestContext.Current.CancellationToken;

            SetupInvalidValidator("Email is required.");

            var result = await _handler.Handle(
                new UpdateProfileCommand(Guid.NewGuid(), CreateDto()), ct);

            result.IsFailure.Should().BeTrue();
            result.Error.Code.Should().Be(ErrorTypes.ValidationFailed);
            result.Error.Messages.Should().Contain("Email is required.");
        }


        [Fact]
        public async Task Handle_UserNotFound_ReturnsNotFound()
        {
            var ct = TestContext.Current.CancellationToken;

            SetupValidValidator();

            var result = await _handler.Handle(
                new UpdateProfileCommand(Guid.NewGuid(), CreateDto()), ct);

            result.IsFailure.Should().BeTrue();
            result.Error.Code.Should().Be(ErrorTypes.NotFound);
            result.Error.Messages.Should().Contain("User not found.");
        }

        [Fact]
        public async Task Handle_ValidDto_UpdatesUserFields()
        {
            var ct = TestContext.Current.CancellationToken;

            var user = Fakers.UserFaker().Generate();
            await _db.Users.AddAsync(user, ct);
            await _db.SaveChangesAsync(ct);

            SetupValidValidator();

            var dto = CreateDto(
                email: "johndoe@gmail.com",
                firstName: "John",
                lastName: "Doe",
                currency: "USD");

            var result = await _handler.Handle(
                new UpdateProfileCommand(user.Id, dto), ct);

            result.IsSuccess.Should().BeTrue();
            result.Value.Email.Should().Be("johndoe@gmail.com");
            result.Value.FirstName.Should().Be("John");
            result.Value.LastName.Should().Be("Doe");

            var updated = await _db.Users.FindAsync([user.Id], ct);
            updated!.Email.Should().Be("johndoe@gmail.com");
            updated.FirstName.Should().Be("John");
            updated.LastName.Should().Be("Doe");
            updated.Currency.Should().Be("USD");
        }

        [Fact]
        public async Task Handle_NullFields_KeepsExistingValues()
        {
            var ct = TestContext.Current.CancellationToken;

            var user = Fakers.UserFaker()
                .RuleFor(u => u.Email, "original@test.com")
                .RuleFor(u => u.FirstName, "Original")
                .RuleFor(u => u.LastName, "Name")
                .Generate();

            await _db.Users.AddAsync(user, ct);
            await _db.SaveChangesAsync(ct);

            SetupValidValidator();


            var dto = CreateDto
            (
                email: null,
                firstName: null,
                lastName: null,
                profilePicture: null,
                currency: null
            );

            var result = await _handler.Handle(
                new UpdateProfileCommand(user.Id, dto), ct);

            result.IsSuccess.Should().BeTrue();
            result.Value.Email.Should().Be("original@test.com");   
            result.Value.FirstName.Should().Be("Original");        
            result.Value.LastName.Should().Be("Name");              
        }

        [Fact]
        public async Task Handle_WithProfileImage_UploadsAndUpdatesUrl()
        {
            var ct = TestContext.Current.CancellationToken;

            var user = Fakers.UserFaker().Generate();
            await _db.Users.AddAsync(user, ct);
            await _db.SaveChangesAsync(ct);

            SetupValidValidator();

            var fileMock = new Mock<IFormFile>();
            fileMock.Setup(f => f.FileName).Returns("photo.jpg");
            fileMock.Setup(f => f.Length).Returns(1024);

            _cloudinary
                .Setup(c => c.UploadProfileImageAsync(
                    It.IsAny<ProfileImageUploadRequest>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ProfileImageUploadResponse(
                    Name: "photo.jpg",
                    Description: null,
                    Extension: ".jpg",
                    SizeInBytes: 1024,
                    Path: "https://cloudinary.com/photo.jpg"));

            var dto = CreateDto(
                email: null,
                firstName: null,
                lastName: null,
                profilePicture: new ProfileImageUploadRequest(
                    File: fileMock.Object,
                    Name: "photo",
                    Description: null),
                currency: null
            );

            var result = await _handler.Handle(
                new UpdateProfileCommand(user.Id, dto), ct);

            result.IsSuccess.Should().BeTrue();
            result.Value.ProfilePictureUrl.Should().Be("https://cloudinary.com/photo.jpg");

            _cloudinary.Verify(c => c.UploadProfileImageAsync(
                It.IsAny<ProfileImageUploadRequest>(),
                It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task Handle_WithoutProfileImage_DoesNotCallCloudinary()
        {
            var ct = TestContext.Current.CancellationToken;

            var user = Fakers.UserFaker().Generate();
            await _db.Users.AddAsync(user, ct);
            await _db.SaveChangesAsync(ct);

            SetupValidValidator();

            var dto = CreateDto
            (
                email: "test@example.com",
                firstName: null,
                lastName: null,
                profilePicture: null,
                currency: null
            );

            var result = await _handler.Handle(
                new UpdateProfileCommand(user.Id, dto), ct);

            result.IsSuccess.Should().BeTrue();
            _cloudinary.Verify(c => c.UploadProfileImageAsync(
                It.IsAny<ProfileImageUploadRequest>(),
                It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task Handle_CloudinaryReturnsEmptyPath_DoesNotUpdateProfileUrl()
        {
            var ct = TestContext.Current.CancellationToken;

            var user = Fakers.UserFaker()
                .RuleFor(u => u.ProfilePictureUrl, "https://existing-url.com/photo.jpg")
                .Generate();

            await _db.Users.AddAsync(user, ct);
            await _db.SaveChangesAsync(ct);

            SetupValidValidator();

            var fileMock = new Mock<IFormFile>();
            fileMock.Setup(f => f.FileName).Returns("photo.jpg");
            _cloudinary
                .Setup(c => c.UploadProfileImageAsync(
                    It.IsAny<ProfileImageUploadRequest>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ProfileImageUploadResponse(
                    Name: "photo.jpg",
                    Description: null,
                    Extension: ".jpg",
                    SizeInBytes: 1024,
                    Path: string.Empty));  

            var dto = CreateDto
            (
                email: null,
                firstName: null,
                lastName: null,
                profilePicture: new ProfileImageUploadRequest(
                    File: fileMock.Object,
                    Name: "photo",
                    Description: null),
                currency: null
            );

            var result = await _handler.Handle(
                new UpdateProfileCommand(user.Id, dto), ct);

            result.IsSuccess.Should().BeTrue();

            result.Value.ProfilePictureUrl.Should()
                .Be("https://existing-url.com/photo.jpg");
        }
    }
}
