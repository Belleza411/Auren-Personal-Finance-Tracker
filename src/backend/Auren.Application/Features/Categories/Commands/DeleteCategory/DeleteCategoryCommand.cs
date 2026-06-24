using System;
using System.Collections.Generic;
using System.Text;

namespace Auren.Application.Features.Categories.Commands.DeleteCategory
{
    public record DeleteCategoryCommand(Guid UserId, Guid CategoryId);
}
