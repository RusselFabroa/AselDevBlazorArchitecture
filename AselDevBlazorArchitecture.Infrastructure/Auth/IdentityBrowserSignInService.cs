using AselDevBlazorArchitecture.Application.Common;
using AselDevBlazorArchitecture.Application.Features.Auth;
using AselDevBlazorArchitecture.Application.Features.Auth.DTOs;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace AselDevBlazorArchitecture.Infrastructure.Auth;

internal sealed class IdentityBrowserSignInService : IBrowserSignInService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;

    public IdentityBrowserSignInService(
        UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager)
    {
        _userManager = userManager;
        _signInManager = signInManager;
    }

    public async Task<ServiceResponse<UserSessionDto>> SignInAsync(LoginDto request)
    {
        var loginId = request.UsernameOrEmployeeId?.Trim() ?? string.Empty;
        var user = await _userManager.FindByNameAsync(loginId) ??
            await _userManager.Users.FirstOrDefaultAsync(candidate => candidate.EmployeeId == loginId);

        if (user is null || !user.IsActive)
            return Unauthorized();

        var result = await _signInManager.CheckPasswordSignInAsync(
            user,
            request.Password,
            lockoutOnFailure: true);

        if (result.IsLockedOut)
            return new ServiceResponse<UserSessionDto>("Account temporarily locked after repeated failed sign-in attempts.", 423);

        if (!result.Succeeded)
            return Unauthorized();

        await _signInManager.SignInAsync(user, isPersistent: false);
        var roles = await _userManager.GetRolesAsync(user);

        return new ServiceResponse<UserSessionDto>(
            new UserSessionDto(
                true,
                user.Id,
                user.FullName,
                user.Email ?? string.Empty,
                roles.ToArray()),
            "Sign-in successful.",
            200);
    }

    public Task SignOutAsync() => _signInManager.SignOutAsync();

    private static ServiceResponse<UserSessionDto> Unauthorized()
        => new("Invalid username/employee ID or password.", 401);
}
