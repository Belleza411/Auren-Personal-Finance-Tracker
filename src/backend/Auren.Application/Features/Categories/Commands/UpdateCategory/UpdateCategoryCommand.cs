using Auren.Application.Features.Categories.DTOs;
using System;
using System.Collections.Generic;
using System.Text;

namespace Auren.Application.Features.Categories.Commands.UpdateCategory
{
    public record UpdateCategoryCommand(Guid CategoryId, Guid UserId, CategoryDto Dto);
}
