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
        public TOut Match<TOut>(
            Func<TOut> onSuccess,
            Func<Error, TOut> onFailure)
                => IsSuccess ? onSuccess() : onFailure(Error);
    }

    public class Result<T> : Result
    {
        public T Value { get; }

        protected internal Result(T value, bool isSuccess, Error error)
            : base(isSuccess, error)
        {
            Value = value;
        }

        public Result<TNew> Map<TNew>(Func<T, TNew> mapper)
            => IsSuccess
                ? Result.Success(mapper(Value))
                : Result.Failure<TNew>(Error);

        public Result<TNew> Bind<TNew>(Func<T, Result<TNew>> binder)
            => IsSuccess
                ? binder(Value)
                : Result.Failure<TNew>(Error);

        public TOut Match<TOut>(
            Func<T, TOut> onSuccess,
            Func<Error, TOut> onFailure)
                => IsSuccess ? onSuccess(Value) : onFailure(Error);
    }
}
