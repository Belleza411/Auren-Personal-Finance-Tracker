namespace Auren.Application.Features.Auth.Commands.DeleteAccount
{
    public record DeleteAccountCommand(Guid UserId, string Password);
}
