namespace Auren.API.Helpers.Result
{
	public enum ErrorType
	{
		None = 1,
		NotFound = 2,
		TypeMismatch = 3,
		NotEnoughBalance = 4,
		ValidationFailed = 5,
		InvalidInput = 6,
		CategoryAlreadyExists = 7,
		UpdateFailed = 8
    }
}
