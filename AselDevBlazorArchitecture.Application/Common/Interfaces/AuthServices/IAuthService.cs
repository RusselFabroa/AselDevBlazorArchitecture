using AselDevBlazorArchitecture.Application.Features.Auth.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AselDevBlazorArchitecture.Application.Common.Interfaces.AuthServices
{
    public interface IAuthService
    {
        Task<ServiceResponse<AuthResponseDto>> LoginAsync(LoginDto dto);
        //Task<ServiceResponse<AuthResponseDto>> LoginAsyncOld(LoginDto dto);

        Task<ServiceResponse<AuthResponseDto>> RegisterAsync(RegisterDto dto);
        Task<ServiceResponse<AuthResponseDto>> CreateUserAsync(RegisterDto dto);
        Task<ServiceResponse> AssignRoleAsync(string userId, string role);
        Task<ServiceResponse> CreateRoleAsync(string roleName);
    }
}
