using Auren.Application.Common.Interfaces;
using Auren.Application.Common.Result;
using Auren.Application.Constants;
using Auren.Application.Features.Auth.Commands.Register;
using Auren.Application.Features.Auth.DTOs;
using Auren.Application.Features.Profile.DTOs;
using Auren.Domain.Entities;
using Auren.Infrastructure.Persistence;
using Auren.Tests.Common.Helpers;
using FluentAssertions;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Moq;

namespace Auren.Tests.Features.Auth.Commands
{
    public class RegisterHandlerTests
    {
        private readonly AurenDbContext _appDb = TestDbContextFactory.CreateAppDb();
        private readonly Mock<IIdentityService> _identity = new();
        private readonly Mock<ICloudinaryService> _cloudinary = new();
        private readonly Mock<IAuthHelper> _authHelper = new();
        private readonly Mock<IValidator<RegisterRequest>> _validator = new();
        private readonly RegisterHandler _handler;

        public RegisterHandlerTests()
        {
            _handler = new RegisterHandler(
                _identity.Object,
                _appDb,
                _cloudinary.Object,
                _authHelper.Object,
                _validator.Object);
        }

        private void SetupValidValidator()
        {
            _validator
                .Setup(v => v.ValidateAsync(
                    It.IsAny<RegisterRequest>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ValidationResult());
        }

        private void SetupInvalidValidator(params string[] errors)
        {
            var failures = errors
                .Select(e => new ValidationFailure("field", e))
                .ToList();

            _validator
                .Setup(v => v.ValidateAsync(
                    It.IsAny<RegisterRequest>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ValidationResult(failures));
        }

        private void SetupUserNotFound()
        {
            _identity
                .Setup(i => i.FindByEmailAsync(It.IsAny<string>()))
                .ReturnsAsync((ApplicationUser?)null);
        }

        private void SetupUserFound()
        {
            _identity
                .Setup(i => i.FindByEmailAsync(It.IsAny<string>()))
                .ReturnsAsync(Fakers.UserFaker().Generate());
        }

        private void SetupSuccessfulCreateUser()
        {
            _identity
                .Setup(i => i.CreateAsync(
                    It.IsAny<ApplicationUser>(),
                    It.IsAny<string>()))
                .ReturnsAsync(IdentityResult.Success);
        }

        private void SetupFailedCreateUser(params string[] errors)
        {
            var identityErrors = errors
                .Select(e => new IdentityError { Description = e })
                .ToArray();

            _identity
                .Setup(i => i.CreateAsync(
                    It.IsAny<ApplicationUser>(),
                    It.IsAny<string>()))
                .ReturnsAsync(IdentityResult.Failed(identityErrors));
        }

        private void SetupSuccessfulSignIn(string message = "Registered successfully")
        {
            _authHelper
                .Setup(a => a.SignInAsync(
                    It.IsAny<ApplicationUser>(),
                    It.IsAny<string>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(Result.Success(new AuthResponse(
                    new UserResponse(
                        "test@test.com",
                        "John",
                        "Doe",
                        null,
                        DateTime.UtcNow,
                        null),
                    message)));
        }

        private static RegisterRequest CreateRequest(
            string email = "john@test.com",
            string password = "Password123!",
            string confirmPassword = "Password123!",
            string firstName = "John",
            string lastName = "Doe",
            ProfileImageUploadRequest? profileImage = null) =>
                new(email, password, confirmPassword, firstName, lastName, profileImage);

        [Fact]
        public async Task Handle_InvalidRequest_ReturnsValidationFailed()
        {
            var ct = TestContext.Current.CancellationToken;

            SetupInvalidValidator("Email is required.", "Password is required.");

            var result = await _handler.Handle(
                new RegisterCommand(CreateRequest()), ct);

            result.IsFailure.Should().BeTrue();
            result.Error.Code.Should().Be(ErrorTypes.ValidationFailed);
            result.Error.Messages.Should().Contain("Email is required.");
            result.Error.Messages.Should().Contain("Password is required.");

            _identity.Verify(i => i.FindByEmailAsync(
                It.IsAny<string>()), Times.Never);
            _identity.Verify(i => i.CreateAsync(
                It.IsAny<ApplicationUser>(),
                It.IsAny<string>()), Times.Never);
        }


        [Fact]
        public async Task Handle_EmailAlreadyExists_ReturnsEmailAlreadyInUse()
        {
            var ct = TestContext.Current.CancellationToken;

            SetupValidValidator();
            SetupUserFound();

            var result = await _handler.Handle(
                new RegisterCommand(CreateRequest(email: "existing@test.com")), ct);

            result.IsFailure.Should().BeTrue();
            result.Error.Code.Should().Be(ErrorTypes.EmailAlreadyInUse);
            result.Error.Messages.Should().Contain("Email already exists.");

            _identity.Verify(i => i.CreateAsync(
                It.IsAny<ApplicationUser>(),
                It.IsAny<string>()), Times.Never);
        }


        [Fact]
        public async Task Handle_CreateUserFailed_ReturnsCreateFailed()
        {
            var ct = TestContext.Current.CancellationToken;

            SetupValidValidator();
            SetupUserNotFound();
            SetupFailedCreateUser("Password too weak.", "Email is invalid.");

            var result = await _handler.Handle(
                new RegisterCommand(CreateRequest()), ct);

            result.IsFailure.Should().BeTrue();
            result.Error.Code.Should().Be(ErrorTypes.CreateFailed);
            result.Error.Messages.Should().Contain("Password too weak.");
            result.Error.Messages.Should().Contain("Email is invalid.");

            var categories = await _appDb.Categories.ToListAsync(ct);
            categories.Should().BeEmpty();
        }

        [Fact]
        public async Task Handle_ValidRequest_ReturnsSuccess()
        {
            var ct = TestContext.Current.CancellationToken;

            SetupValidValidator();
            SetupUserNotFound();
            SetupSuccessfulCreateUser();
            SetupSuccessfulSignIn();

            var result = await _handler.Handle(
                new RegisterCommand(CreateRequest(
                    email: "john@test.com",
                    firstName: "John",
                    lastName: "Doe")), ct);

            result.IsSuccess.Should().BeTrue();
            result.Value.Message.Should().Be("Registered successfully");
            result.Value.User.Should().NotBeNull();
        }

        [Fact]
        public async Task Handle_ValidRequest_SeedsDefaultCategories()
        {
            var ct = TestContext.Current.CancellationToken;

            SetupValidValidator();
            SetupUserNotFound();
            SetupSuccessfulCreateUser();
            SetupSuccessfulSignIn();

            await _handler.Handle(
                new RegisterCommand(CreateRequest()), ct);

            var categories = await _appDb.Categories.ToListAsync(ct);

            categories.Should().NotBeEmpty();
            categories.Count.Should().Be(CategorySeeder.DefaultCategories.Count);

        }

        [Fact]
        public async Task Handle_ValidRequest_CallsSignIn()
        {
            var ct = TestContext.Current.CancellationToken;

            SetupValidValidator();
            SetupUserNotFound();
            SetupSuccessfulCreateUser();
            SetupSuccessfulSignIn();

            await _handler.Handle(
                new RegisterCommand(CreateRequest()), ct);

            _authHelper.Verify(a => a.SignInAsync(
                It.IsAny<ApplicationUser>(),
                "Registered successfully",
                It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task Handle_HtmlInName_SanitizesInput()
        {
            var ct = TestContext.Current.CancellationToken;

            SetupValidValidator();
            SetupUserNotFound();
            SetupSuccessfulSignIn();

            ApplicationUser? capturedUser = null;
            _identity
                .Setup(i => i.CreateAsync(
                    It.IsAny<ApplicationUser>(),
                    It.IsAny<string>()))
                .Callback<ApplicationUser, string>((u, _) => capturedUser = u)
                .ReturnsAsync(IdentityResult.Success);

            await _handler.Handle(
                new RegisterCommand(CreateRequest(
                    firstName: "<script>alert('xss')</script>",
                    lastName: "<b>Doe</b>")), ct);

            capturedUser.Should().NotBeNull();
            capturedUser!.FirstName.Should().NotContain("<script>");
            capturedUser.FirstName.Should().NotContain("</script>");
            capturedUser.LastName.Should().NotContain("<b>");
            capturedUser.LastName.Should().NotContain("</b>");
        }

        [Fact]
        public async Task Handle_WhitespaceInName_TrimsInput()
        {
            var ct = TestContext.Current.CancellationToken;

            SetupValidValidator();
            SetupUserNotFound();
            SetupSuccessfulSignIn();

            ApplicationUser? capturedUser = null;
            _identity
                .Setup(i => i.CreateAsync(
                    It.IsAny<ApplicationUser>(),
                    It.IsAny<string>()))
                .Callback<ApplicationUser, string>((u, _) => capturedUser = u)
                .ReturnsAsync(IdentityResult.Success);

            await _handler.Handle(
                new RegisterCommand(CreateRequest(
                    firstName: "  John  ",
                    lastName: "  Doe  ")), ct);

            capturedUser.Should().NotBeNull();
            capturedUser!.FirstName.Should().NotStartWith(" ");
            capturedUser.FirstName.Should().NotEndWith(" ");
            capturedUser.LastName.Should().NotStartWith(" ");
            capturedUser.LastName.Should().NotEndWith(" ");
        }

        [Fact]
        public async Task Handle_WithProfileImage_UploadsAndSetsUrl()
        {
            var ct = TestContext.Current.CancellationToken;

            SetupValidValidator();
            SetupUserNotFound();
            SetupSuccessfulSignIn();

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

            ApplicationUser? capturedUser = null;
            _identity
                .Setup(i => i.CreateAsync(
                    It.IsAny<ApplicationUser>(),
                    It.IsAny<string>()))
                .Callback<ApplicationUser, string>((u, _) => capturedUser = u)
                .ReturnsAsync(IdentityResult.Success);

            await _handler.Handle(
                new RegisterCommand(CreateRequest(
                    profileImage: new ProfileImageUploadRequest(
                        File: fileMock.Object,
                        Name: "photo",
                        Description: null))), ct);

            capturedUser!.ProfilePictureUrl
                .Should().Be("https://cloudinary.com/photo.jpg");

            _cloudinary.Verify(c => c.UploadProfileImageAsync(
                It.IsAny<ProfileImageUploadRequest>(),
                It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task Handle_WithoutProfileImage_DoesNotCallCloudinary()
        {
            var ct = TestContext.Current.CancellationToken;

            SetupValidValidator();
            SetupUserNotFound();
            SetupSuccessfulCreateUser();
            SetupSuccessfulSignIn();

            await _handler.Handle(
                new RegisterCommand(CreateRequest(profileImage: null)), ct);

            _cloudinary.Verify(c => c.UploadProfileImageAsync(
                It.IsAny<ProfileImageUploadRequest>(),
                It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task Handle_CloudinaryReturnsNull_ReturnsUploadFailed()
        {
            var ct = TestContext.Current.CancellationToken;

            SetupValidValidator();
            SetupUserNotFound();

            var fileMock = new Mock<IFormFile>();
            fileMock.Setup(f => f.FileName).Returns("photo.jpg");

            _cloudinary
                .Setup(c => c.UploadProfileImageAsync(
                    It.IsAny<ProfileImageUploadRequest>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync((ProfileImageUploadResponse?)null);

            var result = await _handler.Handle(
                new RegisterCommand(CreateRequest(
                    profileImage: new ProfileImageUploadRequest(
                        File: fileMock.Object,
                        Name: "photo",
                        Description: null))), ct);

            result.IsFailure.Should().BeTrue();
            result.Error.Code.Should().Be(ErrorTypes.UploadFailed);
            result.Error.Messages.Should().Contain("Failed to upload image.");

            _identity.Verify(i => i.CreateAsync(
                It.IsAny<ApplicationUser>(),
                It.IsAny<string>()), Times.Never);

            var categories = await _appDb.Categories.ToListAsync(ct);
            categories.Should().BeEmpty();
        }

        [Fact]
        public async Task Handle_CreateFailed_DoesNotSeedCategories()
        {
            var ct = TestContext.Current.CancellationToken;

            SetupValidValidator();
            SetupUserNotFound();
            SetupFailedCreateUser("Password too weak.");

            await _handler.Handle(
                new RegisterCommand(CreateRequest()), ct);

            var categories = await _appDb.Categories.ToListAsync(ct);
            categories.Should().BeEmpty(); 
        }

        [Fact]
        public async Task Handle_CreateFailed_DoesNotCallSignIn()
        {
            var ct = TestContext.Current.CancellationToken;

            SetupValidValidator();
            SetupUserNotFound();
            SetupFailedCreateUser("Password too weak.");

            await _handler.Handle(
                new RegisterCommand(CreateRequest()), ct);

            _authHelper.Verify(a => a.SignInAsync(
                It.IsAny<ApplicationUser>(),
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()), Times.Never);
        }
    }
}