using AselDevBlazorArchitecture.Application.Common;

namespace AselDevBlazorArchitecture.Application.Features.Users;

public interface IUserAdministrationService
{
    Task<ServiceResponse<IReadOnlyList<string>>> GetRolesAsync(
        CancellationToken cancellationToken = default);

    Task<ServiceResponse<IReadOnlyList<UserSummaryDto>>> GetUsersAsync(
        CancellationToken cancellationToken = default);

    Task<ServiceResponse<UserSummaryDto>> CreateUserAsync(
        CreateUserRequest request,
        CancellationToken cancellationToken = default);
}
