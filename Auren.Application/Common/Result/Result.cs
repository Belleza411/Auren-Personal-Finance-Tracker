namespace Auren.Application.Common.Result
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

        public static Result Success() => new(true, Error.None);
        public static Result Failure(Error error) => new(false, error);

        public static Result<T> Success<T>(T value) => new(value, true, Error.None);
        public static Result<T> Failure<T>(Error error) => new(default!, false, error);
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
