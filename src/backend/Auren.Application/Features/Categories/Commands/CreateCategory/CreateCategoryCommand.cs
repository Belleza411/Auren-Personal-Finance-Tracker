using Auren.Application.Features.Categories.DTOs;

namespace Auren.Application.Features.Categories.Commands.CreateCategory
{
    public record CreateCategoryCommand(CategoryDto Dto, Guid UserId);
}
