using AselDevBlazorArchitecture.Application.Common;
using AselDevBlazorArchitecture.Application.Features.Users;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace AselDevBlazorArchitecture.Infrastructure.Auth;

internal sealed class IdentityUserAdministrationService : IUserAdministrationService
{
    private static readonly string[] DefaultRoles = ["Admin", "Manager", "User", "Viewer"];

    private readonly UserManager<ApplicationUser> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;
    private readonly ILogger<IdentityUserAdministrationService> _logger;

    public IdentityUserAdministrationService(
        UserManager<ApplicationUser> userManager,
        RoleManager<IdentityRole> roleManager,
        ILogger<IdentityUserAdministrationService> logger)
    {
        _userManager = userManager;
        _roleManager = roleManager;
        _logger = logger;
    }

    public async Task<ServiceResponse<IReadOnlyList<string>>> GetRolesAsync(
        CancellationToken cancellationToken = default)
    {
        var roles = await _roleManager.Roles
            .AsNoTracking()
            .OrderBy(role => role.Name)
            .Select(role => role.Name!)
            .ToListAsync(cancellationToken);

        return new ServiceResponse<IReadOnlyList<string>>(
            roles.Count == 0 ? DefaultRoles : roles,
            statusCode: 200);
    }

    public async Task<ServiceResponse<IReadOnlyList<UserSummaryDto>>> GetUsersAsync(
        CancellationToken cancellationToken = default)
    {
        var users = await _userManager.Users
            .AsNoTracking()
            .OrderBy(user => user.FullName)
            .ToListAsync(cancellationToken);

        var summaries = new List<UserSummaryDto>(users.Count);
        foreach (var user in users)
        {
            var roles = await _userManager.GetRolesAsync(user);
            summaries.Add(ToSummary(user, roles.FirstOrDefault() ?? "User"));
        }

        return new ServiceResponse<IReadOnlyList<UserSummaryDto>>(summaries, statusCode: 200);
    }

    public async Task<ServiceResponse<UserSummaryDto>> CreateUserAsync(
        CreateUserRequest request,
        CancellationToken cancellationToken = default)
    {
        var employeeId = request.EmployeeId.Trim();
        var email = request.Email.Trim();
        var role = request.Role.Trim();

        if (string.IsNullOrWhiteSpace(employeeId) || string.IsNullOrWhiteSpace(email))
            return new ServiceResponse<UserSummaryDto>("Employee ID and email are required.", 400);

        if (await _userManager.FindByNameAsync(employeeId) is not null)
            return new ServiceResponse<UserSummaryDto>("Employee ID is already registered.", 409);

        if (await _userManager.FindByEmailAsync(email) is not null)
            return new ServiceResponse<UserSummaryDto>("Email is already registered.", 409);

        if (!await _roleManager.RoleExistsAsync(role))
            return new ServiceResponse<UserSummaryDto>("The selected role is not available.", 400);

        var user = new ApplicationUser
        {
            UserName = employeeId,
            EmployeeId = employeeId,
            Email = email,
            FullName = request.FullName.Trim(),
            Department = request.Department.Trim(),
            IsActive = true,
            EmailConfirmed = true,
            CreatedAt = DateTime.UtcNow
        };

        var result = await _userManager.CreateAsync(user, request.Password);
        if (!result.Succeeded)
        {
            var message = string.Join(" ", result.Errors.Select(error => error.Description));
            return new ServiceResponse<UserSummaryDto>(message, 400);
        }

        var roleResult = await _userManager.AddToRoleAsync(user, role);
        if (!roleResult.Succeeded)
        {
            await _userManager.DeleteAsync(user);
            _logger.LogWarning("Rolled back user {EmployeeId} after role assignment failed.", employeeId);
            return new ServiceResponse<UserSummaryDto>("User role assignment failed.", 500);
        }

        _logger.LogInformation("Administrator created user {EmployeeId} with role {Role}.", employeeId, role);
        return new ServiceResponse<UserSummaryDto>(
            ToSummary(user, role),
            "User created successfully.",
            201);
    }

    private static UserSummaryDto ToSummary(ApplicationUser user, string role)
        => new(
            user.Id,
            user.EmployeeId,
            user.FullName,
            user.Email ?? string.Empty,
            user.Department,
            role,
            user.IsActive);
}
