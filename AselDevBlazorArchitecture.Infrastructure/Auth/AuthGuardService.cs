using AselDevBlazorArchitecture.Application.Features.Auth;
using AselDevBlazorArchitecture.Application.Common.Interfaces.AuthServices;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AselDevBlazorArchitecture.Infrastructure.Auth
{
    public class AuthGuardService : IAuthGuardService
    {
        private readonly IUserSessionService _userSession;
        private readonly NavigationManager _navManager;
        private readonly ILogger<AuthGuardService> _logger;

        public AuthGuardService(
            IUserSessionService userSession,
            NavigationManager navManager,
            ILogger<AuthGuardService> logger)
        {
            _userSession = userSession;
            _navManager = navManager;
            _logger = logger;
        }

        public async Task<bool> EnsureAuthorizedAsync(
            List<string>? roles = null,
            string? urlReturn = null)
        {
            try
            {
                var session = await _userSession.GetCurrentAsync();
                if (!session.IsAuthenticated)
                {
                    _logger.LogWarning("AuthGuard — User not authenticated, redirecting to login");
                    var returnUrl = urlReturn ?? _navManager.Uri;
                    _navManager.NavigateTo(
                        $"/login?returnUrl={Uri.EscapeDataString(returnUrl)}");
                    return false;
                }

                if (roles != null && roles.Any())
                {
                    bool isInAnyRole = roles.Any(requiredRole =>
                        session.Roles.Contains(requiredRole.Trim(), StringComparer.OrdinalIgnoreCase));

                    if (!isInAnyRole)
                    {
                        _logger.LogWarning(
                            "AuthGuard — User {User} not in required roles: {Roles}",
                            session.DisplayName,
                            string.Join(", ", roles));

                        _navManager.NavigateTo("/unauthorized");
                        return false;
                    }
                }

                _logger.LogInformation(
                    "AuthGuard — Access granted: {User}",
                    session.DisplayName);

                return true; // ✅ Authorized
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "AuthGuard — Unexpected error");
                _navManager.NavigateTo("/login");
                return false;
            }
        }
    }
}
