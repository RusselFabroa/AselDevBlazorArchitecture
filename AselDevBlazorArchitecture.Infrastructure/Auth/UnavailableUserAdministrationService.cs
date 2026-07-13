using AselDevBlazorArchitecture.Application.Common;
using AselDevBlazorArchitecture.Application.Features.Users;

namespace AselDevBlazorArchitecture.Infrastructure.Auth;

internal sealed class UnavailableUserAdministrationService : IUserAdministrationService
{
    private const string Message =
        "User administration is unavailable because database persistence is disabled.";

    public Task<ServiceResponse<IReadOnlyList<string>>> GetRolesAsync(
        CancellationToken cancellationToken = default)
        => Unavailable<IReadOnlyList<string>>();

    public Task<ServiceResponse<IReadOnlyList<UserSummaryDto>>> GetUsersAsync(
        CancellationToken cancellationToken = default)
        => Unavailable<IReadOnlyList<UserSummaryDto>>();

    public Task<ServiceResponse<UserSummaryDto>> CreateUserAsync(
        CreateUserRequest request,
        CancellationToken cancellationToken = default)
        => Unavailable<UserSummaryDto>();

    private static Task<ServiceResponse<T>> Unavailable<T>()
        => Task.FromResult(new ServiceResponse<T>(Message, 503));
}
