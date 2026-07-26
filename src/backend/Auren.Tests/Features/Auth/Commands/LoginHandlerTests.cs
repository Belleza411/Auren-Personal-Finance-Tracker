using Auren.Application.Common.Interfaces;
using Auren.Application.Common.Result;
using Auren.Application.Features.Auth.Commands.Login;
using Auren.Application.Features.Auth.DTOs;
using Auren.Domain.Entities;
using Auren.Tests.Common.Helpers;
using FluentAssertions;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.AspNetCore.Identity;
using Moq;

namespace Auren.Tests.Features.Auth.Commands
{
    public class LoginHandlerTests
    {
        private readonly Mock<IIdentityService> _identity = new();
        private readonly Mock<IAuthHelper> _authHelper = new();
        private readonly Mock<IValidator<LoginRequest>> _validator = new();
        private readonly LoginHandler _handler;

        public LoginHandlerTests()
        {
            _handler = new LoginHandler(
                _identity.Object,
                _authHelper.Object,
                _validator.Object);
        }

        private void SetupValidValidator()
        {
            _validator
                .Setup(v => v.ValidateAsync(
                    It.IsAny<LoginRequest>(),
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
                    It.IsAny<LoginRequest>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ValidationResult(failures));
        }

        private void SetupSuccessfulSignIn()
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
                    "Login successfully")));
        }

        private static LoginRequest CreateRequest(
            string email = "test@test.com",
            string password = "Password123!") =>
            new(email, password);

        private static ApplicationUser CreateUser(
            string email = "test@test.com",
            string firstName = "John",
            string lastName = "Doe") =>
            Fakers.UserFaker()
                .RuleFor(u => u.Email, email)
                .RuleFor(u => u.UserName, email)
                .RuleFor(u => u.FirstName, firstName)
                .RuleFor(u => u.LastName, lastName)
                .Generate();


        [Fact]
        public async Task Handle_InvalidRequest_ReturnsValidationFailed()
        {
            var ct = TestContext.Current.CancellationToken;

            SetupInvalidValidator("Email is required.", "Password is required.");

            var result = await _handler.Handle(
                new LoginCommand(CreateRequest()), ct);

            result.IsFailure.Should().BeTrue();
            result.Error.Code.Should().Be(ErrorTypes.ValidationFailed);
            result.Error.Messages.Should().Contain("Email is required.");
            result.Error.Messages.Should().Contain("Password is required.");

            _identity.Verify(i => i.FindByEmailAsync(
                It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public async Task Handle_UserNotFound_ReturnsInvalidInput()
        {
            var ct = TestContext.Current.CancellationToken;

            SetupValidValidator();

            _identity
                .Setup(i => i.FindByEmailAsync(It.IsAny<string>()))
                .ReturnsAsync((ApplicationUser?)null);

            var result = await _handler.Handle(
                new LoginCommand(CreateRequest(email: "notfound@test.com")), ct);

            result.IsFailure.Should().BeTrue();
            result.Error.Code.Should().Be(ErrorTypes.InvalidInput);
            result.Error.Messages.Should().Contain("Invalid email or password.");

            _identity.Verify(i => i.CheckPasswordAsync(
                It.IsAny<ApplicationUser>(),
                It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public async Task Handle_WrongPassword_ReturnsInvalidInput()
        {
            var ct = TestContext.Current.CancellationToken;

            SetupValidValidator();

            var user = CreateUser();

            _identity
                .Setup(i => i.FindByEmailAsync(It.IsAny<string>()))
                .ReturnsAsync(user);

            _identity
                .Setup(i => i.CheckPasswordAsync(user, It.IsAny<string>()))
                .ReturnsAsync(SignInResult.Failed);

            var result = await _handler.Handle(
                new LoginCommand(CreateRequest(password: "WrongPassword!")), ct);

            result.IsFailure.Should().BeTrue();
            result.Error.Code.Should().Be(ErrorTypes.InvalidInput);
            result.Error.Messages.Should().Contain("Invalid email or password.");

            _authHelper.Verify(a => a.SignInAsync(
                It.IsAny<ApplicationUser>(),
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task Handle_AccountLockedOut_ReturnsUserLockedOut()
        {
            var ct = TestContext.Current.CancellationToken;

            SetupValidValidator();

            var user = CreateUser();

            _identity
                .Setup(i => i.FindByEmailAsync(It.IsAny<string>()))
                .ReturnsAsync(user);

            _identity
                .Setup(i => i.CheckPasswordAsync(user, It.IsAny<string>()))
                .ReturnsAsync(SignInResult.LockedOut);

            _identity
                .Setup(i => i.GetLockoutEndDateAsync(user))
                .ReturnsAsync(DateTimeOffset.UtcNow.AddMinutes(10));

            var result = await _handler.Handle(
                new LoginCommand(CreateRequest()), ct);

            result.IsFailure.Should().BeTrue();
            result.Error.Code.Should().Be(ErrorTypes.UserLockedOut);
            result.Error.Messages.First().Should().Contain("Account locked.");
            result.Error.Messages.First().Should().Contain("minutes");
        }

        [Fact]
        public async Task Handle_AccountLockedOutExpired_ReturnsGenericLockedMessage()
        {
            var ct = TestContext.Current.CancellationToken;

            SetupValidValidator();

            var user = CreateUser();

            _identity
                .Setup(i => i.FindByEmailAsync(It.IsAny<string>()))
                .ReturnsAsync(user);

            _identity
                .Setup(i => i.CheckPasswordAsync(user, It.IsAny<string>()))
                .ReturnsAsync(SignInResult.LockedOut);

            _identity
                .Setup(i => i.GetLockoutEndDateAsync(user))
                .ReturnsAsync(DateTimeOffset.UtcNow.AddMinutes(-1));

            var result = await _handler.Handle(
                new LoginCommand(CreateRequest()), ct);

            result.IsFailure.Should().BeTrue();
            result.Error.Code.Should().Be(ErrorTypes.UserLockedOut);
            result.Error.Messages.Should().Contain("Account is temporarily locked.");
        }

        [Fact]
        public async Task Handle_AccountLockedOut_DoesNotCallSignIn()
        {
            var ct = TestContext.Current.CancellationToken;

            SetupValidValidator();

            var user = CreateUser();

            _identity
                .Setup(i => i.FindByEmailAsync(It.IsAny<string>()))
                .ReturnsAsync(user);

            _identity
                .Setup(i => i.CheckPasswordAsync(user, It.IsAny<string>()))
                .ReturnsAsync(SignInResult.LockedOut);

            _identity
                .Setup(i => i.GetLockoutEndDateAsync(user))
                .ReturnsAsync(DateTimeOffset.UtcNow.AddMinutes(10));

            await _handler.Handle(new LoginCommand(CreateRequest()), ct);

            _authHelper.Verify(a => a.SignInAsync(
                It.IsAny<ApplicationUser>(),
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task Handle_ValidCredentials_ReturnsSuccess()
        {
            var ct = TestContext.Current.CancellationToken;

            SetupValidValidator();
            SetupSuccessfulSignIn();

            var user = CreateUser();

            _identity
                .Setup(i => i.FindByEmailAsync(It.IsAny<string>()))
                .ReturnsAsync(user);

            _identity
                .Setup(i => i.CheckPasswordAsync(user, It.IsAny<string>()))
                .ReturnsAsync(SignInResult.Success);

            _identity
                .Setup(i => i.UpdateAsync(It.IsAny<ApplicationUser>()))
                .ReturnsAsync(IdentityResult.Success);

            var result = await _handler.Handle(
                new LoginCommand(CreateRequest()), ct);

            result.IsSuccess.Should().BeTrue();
            result.Value.Message.Should().Be("Login successfully");
            result.Value.User.Should().NotBeNull();
        }

        [Fact]
        public async Task Handle_ValidCredentials_UpdatesLastLoginAt()
        {
            var ct = TestContext.Current.CancellationToken;

            SetupValidValidator();
            SetupSuccessfulSignIn();

            var user = CreateUser();
            var beforeLogin = DateTime.UtcNow;

            _identity
                .Setup(i => i.FindByEmailAsync(It.IsAny<string>()))
                .ReturnsAsync(user);

            _identity
                .Setup(i => i.CheckPasswordAsync(user, It.IsAny<string>()))
                .ReturnsAsync(SignInResult.Success);

            _identity
                .Setup(i => i.UpdateAsync(It.IsAny<ApplicationUser>()))
                .ReturnsAsync(IdentityResult.Success);

            await _handler.Handle(new LoginCommand(CreateRequest()), ct);

            user.LastLoginAt.Should().NotBeNull();
            user.LastLoginAt.Should().BeOnOrAfter(beforeLogin);

            _identity.Verify(i => i.UpdateAsync(user), Times.Once);
        }

        [Fact]
        public async Task Handle_ValidCredentials_CallsSignIn()
        {
            var ct = TestContext.Current.CancellationToken;

            SetupValidValidator();
            SetupSuccessfulSignIn();

            var user = CreateUser();

            _identity
                .Setup(i => i.FindByEmailAsync(It.IsAny<string>()))
                .ReturnsAsync(user);

            _identity
                .Setup(i => i.CheckPasswordAsync(user, It.IsAny<string>()))
                .ReturnsAsync(SignInResult.Success);

            _identity
                .Setup(i => i.UpdateAsync(It.IsAny<ApplicationUser>()))
                .ReturnsAsync(IdentityResult.Success);

            await _handler.Handle(new LoginCommand(CreateRequest()), ct);

            _authHelper.Verify(a => a.SignInAsync(
                user,
                "Login successfully",
                It.IsAny<CancellationToken>()), Times.Once);
        }
        [Fact]
        public async Task Handle_UserNotFound_ReturnsSameMessageAsWrongPassword()
        {
            var ct = TestContext.Current.CancellationToken;

            SetupValidValidator();

            _identity
                .Setup(i => i.FindByEmailAsync(It.IsAny<string>()))
                .ReturnsAsync((ApplicationUser?)null);

            var notFoundResult = await _handler.Handle(
                new LoginCommand(CreateRequest()), ct);

            var user = CreateUser();
            _identity
                .Setup(i => i.FindByEmailAsync(It.IsAny<string>()))
                .ReturnsAsync(user);

            _identity
                .Setup(i => i.CheckPasswordAsync(user, It.IsAny<string>()))
                .ReturnsAsync(SignInResult.Failed);

            var wrongPasswordResult = await _handler.Handle(
                new LoginCommand(CreateRequest()), ct);

            notFoundResult.Error.Messages.First()
                .Should().Be(wrongPasswordResult.Error.Messages.First());
        }
    }
}