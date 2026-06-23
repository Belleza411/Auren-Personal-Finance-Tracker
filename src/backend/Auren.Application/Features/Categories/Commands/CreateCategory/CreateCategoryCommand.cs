using Auren.Application.Features.Categories.DTOs;
using System;
using System.Collections.Generic;
using System.Text;

namespace Auren.Application.Features.Categories.Commands.CreateCategory
{
    public record CreateCategoryCommand(CategoryDto Dto, Guid UserId);
}
