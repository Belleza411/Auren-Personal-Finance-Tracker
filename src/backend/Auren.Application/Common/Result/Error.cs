namespace Auren.Application.Common.Result
{
	public record Error(ErrorTypes Code, IEnumerable<string>? Messages = null)
    {
        public ErrorTypes Code { get; } = Code;
        public IEnumerable<string> Messages { get; } = Messages?.ToList() ?? [];

        public static Error None => new(ErrorTypes.None, []);
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

        public static class UserError
        {
            public static Error EmailAlreadyInUse(params string[] messages) => new(ErrorTypes.EmailAlreadyInUse, messages);
            public static Error UserLockedOut(params string[] messages) => new(ErrorTypes.UserLockedOut, messages);
            public static Error LogoutFailed(params string[] messages) => new(ErrorTypes.LogoutFailed, messages);
        }
    }
}
