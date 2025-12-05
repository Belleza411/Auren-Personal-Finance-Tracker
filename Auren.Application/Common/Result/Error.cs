namespace Auren.Application.Common.Result
{
	public class Error
	{
        public ErrorTypes Code { get; }
        public List<string> Messages { get; }

        public Error(ErrorTypes code, IEnumerable<string>? messages = null)
        {
            Code = code;
            Messages = messages?.ToList() ?? new List<string>();
        }

        public static Error None => new Error(ErrorTypes.None, Enumerable.Empty<string>());
        public static Error NotFound(params string[] messages) => new(ErrorTypes.NotFound, messages);
        public static Error TypeMismatch(params string[] messages) => new(ErrorTypes.TypeMismatch, messages);
        public static Error NotEnoughBalance(params string[] messages) => new(ErrorTypes.NotEnoughBalance, messages);
        public static Error ValidationFailed(params string[] messages) => new(ErrorTypes.ValidationFailed, messages);
        public static Error InvalidInput(params string[] messages) => new(ErrorTypes.InvalidInput, messages);
        public static Error UpdateFailed(params string[] messages) => new(ErrorTypes.UpdateFailed, messages);
        public static Error CreateFailed(params string[] messages) => new(ErrorTypes.CreateFailed, messages);
        public static Error DeleteFailed(params string[] messages) => new(ErrorTypes.DeleteFailed, messages);
        public static Error UploadFailed(params string[] messages) => new(ErrorTypes.UploadFailed, messages);

        public static class CategoryError
        {
            public static Error AlreadyExists(params string[] messages) => new(ErrorTypes.CategoryAlreadyExists, messages);
        }

        public static class GoalError
        {
            public static Error AmountMustBePositive(params string[] messages) => new(ErrorTypes.AmountMustBePositive, messages);
        }

        public static class UserError
        {
            public static Error EmailAlreadyInUse(params string[] messages) => new(ErrorTypes.EmailAlreadyInUse, messages);
            public static Error UserLockedOut(params string[] messages) => new(ErrorTypes.UserLockedOut, messages);
            public static Error LogoutFailed(params string[] messages) => new(ErrorTypes.LogoutFailed, messages);
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
