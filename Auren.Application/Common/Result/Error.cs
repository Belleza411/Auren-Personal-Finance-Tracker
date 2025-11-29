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
        public static Error NotFound(params string[] messages) => new Error(ErrorType.NotFound, messages);
        public static Error TypeMismatch(params string[] messages) => new Error(ErrorType.TypeMismatch, messages);
        public static Error NotEnoughBalance(params string[] messages) => new Error(ErrorType.NotEnoughBalance, messages);
        public static Error ValidationFailed(params string[] messages) => new Error(ErrorType.ValidationFailed, messages);
        public static Error InvalidInput(params string[] messages) => new Error(ErrorType.InvalidInput, messages);
        public static Error UpdateFailed(params string[] messages) => new Error(ErrorType.UpdateFailed, messages);
        public static Error CreateFailed(params string[] messages) => new Error(ErrorType.CreateFailed, messages);
        public static Error DeleteFailed(params string[] messages) => new Error(ErrorType.DeleteFailed, messages);

        public static class CategoryError
        {
            public static Error AlreadyExists(params string[] messages) => new Error(ErrorType.CategoryAlreadyExists, messages);
        }

        public static class GoalError
        {
            public static Error AmountMustBePositive(params string[] messages) => new Error(ErrorType.AmountMustBePositive, messages);
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

        public static ResultWithError Success() => new ResultWithError(true, Error.None);
        public static ResultWithError Failure(Error error) => new ResultWithError(false, error);
    }
}
