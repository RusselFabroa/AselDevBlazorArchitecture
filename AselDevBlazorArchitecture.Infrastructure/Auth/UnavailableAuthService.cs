using AselDevBlazorArchitecture.Application.Common;
using AselDevBlazorArchitecture.Application.Common.Interfaces.AuthServices;
using AselDevBlazorArchitecture.Application.Features.Auth.DTOs;

namespace AselDevBlazorArchitecture.Infrastructure.Auth;

internal sealed class UnavailableAuthService : IAuthService
{
    private const string Message =
        "Database-backed authentication is not enabled for this application.";

    public Task<ServiceResponse<AuthResponseDto>> LoginAsync(LoginDto dto)
        => Unavailable<AuthResponseDto>();

    public Task<ServiceResponse<AuthResponseDto>> RegisterAsync(RegisterDto dto)
        => Unavailable<AuthResponseDto>();

    public Task<ServiceResponse<AuthResponseDto>> CreateUserAsync(RegisterDto dto)
        => Unavailable<AuthResponseDto>();

    public Task<ServiceResponse> AssignRoleAsync(string userId, string role)
        => Task.FromResult(ServiceResponse.Error(Message, 503));

    public Task<ServiceResponse> CreateRoleAsync(string roleName)
        => Task.FromResult(ServiceResponse.Error(Message, 503));

    private static Task<ServiceResponse<T>> Unavailable<T>()
        => Task.FromResult(new ServiceResponse<T>(Message, 503));
}
