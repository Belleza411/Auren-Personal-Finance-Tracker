using System;
using System.Collections.Generic;
using System.Text;

namespace Auren.Application.Features.Categories.Queries.GetCategoryById
{
    public record GetCategoryByIdCommand(Guid UserId, Guid CategoryId);
}
