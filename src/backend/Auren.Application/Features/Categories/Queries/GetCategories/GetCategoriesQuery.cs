using Auren.Application.Features.Categories.DTOs;
using System;
using System.Collections.Generic;
using System.Text;

namespace Auren.Application.Features.Categories.Queries.GetCategories
{
    public record GetCategoriesQuery(
        Guid UserId,
        CategoriesFilter Filter,
        int PageSize,
        int PageNumber);
}
