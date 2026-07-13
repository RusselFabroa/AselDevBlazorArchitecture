namespace AselDevBlazorArchitecture.Application.Features.Auth
{
    public class SsoSettings
    {
        public string Mode { get; set; } = "IdentityProvider";
        public string Authority { get; set; } = string.Empty;
        public string LoginUrl { get; set; } = "/login";
        public string UserInfoUrl { get; set; } = "/api/sso/me";
        public List<string> AllowedReturnHosts { get; set; } = new();

        public bool IsIdentityProvider =>
            string.Equals(Mode, "IdentityProvider", StringComparison.OrdinalIgnoreCase);

        public bool IsClient =>
            string.Equals(Mode, "Client", StringComparison.OrdinalIgnoreCase);
    }
}
