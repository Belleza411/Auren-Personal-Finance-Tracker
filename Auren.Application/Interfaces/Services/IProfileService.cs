using Auren.Application.Common.Result;
using Auren.Application.DTOs.Requests;
using Auren.Application.DTOs.Responses.User;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Auren.Application.Interfaces.Services
{
	public interface IProfileService
	{
        Task<Result<UserResponse>> GetUserProfile(Guid userId, CancellationToken cancellationToken);
        Task<Result<UserResponse>> UpdateUserProfile(Guid userId, UserDto userDto, CancellationToken cancellationToken);
    }
}
