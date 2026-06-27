using Auren.Application.Features.Categories.DTOs;

namespace Auren.Application.Features.Categories.Queries.GetCategories
{
    public record GetCategoriesQuery(
        Guid UserId,
        CategoriesFilter Filter,
        int PageSize,
        int PageNumber);
}
