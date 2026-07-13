using AselDevBlazorArchitecture.Application.Common;
using AselDevBlazorArchitecture.Application.Features.Auth;
using AselDevBlazorArchitecture.Application.Features.Auth.DTOs;

namespace AselDevBlazorArchitecture.Infrastructure.Auth;

internal sealed class UnavailableBrowserSignInService : IBrowserSignInService
{
    public Task<ServiceResponse<UserSessionDto>> SignInAsync(LoginDto request)
        => Task.FromResult(new ServiceResponse<UserSessionDto>(
            "Database-backed authentication is not enabled for this application.",
            503));

    public Task SignOutAsync() => Task.CompletedTask;
}
