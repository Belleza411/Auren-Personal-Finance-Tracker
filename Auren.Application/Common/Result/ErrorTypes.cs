namespace Auren.Application.Common.Result
{
	public enum ErrorTypes
	{
		None = 1,
		NotFound = 2,
		TypeMismatch = 3,
		NotEnoughBalance = 4,
		ValidationFailed = 5,
		InvalidInput = 6,
		CategoryAlreadyExists = 7,
		UpdateFailed = 8,
		CreateFailed = 9,
		DeleteFailed = 10,
		AmountMustBePositive = 11,
		EmailAlreadyInUse = 12,
		UploadFailed = 13,
		UserLockedOut = 14,
		LogoutFailed = 15
    }
}
