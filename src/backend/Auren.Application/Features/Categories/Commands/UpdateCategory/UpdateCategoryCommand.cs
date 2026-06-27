using Auren.Application.Features.Categories.DTOs;

namespace Auren.Application.Features.Categories.Commands.UpdateCategory
{
    public record UpdateCategoryCommand(Guid CategoryId, Guid UserId, CategoryDto Dto);
}
