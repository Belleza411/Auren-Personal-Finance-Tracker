namespace Auren.API.Helpers.Result
{
	public class Result
	{
        public bool IsSuccess { get; }
        public bool IsFailure => !IsSuccess;
        public Error Error { get; }

        protected Result(bool isSuccess, Error error)
        {
            if (!isSuccess && error == null)
                throw new ArgumentNullException(nameof(error), "Failure result must have an Error instance.");
            IsSuccess = isSuccess;
            Error = IsSuccess ? Error.None : error;
        }

        public static Result Success() => new Result(true, Error.None);
        public static Result Failure(Error error) => new Result(false, error);
        public static Result Failure(string code, params string[] messages) =>
            new Result(false, new Error(code, messages));

        public static Result<T> Success<T>(T value) => new Result<T>(value, true, Error.None);
        public static Result<T> Failure<T>(Error error) => new Result<T>(default!, false, error);
        public static Result<T> Failure<T>(string code, params string[] messages) =>
            new Result<T>(default!, false, new Error(code, messages));
    }

    public class Result<T> : Result
    {
        public T Value { get; }

        protected internal Result(T value, bool isSuccess, Error error)
            : base(isSuccess, error)
        {
            Value = value;
        }
    }
}
