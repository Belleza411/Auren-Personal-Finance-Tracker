namespace Auren.Application.Features.Categories.Queries.GetCategoryById
{
    public record GetCategoryByIdCommand(Guid UserId, Guid CategoryId);
}
