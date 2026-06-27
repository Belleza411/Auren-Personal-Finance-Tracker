namespace Auren.Application.Features.Categories.Commands.DeleteCategory
{
    public record DeleteCategoryCommand(Guid UserId, Guid CategoryId);
}
