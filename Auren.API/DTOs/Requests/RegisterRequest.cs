namespace Auren.API.DTOs.Requests
{
	public sealed record RegisterRequest(
		string Email,
		string Password,
		string ConfirmPassword,
		string FirstName,
		string LastName
	);
}
