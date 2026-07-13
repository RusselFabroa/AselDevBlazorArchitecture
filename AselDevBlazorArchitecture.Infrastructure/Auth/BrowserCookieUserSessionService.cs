using AselDevBlazorArchitecture.Application.Common;
using AselDevBlazorArchitecture.Application.Features.Auth;
using AselDevBlazorArchitecture.Application.Features.Auth.DTOs;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.JSInterop;
using System.Security.Claims;

namespace AselDevBlazorArchitecture.Infrastructure.Auth;

internal sealed class BrowserCookieUserSessionService : IUserSessionService
{
    private readonly AuthenticationStateProvider _authenticationStateProvider;
    private readonly IJSRuntime _js;

    public BrowserCookieUserSessionService(
        AuthenticationStateProvider authenticationStateProvider,
        IJSRuntime js)
    {
        _authenticationStateProvider = authenticationStateProvider;
        _js = js;
    }

    public async Task<UserSessionDto> GetCurrentAsync()
    {
        var state = await _authenticationStateProvider.GetAuthenticationStateAsync();
        var user = state.User;

        if (user.Identity?.IsAuthenticated != true)
            return UserSessionDto.Anonymous;

        return new UserSessionDto(
            true,
            user.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? string.Empty,
            user.Identity.Name ?? "User",
            user.FindFirst(ClaimTypes.Email)?.Value ?? string.Empty,
            user.FindAll(ClaimTypes.Role)
                .Select(claim => claim.Value)
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToArray());
    }

    public async Task<ServiceResponse<UserSessionDto>> SignInAsync(LoginDto request)
        => await _js.InvokeAsync<ServiceResponse<UserSessionDto>>(
            "aselSession.signIn",
            request);

    public async Task ClearAsync()
        => await _js.InvokeVoidAsync("aselSession.signOut");
}
