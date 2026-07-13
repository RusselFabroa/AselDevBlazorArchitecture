using AselDevBlazorArchitecture.Application.Features.Auth;
using Microsoft.AspNetCore.WebUtilities;

namespace AselDevBlazorArchitecture.Web.Services
{
    public class SsoNavigationService
    {
        private readonly SsoSettings _settings;

        public SsoNavigationService(IConfiguration configuration)
        {
            _settings = configuration.GetSection("Sso").Get<SsoSettings>() ?? new SsoSettings();
        }

        public SsoSettings Settings => _settings;

        public string BuildLoginUrl(string returnUrl)
        {
            var loginUrl = string.IsNullOrWhiteSpace(_settings.LoginUrl)
                ? "/login"
                : _settings.LoginUrl;

            return QueryHelpers.AddQueryString(loginUrl, "urlReturn", returnUrl);
        }

        public string GetSafeReturnUrl(string? urlReturn, string? returnUrl)
        {
            var target = !string.IsNullOrWhiteSpace(urlReturn)
                ? urlReturn
                : returnUrl;

            if (string.IsNullOrWhiteSpace(target))
                return "/";

            if (target.StartsWith("//", StringComparison.Ordinal))
                return "/";

            if (target.StartsWith("/login", StringComparison.OrdinalIgnoreCase))
                return "/";

            if (Uri.TryCreate(target, UriKind.Absolute, out var absoluteUri))
            {
                if (!string.Equals(absoluteUri.Scheme, Uri.UriSchemeHttps, StringComparison.OrdinalIgnoreCase) &&
                    !string.Equals(absoluteUri.Scheme, Uri.UriSchemeHttp, StringComparison.OrdinalIgnoreCase))
                {
                    return "/";
                }

                return IsAllowedReturnHost(absoluteUri.Host)
                    ? absoluteUri.ToString()
                    : "/";
            }

            return target.StartsWith("/", StringComparison.Ordinal)
                ? target
                : $"/{target}";
        }

        private bool IsAllowedReturnHost(string host)
        {
            if (string.IsNullOrWhiteSpace(host))
                return false;

            return _settings.AllowedReturnHosts.Any(allowedHost =>
                string.Equals(allowedHost, host, StringComparison.OrdinalIgnoreCase));
        }
    }
}
