namespace AselDevBlazorArchitecture.Application.Features.Users;

public sealed record UserSummaryDto(
    string Id,
    string EmployeeId,
    string FullName,
    string Email,
    string Department,
    string Role,
    bool IsActive);
