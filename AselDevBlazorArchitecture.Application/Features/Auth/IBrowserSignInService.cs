using AselDevBlazorArchitecture.Application.Common;
using AselDevBlazorArchitecture.Application.Features.Auth.DTOs;

namespace AselDevBlazorArchitecture.Application.Features.Auth;

public interface IBrowserSignInService
{
    Task<ServiceResponse<UserSessionDto>> SignInAsync(LoginDto request);
    Task SignOutAsync();
}
