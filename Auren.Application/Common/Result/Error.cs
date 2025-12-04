namespace Auren.Application.Common.Result
{
	public class Error
	{
        public ErrorType Code { get; }
        public List<string> Messages { get; }

        public Error(ErrorType code, IEnumerable<string>? messages = null)
        {
            Code = code;
            Messages = messages?.ToList() ?? new List<string>();
        }

        public static Error None => new Error(ErrorType.None, Enumerable.Empty<string>());
        public static Error NotFound(params string[] messages) => new(ErrorType.NotFound, messages);
        public static Error TypeMismatch(params string[] messages) => new(ErrorType.TypeMismatch, messages);
        public static Error NotEnoughBalance(params string[] messages) => new(ErrorType.NotEnoughBalance, messages);
        public static Error ValidationFailed(params string[] messages) => new(ErrorType.ValidationFailed, messages);
        public static Error InvalidInput(params string[] messages) => new(ErrorType.InvalidInput, messages);
        public static Error UpdateFailed(params string[] messages) => new(ErrorType.UpdateFailed, messages);
        public static Error CreateFailed(params string[] messages) => new(ErrorType.CreateFailed, messages);
        public static Error DeleteFailed(params string[] messages) => new(ErrorType.DeleteFailed, messages);
        public static Error UploadFailed(params string[] messages) => new(ErrorType.UploadFailed, messages);

        public static class CategoryError
        {
            public static Error AlreadyExists(params string[] messages) => new(ErrorType.CategoryAlreadyExists, messages);
        }

        public static class GoalError
        {
            public static Error AmountMustBePositive(params string[] messages) => new(ErrorType.AmountMustBePositive, messages);
        }

        public static class UserError
        {
            public static Error EmailAlreadyInUse(params string[] messages) => new(ErrorType.EmailAlreadyInUse, messages);
            public static Error UserLockedOut(params string[] messages) => new(ErrorType.UserLockedOut, messages);
            public static Error LogoutFailed(params string[] messages) => new(ErrorType.LogoutFailed, messages);
        }
    }
      
    public class ResultWithError
    {
        public bool IsSuccess { get; }
        public bool IsFailure => !IsSuccess;
        public Error Error { get; }

        protected ResultWithError(bool isSuccess, Error error)
        {
            if (!isSuccess && error == null)
                throw new ArgumentNullException(nameof(error), "Failure result must have an Error instance.");

            IsSuccess = isSuccess;
            Error = isSuccess ? Error.None : error;
        }

        public static ResultWithError Success() => new(true, Error.None);
        public static ResultWithError Failure(Error error) => new(false, error);
    }
}
