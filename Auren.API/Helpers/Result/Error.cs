namespace Auren.API.Helpers.Result
{
	public class Error
	{
        public string Code { get; }
        public List<string> Messages { get; }

        public Error(string code, IEnumerable<string>? messages = null)
        {
            Code = code ?? string.Empty;
            Messages = messages?.ToList() ?? new List<string>();
        }

        public static Error None => new Error(string.Empty, Enumerable.Empty<string>());
        public static Error NotFound(params string[] messages) => new Error("NOT_FOUND", messages);
        public static Error TypeMismatch(params string[] messages) => new Error("TYPE_MISMATCH", messages);
        public static Error NotEnoughBalance(params string[] messages) => new Error("NOT_ENOUGH_BALANCE", messages);
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
        public static ResultWithError Failure(string code, params string[] messages) =>
            new ResultWithError(false, new Error(code, messages));
    }
}
